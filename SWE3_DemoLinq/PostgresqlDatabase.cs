using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Linq.Attributes;
using Npgsql;

namespace Linq
{
    public class PostGreSqlDatabase : IDatabase
    {
        private readonly string _connectionString = "";
        private readonly PostGreSqlExpressionTreeVisitor _visitor = null;

        public PostGreSqlDatabase(string connection = "Server=127.0.0.1; Port=5432; User Id=postgres; Password=postgres; Database=imgDB")
        {
            _connectionString = connection;
            _visitor = new PostGreSqlExpressionTreeVisitor();
        }

        public PostGreSqlDatabase(PostGreSqlExpressionTreeVisitor visitor, string connection = "Server=127.0.0.1; Port=5432; User Id=postgres; Password=postgres; Database=imgDB")
        {
            _connectionString = connection;
            _visitor = visitor;
        }

        /// <summary>
        /// Converts an expression into a PostGreSql select statement, executes it against the database
        /// and returns the Result as an enumerable
        /// </summary>
        /// <typeparam name="T">the Table Type</typeparam>
        /// <param name="expression">A valid Linq expression without joins or complicated things</param>
        /// <returns></returns>
        public IEnumerable<T> Select<T>(Expression expression)
        {
            return ExecuteSelect<T>(BuildSqlSelectString(expression));
        }

        /// <summary>
        /// Builds a PostGreSql insert statement and inserts the object into the database
        /// </summary>
        /// <typeparam name="T">the Table Type</typeparam>
        /// <param name="newObject">the objects that is to be inserted</param>
        /// <returns>the id of the newly inserted object</returns>
        public int Insert<T>(T newObject)
        {
            int id = ExecuteInsert(BuildSqlInsertString(newObject));

            foreach (var prop in newObject.GetType().GetProperties())
            {
                foreach (var unused in prop.GetCustomAttributes(typeof(PrimaryKeyAttribute)))
                {
                    prop.SetValue(newObject, id);
                }
            }

            return id;
        }

        /// <summary>
        /// Builds a PostGreSql delete statement and deletes the object from the database
        /// </summary>
        /// <typeparam name="T">the Table Type</typeparam>
        /// <param name="deletedObject">the objects that is to be deleted</param>
        public void Delete<T>(T deletedObject)
        {
            ExecuteDelete(BuildSqlDeleteString(deletedObject));
        }

        /// <summary>
        /// Builds a PostGreSql update statement and updates the object in the database
        /// </summary>
        /// <typeparam name="T">the Table Type</typeparam>
        /// <param name="changedObject">the objects that is to be updated</param>
        public void Update<T>(T changedObject)
        {
            ExecuteUpdate(BuildSqlUpdateString(changedObject));
        }


        //ToDo: Refactor the stringbuilder methods with possibly new Class that gets attributes and values and so on.
        private string BuildSqlSelectString(Expression ex)
        {
            _visitor.Visit(ex);

            var tableAttributes = _visitor.SourceType.GetCustomAttributes<TableAttribute>().GetEnumerator();
            tableAttributes.MoveNext();
            var tableName = tableAttributes.Current.Name;
            tableAttributes.Dispose();

            string select = "SELECT ";

            //Select * but with names to control the order:
            foreach (var property in _visitor.SourceType.GetProperties())
            {
                foreach (var attribute in property.GetCustomAttributes<ColumnAttribute>())
                {
                    select += $"{tableName}.{attribute.Name}, ";
                }
            }

            select = select.Remove(select.LastIndexOf(',')) + " ";

            //from my table
            select += $"FROM {tableName} ";

            //where blabla
            select += _visitor.GetStatement() + ";";

            return select;
        }

        private string BuildSqlInsertString(object obj)
        {
            //ToDo: Refactor this :D
            Type T = obj.GetType();
            StringBuilder insert = new StringBuilder("INSERT INTO ").Append(T.Name).Append(" (");
            StringBuilder values = new StringBuilder("VALUES (");
            string pkName = "";

            foreach (var property in T.GetProperties())
            {
                bool isPK = false;
                string columnName = "";
                foreach (var attribute in property.GetCustomAttributes<ColumnAttribute>())
                {
                    columnName = attribute.Name;
                }
                foreach (var attribute in property.GetCustomAttributes<PrimaryKeyAttribute>())
                {
                    isPK = true;
                    pkName = columnName;
                }

                if (!String.IsNullOrEmpty(columnName) && !isPK)
                {
                    if(property.GetValue(obj) != null)
                    {
                        insert.Append(columnName).Append(",");
                        if (typeof(DateTime) == property.PropertyType || typeof(DateTime?) == property.PropertyType)
                        {
                            DateTime dt = (DateTime) property.GetValue(obj);
                            values.Append($"'{dt.Year}-{dt.Month}-{dt.Day}',");
                        }else if (typeof(string) == property.PropertyType)
                        {
                            values.Append($"'{property.GetValue(obj)}',");
                        }else if (typeof(int) == property.PropertyType || typeof(int?) == property.PropertyType)
                        {
                            values.Append($"{property.GetValue(obj)},");
                        }
                        else if (typeof(double) == property.PropertyType || typeof(double?) == property.PropertyType ||
                                  typeof(float) == property.PropertyType || typeof(float?) == property.PropertyType ||
                                  typeof(decimal) == property.PropertyType || typeof(decimal?) == property.PropertyType)
                        {
                            values.Append($"{property.GetValue(obj).ToString().Replace(',', '.')},");
                        }
                        else
                        {
                            throw new InvalidOperationException($"Type {property.PropertyType} not supported with PostgresqlDatabase");
                        }
                    }
                }
            }

            insert.Remove(insert.Length - 1, 1); //remove the last colon
            insert.Append(") ");
            values.Remove(values.Length - 1, 1);
            values.Append($") RETURNING {pkName};");

            return insert.Append(values.ToString()).ToString();
        }

        private string BuildSqlUpdateString(object obj)
        {
            //ToDo: Refactor this :D
            Type T = obj.GetType();
            StringBuilder update = new StringBuilder("UPDATE ").Append(T.Name).Append(" SET ");
            string pkName = "";
            int pkValue = -1;

            foreach (var property in T.GetProperties())
            {
                bool isPK = false;
                string columnName = "";
                foreach (var attribute in property.GetCustomAttributes<ColumnAttribute>())
                {
                    columnName = attribute.Name;
                }
                foreach (var attribute in property.GetCustomAttributes<PrimaryKeyAttribute>())
                {
                    isPK = true;
                    pkName = columnName;
                    pkValue = (int)property.GetValue(obj);
                }

                if (!String.IsNullOrEmpty(columnName) && !isPK)
                {
                    if (property.GetValue(obj) != null)
                    {
                        update.Append(columnName).Append("=");
                        if (typeof(DateTime) == property.PropertyType || typeof(DateTime?) == property.PropertyType)
                        {
                            DateTime dt = (DateTime)property.GetValue(obj);
                            update.Append($"'{dt.Year}-{dt.Month}-{dt.Day}',");
                        }
                        else if (typeof(string) == property.PropertyType)
                        {
                            update.Append($"'{property.GetValue(obj)}',");
                        }
                        else if (typeof(int) == property.PropertyType || typeof(int?) == property.PropertyType)
                        {
                            update.Append($"{property.GetValue(obj)},");
                        }
                        else if (typeof(double) == property.PropertyType || typeof(double?) == property.PropertyType ||
                                  typeof(float) == property.PropertyType || typeof(float?) == property.PropertyType ||
                                  typeof(decimal) == property.PropertyType || typeof(decimal?) == property.PropertyType)
                        {
                            update.Append($"{property.GetValue(obj).ToString().Replace(',', '.')},");
                        }
                        else
                        {
                            throw new InvalidOperationException($"Type {property.PropertyType} not supported with PostgresqlDatabase");
                        }
                    }
                    else
                    {
                        update.Append(columnName).Append("=NULL,");
                    }
                }
            }

            update.Remove(update.Length - 1, 1); //remove the last colon
            update.Append($" WHERE {pkName}={pkValue};");

            return update.ToString();
        }

        private string BuildSqlDeleteString(object obj)
        {
            //ToDo: Refactor this :D
            Type T = obj.GetType();
            StringBuilder delete = new StringBuilder("DELETE FROM ").Append(T.Name);
            string pkName = "";
            int pkValue = -1;

            foreach (var property in T.GetProperties())
            {
                string columnName = "";
                foreach (var attribute in property.GetCustomAttributes<ColumnAttribute>())
                {
                    columnName = attribute.Name;
                }
                foreach (var attribute in property.GetCustomAttributes<PrimaryKeyAttribute>())
                {
                    pkName = columnName;
                    pkValue = (int)property.GetValue(obj);
                }
            }

            if (pkValue < 0)
            {
                throw new ArgumentException("object does not have valid primary key, key should be >=1, but is " + pkValue);
            }
            delete.Append($" WHERE {pkName}={pkValue};");

            return delete.ToString();
        }

        private List<T> ExecuteSelect<T>(string select)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(select, con))
                {
                    cmd.Connection?.Open();
                    List<T> ret;
                    using (IDataReader idr = cmd.ExecuteReader())
                    {
                        ret = (PopulateEntities<T>(idr, cmd));
                    }
                    cmd.Connection?.Close();
                    return ret;
                }
            }
        }

        private int ExecuteInsert(string insert)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(insert, con))
                {
                    cmd.Connection?.Open();
                    int ret = -1;
                    using (IDataReader idr = cmd.ExecuteReader())
                    {
                        if (idr.Read() && idr.FieldCount == 1 && DBNull.Value != idr[0])
                        {
                            return (int)idr[0];
                        }
                    }
                    cmd.Connection?.Close();
                    if (ret == -1)
                    {
                        throw new DataException("Insert Failed :C");
                    }
                    return ret;
                }
            }
        }

        private void ExecuteUpdate(string update)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(update, con))
                {
                    cmd.Connection?.Open();
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        throw new DataException("More than 1 row affected by update");
                    }
                    cmd.Connection?.Close();
                }
            }
        }

        private void ExecuteDelete(string delete)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(delete, con))
                {
                    cmd.Connection?.Open();
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        throw new DataException("More than 1 row affected by delete");
                    }
                    cmd.Connection?.Close();
                }
            }
        }

        private List<T> PopulateEntities<T>(IDataReader dr, NpgsqlCommand cmd)
        {
            List<T> entities = new List<T>();
            while (dr.Read())
            {
                T ent = Activator.CreateInstance<T>();
                PopulateEntity<T>(ent, dr);
                entities.Add(ent);
            }
            return entities;
        }


        private void PopulateEntity<T>(T entity, IDataRecord record)
        {
            if (record != null && record.FieldCount > 0)
            {
                Type type = entity.GetType();

                for (int i = 0; i < record.FieldCount; i++)
                {
                    if (DBNull.Value != record[i])
                    {
                        PropertyInfo property =
                            type.GetProperty(record.GetName(i),
                                BindingFlags.IgnoreCase |
                                BindingFlags.Public | BindingFlags.Instance);
                        if (property != null)
                        {
                            property.SetValue(entity,
                            record[property.Name], null);
                        }
                    }
                    
                }
            }
        }
    }
}
