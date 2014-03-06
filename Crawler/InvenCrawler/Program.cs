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
            var table = DatabaseHelper.ToDatabaseScheme<Article>();
            Console.WriteLine(table.GenerateDDL(DDLType.CreateTable));
        }
    }
}
