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
    class PostGreSqlDatabase : IDatabase
    {
        private readonly string _connectionString = "";
        private readonly PostGreSqlExpressionTreeVisitor _visitor = null;
        public PostGreSqlDatabase(string connection = "Server=127.0.0.1; Port=5432; User Id=postgres; Password=postgres; Database=imgDB")
        {
            _connectionString = connection;
            _visitor = new PostGreSqlExpressionTreeVisitor();
        }

        public IEnumerable<T> Select<T>(Expression expression)
        {
            return ExecuteSelect<T>(BuildSqlSelectString(expression));
        }

        public int Insert<T>(T newObject)
        {
            //ToDo: If object tabletype , inster into... without PK
            throw new NotImplementedException();
        }

        public void Delete<T>(T deletedObject)
        {
            //ToDo: If object tabletype , delete from... without PK
            throw new NotImplementedException();
        }

        public void Update<T>(T changedObject)
        {
            //ToDo: If object tabletype , update ... without PK
            throw new NotImplementedException();
        }

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
