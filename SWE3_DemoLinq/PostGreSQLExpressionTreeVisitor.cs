using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Linq
{
    public class PostGreSqlExpressionTreeVisitor : ExpressionTreeVisitor
    {
        private readonly StringBuilder _postGreSqlString = new StringBuilder();
        private bool _firstWhereVisited = false;

        public Type SourceType { get; private set; }

        public override Expression Visit(Expression e)
        {
            return base.Visit(e);
        }

        //.Where, .Select usw usw
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            string append = "";
            if (m.Method.Name == "Where")
            {
                
                if (m.Method.DeclaringType == typeof(Queryable) && _firstWhereVisited && m.Arguments[0].NodeType == ExpressionType.Call)
                {
                    append = " AND ";
                }

                _firstWhereVisited = true;
                
                if (m.Arguments[0].NodeType == ExpressionType.Constant)
                {
                    var ce = (ConstantExpression)m.Arguments[0];
                    var t = ce.Type;
                    if (t.GetGenericTypeDefinition() == typeof(DemoLinq<>))
                    {
                        var tt = t.GenericTypeArguments[0];
                        if (tt != SourceType && SourceType != null)
                        {
                            throw new InvalidOperationException("Mixing tables not allowed");
                        }
                        else
                        {
                            this.SourceType = tt;
                        }
                    }
                _postGreSqlString.Append("WHERE ");
                }
            }
            Expression x = base.VisitMethodCall(m);
            _postGreSqlString.Append(append);
            return x;
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            Expression x = base.VisitLambda(lambda);
            if (lambda.Body.NodeType == ExpressionType.AndAlso)
            {
                _postGreSqlString.Append(" " + NodeTypeToString(lambda.Body.NodeType) + " ");
            }
            return x;
        }


        protected override Expression VisitConstant(ConstantExpression c)
        {
            _postGreSqlString.Append(GetPostGreSqlValue(c.Type, c.Value));

            return base.VisitConstant(c);
        }

        protected override ParameterExpression VisitParameter(ParameterExpression p)
        {
            return base.VisitParameter(p);
        }

        //Es kann sich hier "nur" um eine Spalte oder eine Konstante halten
        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression.NodeType == ExpressionType.Constant || m.Expression.NodeType == ExpressionType.MemberAccess)
            {
                object value;
                var ce = (ConstantExpression)Visit(m.Expression);

                if (m.Type.IsPrimitive || m.Type == typeof(string) || m.Type == typeof(DateTime))
                {
                    var fi = m.Member.DeclaringType.GetField(m.Member.Name);
                    var pi = m.Member.DeclaringType.GetProperty(m.Member.Name);
                    if (fi != null)
                    {
                        _postGreSqlString.Append(GetPostGreSqlValue(m.Type, fi.GetValue(ce.Value)));
                        value = fi.GetValue(ce.Value);
                    }
                    else if (pi != null)
                    {
                        _postGreSqlString.Append(GetPostGreSqlValue(m.Type, pi.GetValue(ce.Value)));
                        value = pi.GetValue(ce.Value);
                    }
                    else throw new NotSupportedException();
                }
                else if (m.Member.MemberType == MemberTypes.Field)
                {
                    value = GetFieldValue(Expression.MakeMemberAccess(ce, m.Member));
                }
                else if (m.Member.MemberType == MemberTypes.Property)
                {
                    value = GetPropertyValue(Expression.MakeMemberAccess(ce, m.Member));
                }
                else
                {
                    throw new NotSupportedException();
                }
                return Expression.Constant(value, m.Type);

            }
            else
            {
                _postGreSqlString.Append(SourceType.Name.ToLower() + "." +  m.Member.Name.ToLower());
            }
            return base.VisitMemberAccess(m);
        }
        

        protected override Expression VisitBinary(BinaryExpression b)
        {
            _postGreSqlString.Append("(");
            Visit(b.Left);
            _postGreSqlString.Append($" {NodeTypeToString(b.NodeType)} ");
            Visit(b.Right);
            _postGreSqlString.Append(")");
            return b;
        }

        /// <summary>
        /// before returning the saved string the class resets its values to be ready for the next Expression
        /// </summary>
        /// <returns>the Where part of a PostGreSql select statement as a string</returns>
        public string GetStatement()
        {
            //Reset Class and return string
            string ret = _postGreSqlString.ToString();
            _postGreSqlString.Clear();
            _firstWhereVisited = false;
            SourceType = null;
            return ret;
        }

        private String NodeTypeToString (ExpressionType e)
        {
            switch (e)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
            }

            return "InvalidNodeTypeToString";
        }

        private static object GetFieldValue(MemberExpression node)
        {
            var fieldInfo = (FieldInfo)node.Member;

            var instance = (node.Expression == null) ? null : TryEvaluate(node.Expression).Value;

            return fieldInfo.GetValue(instance);
        }

        private static object GetPropertyValue(MemberExpression node)
        {
            var propertyInfo = (PropertyInfo)node.Member;

            var instance = (node.Expression == null) ? null : TryEvaluate(node.Expression).Value;

            return propertyInfo.GetValue(instance, null);
        }

        private static ConstantExpression TryEvaluate(Expression expression)
        {

            if (expression.NodeType == ExpressionType.Constant)
            {
                return (ConstantExpression)expression;
            }
            throw new NotSupportedException();

        }

        private static string GetPostGreSqlValue(Type type, object c)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments().First();
            }
            if (type == typeof(int) || type == typeof(bool) || type == typeof(short) || type == typeof(long) || type == typeof(decimal))
            {
                return c.ToString();
            }
            if (type == typeof(string)) return "\'" + c + "\'";
            if (type == typeof(DateTime)) return "\'" + ((DateTime)c).Year + "-" + ((DateTime)c).Month + "-" + ((DateTime)c).Day + "\'";
            return "";
        }

    }
}

