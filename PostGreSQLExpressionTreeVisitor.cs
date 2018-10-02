using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Linq
{
    class PostGreSQLExpressionTreeVisitor : ExpressionTreeVisitor
    {
        private StringBuilder _postGreSQLString = new StringBuilder();

        public Type SourceType { get; private set; }

        public override Expression Visit(Expression e)
        {
            return base.Visit(e);
        }

        //.Where, .Select usw usw
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.Name == "Where")
            {
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
                _postGreSQLString.Append("WHERE ");
                }
            }
            Console.WriteLine(m.Method.Name);
            return base.VisitMethodCall(m);
        }


        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.Equal)
            {
                _postGreSQLString.Append(" AND ");
            }
            Expression x = base.VisitLambda(lambda);
            if (lambda.Body.NodeType == ExpressionType.AndAlso)
            {
                _postGreSQLString.Append(" " + NodeTypeToString(lambda.Body.NodeType) + " ");
            }
            return x;
        }


        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (isSemiPrimitiveType(c.Type))
            {
                _postGreSQLString.Append(c.Value);
            }
            
            return base.VisitConstant(c);
        }

        protected override ParameterExpression VisitParameter(ParameterExpression p)
        {
            return base.VisitParameter(p);
        }

        //Es kann sich hier "nur" um eine Spalte oder eine Konstante halten
        protected override Expression VisitMemberAccess(MemberExpression e)
        {
            _postGreSQLString.Append(e.Member.Name);
            return base.VisitMemberAccess(e);
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            _postGreSQLString.Append("(");
            Visit(b.Left);
            _postGreSQLString.Append($" {NodeTypeToString(b.NodeType)} ");
            Visit(b.Right);
            _postGreSQLString.Append(")");
            return b;
        }

        public void printSQL()
        {
            Console.WriteLine(_postGreSQLString.ToString());
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

        private string Member(string member)
        {
            //ToDo: Performance verbessern:
            return SourceType.Name + "." +  member;
        }

        private bool isSemiPrimitiveType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments().First();
            }
            if (type == typeof(int)) return true;
            if (type == typeof(bool)) return true;
            if (type == typeof(short)) return true;
            if (type == typeof(long)) return true;
            if (type == typeof(decimal)) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(DateTime)) return true;

            return false;
        }
    }
}

