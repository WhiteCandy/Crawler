using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using DatabaseCore.Model;
using MySql.Data.MySqlClient;
using System.Data;
namespace DatabaseCore
{
    public class Database
    {
        public string Server { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string InitialCatalog { get; set; }

        private string DatabaseConnectionString
        {
            get
            {
                return new MySqlConnectionStringBuilder
                {
                    Server = Server,
                    UserID = UserId,
                    Password = Password,
                    Database = InitialCatalog,
                    CharacterSet = "utf8",
                }
                .ConnectionString;
            }
        }

        public void SyncTable<T>() where T : class
        {
            var table = DatabaseHelper.ToDatabaseScheme<T>();
            if (!IsTableExist(table.Name))
                CreateScheme<T>();
            else
                UpdateScheme<T>();
        }

        public DataTable ExecuteReader(string query)
        {
            var data = new DataTable();
            using (var conn = new MySqlConnection(DatabaseConnectionString))
            {
                conn.Open();
                using (var adapter = new MySqlDataAdapter(query, conn))
                {
                    adapter.Fill(data);
                }
            }
            return data;
        }

        public List<T> ExecuteReader<T>(string query) where T : class, new()
        {
            var queryResult = new List<T>();

            var data = ExecuteReader(query);
            foreach (DataRow row in data.Rows)
            {
                var result = new T();
                var typeInfo = typeof (T);
                foreach (var fieldInfo in typeInfo.GetFields())
                {
                    if(row[fieldInfo.Name] == null) continue;
                    fieldInfo.SetValue(result, row[fieldInfo.Name]);
                }

                queryResult.Add(result);
            }

            return queryResult;
        }

        public void ExecuteNonQuery(string query)
        {
            using (var conn = new MySqlConnection(DatabaseConnectionString))
            {
                conn.Open();
                using (var command = new MySqlCommand(query, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void CreateScheme<T>() where T : class
        {
            var table = DatabaseHelper.ToDatabaseScheme<T>();
            var query = table.CreateTableQuery();

            ExecuteNonQuery(query);
        }

        public void UpdateScheme<T>() where T : class
        {
            var table = DatabaseHelper.ToDatabaseScheme<T>();
            var query = this.ModifyTableQuery(table, GetTableSchema(table.Name));

            if (query.Trim().Length > 0)
                ExecuteNonQuery(query);
        }

        public bool IsTableExist(string tableName)
        {
            var tableQuery = DatabaseHelper.CheckTableExistQuery(InitialCatalog, tableName);
            var result = ExecuteReader(tableQuery);
            return (long) result.Rows[0][0] > 0;
        }

        public DataTable GetTableSchema(string tableName)
        {
            DataTable scheme = null;
            using (var conn = new MySqlConnection(DatabaseConnectionString))
            {
                conn.Open();
                using (var command = new MySqlCommand(string.Format("SELECT * FROM {0}", tableName), conn))
                {
                    using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        scheme = reader.GetSchemaTable();
                    }
                }
            }
            return scheme;
        }

        public void SyncData<T>(T row) where T : class
        {
            if (!IsElementExist<T>(row))
            {
                InsertData<T>(row);
            }
            else
            {
                UpdateData<T>(row);
            }
        }

        public bool IsElementExist<T>(T row) where T : class
        {
            var elementExistQuery = DatabaseHelper.CheckElementExistQuery<T>(row);
            if (elementExistQuery.Length == 0) return false;

            Console.WriteLine(elementExistQuery);

            using (var conn = new MySqlConnection(DatabaseConnectionString))
            {
                conn.Open();
                using (var command = new MySqlCommand(elementExistQuery, conn))
                {
                    var table = DatabaseHelper.ToDatabaseScheme<T>();
                    foreach (var elem in table.ElementList.Where(e => e.IsKey))
                    {
                        var typeInfo = typeof (T);
                        foreach (var fieldInfo in typeInfo.GetFields())
                        {
                            if (fieldInfo.Name != elem.Name) continue;
                            command.Parameters.AddWithValue("@" + elem.Name, fieldInfo.GetValue(row));
                        }
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        return (long) reader[0] > 0;
                    }
                }
            }
        }

        public void InsertData<T>(T row) where T : class
        {
            var insertQuery = DatabaseHelper.InsertElementQuery<T>(row);
            if (insertQuery.Length == 0) return;

            Console.WriteLine(insertQuery);

            using (var conn = new MySqlConnection(DatabaseConnectionString))
            {
                conn.Open();
                using (var command = new MySqlCommand(insertQuery, conn))
                {
                    var table = DatabaseHelper.ToDatabaseScheme<T>();
                    foreach (var elem in table.ElementList)
                    {
                        var typeInfo = typeof(T);
                        foreach (var fieldInfo in typeInfo.GetFields())
                        {
                            if (fieldInfo.Name != elem.Name) continue;
                            command.Parameters.AddWithValue("@" + elem.Name, fieldInfo.GetValue(row));
                        }
                    }

                    var insertedCount = command.ExecuteNonQuery();
                    Console.WriteLine(insertedCount);
                }
            }

        }

        public void UpdateData<T>(T row) where T : class
        {
            var updateQuery = DatabaseHelper.UpdateElementQuery<T>(row);
            if (updateQuery.Length == 0) return;

            Console.WriteLine(updateQuery);

            using (var conn = new MySqlConnection(DatabaseConnectionString))
            {
                conn.Open();
                using (var command = new MySqlCommand(updateQuery, conn))
                {
                    var table = DatabaseHelper.ToDatabaseScheme<T>();
                    foreach (var elem in table.ElementList)
                    {
                        var typeInfo = typeof(T);
                        foreach (var fieldInfo in typeInfo.GetFields())
                        {
                            if (fieldInfo.Name != elem.Name) continue;
                            command.Parameters.AddWithValue("@" + elem.Name, fieldInfo.GetValue(row));
                        }
                    }

                    var updatedCount = command.ExecuteNonQuery();
                    Console.WriteLine(updatedCount);
                }
            }
        }
    }
}
