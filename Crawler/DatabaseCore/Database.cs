using System;
using System.Data.SqlClient;
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

        public bool CreateScheme<T>() where T : class
        {
            var table = DatabaseHelper.ToDatabaseScheme<T>();
            var query = table.GenerateDDL(DDLType.CreateTable);

            Console.WriteLine(query);

            ExecuteNonQuery(query);

            return true;
        }

        public bool UpdateScheme<T>() where T : class
        {
            return false;
        }

        public bool IsTableExist(string tableName)
        {
            return false;
        }

        public bool IsFieldExist(string tableName, string fieldName)
        {
            return false;
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
