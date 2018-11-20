using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Linq.Attributes;

namespace Linq
{
    public class OrMapper
    {
        private readonly ChangeTracker _ct;
        private readonly IDatabase _db;

        public OrMapper() : this(new PostGreSqlDatabase())
        {
        }

        public OrMapper(IDatabase db)
        {
            _ct = new ChangeTracker();
            _db = db;
        }

        public IQueryable<T> GetQuery<T>()
        {
            if(IsTypeATable(typeof(T)))
            {
                return new DemoLinq<T>();
            }
            else
            {
                throw new InvalidOperationException($"{typeof(T)} is not a valid Table Type, please set correct Attributes");
            }
        }

        public IEnumerable<T> GetEnumerable<T>(Expression expression)
        {
            IEnumerable <T> list = _db.Select<T>(expression);
            var enumerable = list.ToList();
            foreach (var obj in enumerable)
            {
                _ct.Track(obj);
            }
            return enumerable;
        }

        public void Insert(object obj)
        {
            if (IsTypeATable(obj.GetType()))
            {
                _ct.Insert(obj);
            }
            else
            {
                throw new InvalidOperationException($"{obj.GetType()} is not a valid Table Type, please set correct Attributes");
            }
        }

        public void Delete(object obj)
        {
            if (IsTypeATable(obj.GetType()))
            {
                _ct.Delete(obj);
            }
            else
            {
                throw new InvalidOperationException($"{obj.GetType()} is not a valid Table Type, please set correct Attributes");
            }
        }

        public void SubmitChanges()
        {
            List<ChangeTrackerEntry> changedEntries = _ct.DetectChanges();
            foreach (var change in changedEntries)
            {
                switch (change.State)
                {
                    case ChangeTrackerEntry.States.Modified:
                        ExecuteUpdate(change);
                        break;
                    case ChangeTrackerEntry.States.Added:
                        ExecuteInsert(change);
                        break;
                    case ChangeTrackerEntry.States.Deleted:
                        ExecuteDelete(change);
                        break;
                }
            }
            //set ChangeTrackerEntries to unmodified and delete old ones
            _ct.ComputeChanges();
        }

        private void ExecuteUpdate(ChangeTrackerEntry entry)
        {
            _db.Update(entry.Item);
        }

        private void ExecuteInsert(ChangeTrackerEntry entry)
        {
            _db.Insert(entry.Item);

        }

        private void ExecuteDelete(ChangeTrackerEntry entry)
        {
            _db.Delete(entry.Item);
        }

        public bool IsTypeATable(Type T)
        {
            if (T.GetCustomAttributes(typeof(TableAttribute)).GetEnumerator().MoveNext())
            {
                foreach (var prop in T.GetProperties())
                {
                    //if there table with a property with at least one PK and one Column attribute, it is a valid table 
                    if (prop.GetCustomAttributes<ColumnAttribute>().GetEnumerator().MoveNext() &&
                        prop.GetCustomAttributes<PrimaryKeyAttribute>().GetEnumerator().MoveNext())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
