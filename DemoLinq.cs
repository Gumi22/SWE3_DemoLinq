using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Transactions;
using Npgsql;

namespace Linq
{
    public class DemoLinq<T> : IQueryable<T>
    {
        private Expression _expression = null;
        private DemoLinqProvider _provider = null;
        public DemoLinq() : this(new OrMapper())
        {
        }

        public DemoLinq(OrMapper orMapper)
        {
            _expression = Expression.Constant(this);
            _provider = new DemoLinqProvider(orMapper);
        }

        internal DemoLinq(DemoLinqProvider provider, Expression expression)
        {
            _expression = expression;
            _provider = provider;
        }

        public Type ElementType => typeof(T);

        public Expression Expression => _expression;

        public IQueryProvider Provider => _provider;

        public IEnumerator<T> GetEnumerator()
        {
            // Returns a enumeration (ToList, ToArray, foreach, ...)
            return _provider.GetEnumerator<T>(_expression);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // Returns a enumeration (ToList, ToArray, foreach, ...)
            return _provider.GetEnumerator<T>(_expression);
        }
    }

    internal class DemoLinqProvider : IQueryProvider
    {
        private OrMapper _orMapper;

        public DemoLinqProvider(OrMapper orMapper)
        {
            _orMapper = orMapper;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new DemoLinq<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            // returns a single object (First, Single, etc)
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            // returns a single object (First, Single, etc)
            throw new NotImplementedException();
        }

        internal IEnumerator<T> GetEnumerator<T>(Expression expression)
        {
            return _orMapper.GetEnumerable<T>(expression)
                .AsQueryable()
                .GetEnumerator(); ;
        }

        


    }
}
