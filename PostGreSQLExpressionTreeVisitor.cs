using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Linq
{
    class PostGreSqlExpressionTreeVisitor : ExpressionTreeVisitor
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
            Type type = c.Type;
            if (c.Type.IsGenericType && c.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = c.Type.GetGenericArguments().First();
            }
            if (type == typeof(int) || type == typeof(bool) || type == typeof(short) || type == typeof(long) || type == typeof(decimal))
            {
                _postGreSqlString.Append(c.Value);
            }
            if (type == typeof(string)) _postGreSqlString.Append("\'"+c.Value+"\'");
            if (type == typeof(DateTime)) _postGreSqlString.Append("\'" + ((DateTime)c.Value).Year + "-" + ((DateTime)c.Value).Month + "-" + ((DateTime)c.Value).Day + "\'");
            
            return base.VisitConstant(c);
        }

        

        protected override ParameterExpression VisitParameter(ParameterExpression p)
        {
            return base.VisitParameter(p);
        }

        //Es kann sich hier "nur" um eine Spalte oder eine Konstante halten
        protected override Expression VisitMemberAccess(MemberExpression e)
        {
            if (e.Expression.NodeType == ExpressionType.Constant)
            {
                var ce = (ConstantExpression) e.Expression;
                var fi = ce.Type.GetField(e.Member.Name);
                _postGreSqlString.Append(fi.GetValue(ce.Value));

            }
            else
            {
                _postGreSqlString.Append(SourceType.Name.ToLower() + "." +  e.Member.Name.ToLower());
            }
            return base.VisitMemberAccess(e);
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

        public string GetStatement()
        {
            return _postGreSqlString.ToString();
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

    }
}

