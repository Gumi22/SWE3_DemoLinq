using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Linq
{
    class PostGreSQLExpressionTreeVisitor : ExpressionTreeVisitor
    {
        public Type SourceType { get; private set; }
        StringBuilder _postGreSQLString = new StringBuilder();

        private bool _startedSelect = false;

        public override Expression Visit(Expression e)
        {
            if (e != null)
            {
                //Console.WriteLine(e.NodeType + "!!!!");
            }
            return base.Visit(e);
        }

        //.Where, .Select usw usw
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.Name == "Where" && m.Arguments[0].NodeType == ExpressionType.Constant)
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
                    else if (tt == SourceType && SourceType != null)
                    {
                        _postGreSQLString.Append("AND");
                    }
                    else
                    {
                        this.SourceType = tt;
                        _postGreSQLString.Append("WHERE ");
                        _startedSelect = true;
                    }
                }
            }
            return base.VisitMethodCall(m);
        }
        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            //Look for second 
            if (_startedSelect)
            {
                _postGreSQLString.Append(" AND ");
            }
            return base.VisitLambda(lambda);
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
            //_postGreSQLString.Append("Param: " + p. + " <-------");
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
            string s = "";

            //ToDo: Make switch case;

            if (e.Equals(ExpressionType.Add)){
                s = "+";
            }
            else if (e.Equals(ExpressionType.Subtract))
            {
                s = "-";
            }
            else if(e.Equals(ExpressionType.And))
            {
                s = "AND";
            }
            else if(e.Equals(ExpressionType.AndAlso))
            {
                s = "AND";
            }
            else if(e.Equals(ExpressionType.Or))
            {
                s = "OR";
            }
            else if(e.Equals(ExpressionType.OrElse))
            {
                s = "OR";
            }
            else if(e.Equals(ExpressionType.LessThan))
            {
                s = "<";
            }
            else if(e.Equals(ExpressionType.LessThanOrEqual))
            {
                s = "<=";
            }
            else if(e.Equals(ExpressionType.GreaterThan))
            {
                s = ">";
            }
            else if(e.Equals(ExpressionType.GreaterThanOrEqual))
            {
                s = ">=";
            }
            else if(e.Equals(ExpressionType.Equal))
            {
                s = "=";
            }
            else if (e.Equals(ExpressionType.NotEqual))
            {
                s = "<>";
            }

            return s;
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

