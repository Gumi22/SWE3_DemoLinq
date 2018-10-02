using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Linq
{
    public class DemoExpressionTreeVisitor : ExpressionTreeVisitor
    {
        private Type TableType;

        public override Expression Visit(Expression e)
        {
            if (e != null)
            {
                // Console.WriteLine(e.NodeType);
            }
            return base.Visit(e);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if(m.Method.Name == "Where" && m.Arguments[0].NodeType == ExpressionType.Constant)
            {
                var ce = (ConstantExpression)m.Arguments[0];
                var t = ce.Type;
                if(t.GetGenericTypeDefinition() == typeof(DemoLinq<>))
                {
                    var tt = t.GenericTypeArguments[0];
                    if(tt != TableType && TableType != null)
                    {
                        throw new InvalidOperationException("Mixing tables not allowed");
                    }
                    else
                    {
                        this.TableType = tt;
                    }
                }
            }
            return base.VisitMethodCall(m);
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            // Console.WriteLine($"  Constant = {c.Value}");
            return base.VisitConstant(c);
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            Console.WriteLine($"  {Visit(b.Left)} {b.NodeType} {Visit(b.Right)}");
            return b;
            // return base.VisitBinary(b);
        }
    }
}
