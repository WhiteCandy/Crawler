using MySql.Data.MySqlClient;
namespace DatabaseCore
{
    public class Database
    {
        private readonly string _server;
        private readonly string _userId;
        private readonly string _password;
        private readonly string _database;

        private string DatabaseConnectionString
        {
            get
            {
                return new MySqlConnectionStringBuilder
                {
                    Server = _server,
                    UserID = _userId,
                    Password = _password,
                    Database = _database,
                }
                .ConnectionString;
            }
        }
        
        public Database(string server, string userId, string password, string database)
        {
            _server = server;
            _userId = userId;
            _password = password;
            _database = database;
        }

        public bool CreateScheme<T>() where T : class
        {
            return false;
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
    }
}
