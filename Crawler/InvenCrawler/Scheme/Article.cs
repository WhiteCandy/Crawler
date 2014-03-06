using DatabaseCore.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvenCrawler.Scheme
{
    [Table]
    public class Article
    {
        [Key]
        public int CategoryId;
        [Key]
        public int ArticleId;

        public string RawHtml;
        public DateTime CrawlingTime;

        public bool IsDeleted;
        public string Author;
        public int AuthorId;
        public DateTime WriteTime;
        public string Title;
        public string Content;
        public int CommentCount;
    }
}
