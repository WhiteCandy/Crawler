using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseCore;
using InvenCrawler.Scheme;

namespace InvenCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            var database = new Database {
                Server = "",
                UserId = "",
                Password = "",
                InitialCatalog = "",
            };

            database.SyncTable<Article>();
            database.SyncTable<Category>();
            var result = database.ExecuteReader<Article>("SELECT * FROM Article");
            Console.WriteLine(result.Count);
        }
    }
}
