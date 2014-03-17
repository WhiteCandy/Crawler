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
            if (args.Length != 5)
            {
                const string command = @"InvenCralwer.exe [server] [userId] [password] [database] [inven category id]";
                Console.WriteLine("Parameters: {0}", command);
                return;
            }
            
            var database = new Database {
                Server = args[0],
                UserId = args[1],
                Password = args[2],
                InitialCatalog = args[3],
            };

            database.SyncTable<Article>();
            database.SyncTable<Category>();

            // start crawler
            var categoryId = int.Parse(args[4]);
            var crawler = new InvenCrawler(categoryId);
            crawler.Start(database);
        }
    }
}
