using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Linq
{
    public interface IDatabase
    {
        IEnumerable<T> Select<T>(Expression expression);

        int Insert<T>(T newObject);

        void Delete<T>(T deletedObject);

        void Update<T>(T changedObject);
    }
}
