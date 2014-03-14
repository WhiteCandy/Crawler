using System;
using System.Data.SqlClient;
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
            var tableQuery = string.Format(
                @"SELECT count(*) FROM information_schema.tables WHERE table_schema = '{0}' AND table_name = '{1}'",
                InitialCatalog, tableName);
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
    }
}
