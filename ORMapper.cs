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
        private readonly IChangeTracker _ct;
        private readonly IDatabase _db;

        public OrMapper() : this(new PostGreSqlDatabase(), new ChangeTracker())
        {
        }

        public OrMapper(IDatabase db, IChangeTracker ct)
        {
            _ct = ct;
            _db = db;
        }

        /// <summary>
        /// Gets a queryable Object (DemoLinq)
        /// </summary>
        /// <typeparam name="T">A Table Type that is stored in the Database</typeparam>
        /// <returns>A queryable object of Type T</returns>
        public IQueryable<T> GetQuery<T>()
        {
            if (IsTypeATable(typeof(T)))
            {
                return new DemoLinq<T>(this);
            }
            else
            {
                throw new InvalidOperationException(
                    $"{typeof(T)} is not a valid Table Type, please set correct Attributes");
            }
        }

        /// <summary>
        /// Executes a Select statement defined by the expression on a database and returns the results
        /// </summary>
        /// <typeparam name="T">The Type of the table, must have Table Attribute</typeparam>
        /// <param name="expression">A Linq Expression on a certain Table</param>
        /// <returns>A List of objects of Type T </returns>
        public IEnumerable<T> GetEnumerable<T>(Expression expression)
        {
            if (IsTypeATable(typeof(T)))
            {
                IEnumerable<T> list = _db.Select<T>(expression);
                var enumerable = list.ToList();
                foreach (var obj in enumerable)
                {
                    _ct.Track(obj);
                }

                return enumerable;
            }
            else
            {
                throw new InvalidOperationException(
                    $"{typeof(T)} is not a valid Table Type, please set correct Attributes");
            }
        }

        /// <summary>
        /// Inserts an object into the database
        /// </summary>
        /// <param name="obj">an valid table type object that should be stored in db</param>
        public void Insert(object obj)
        {
            if (IsTypeATable(obj.GetType()))
            {
                _ct.Insert(obj);
            }
            else
            {
                throw new InvalidOperationException(
                    $"{obj.GetType()} is not a valid Table Type, please set correct Attributes");
            }
        }

        /// <summary>
        /// Deletes an object fro the database.
        /// </summary>
        /// <param name="obj">an valid table type object that should be stored in db</param>
        public void Delete(object obj)
        {
            if (IsTypeATable(obj.GetType()))
            {
                _ct.Delete(obj);
            }
            else
            {
                throw new InvalidOperationException(
                    $"{obj.GetType()} is not a valid Table Type, please set correct Attributes");
            }
        }

        /// <summary>
        /// Actually Submits the cached changes into the database all at once.
        /// </summary>
        public void SubmitChanges()
        {
            //ToDo: eventually insert first, then delete, then update
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

        /// <summary>
        /// Checks if Type is a valid type, that can be copied to the Database
        /// </summary>
        /// <param name="T">Type that should be checked</param>
        /// <returns>true if Attributes are set correctly, false if there are missing or no attributes</returns>
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