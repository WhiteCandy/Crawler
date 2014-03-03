using MySql.Data.MySqlClient;

namespace CrawlCore.Database
{
    public class DatabaseHelper
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
        
        public DatabaseHelper(string server, string userId, string password, string database)
        {
            _server = server;
            _userId = userId;
            _password = password;
            _database = database;
        }

    }
}
