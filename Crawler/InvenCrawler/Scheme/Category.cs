using DatabaseCore.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvenCrawler.Scheme
{
    [Table]
    public class Category
    {
        [Key]
        public int CategoryId;

        public string CategoryName;
        public int LastCrawledArticleId;
        public DateTime LastCrawlingTime;
    }
}
