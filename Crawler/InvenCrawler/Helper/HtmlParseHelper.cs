using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace InvenCrawler.Helper
{
    public static class InvenParseHelper
    {
        public static bool IsDeletedArticle(this HtmlDocument htmlDoc)
        {
            return htmlDoc.DocumentNode.ChildNodes.Count == 1;
        }

        public static string ExtractAutor(this HtmlDocument htmlDoc)
        {
            var articleWriterNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='articleWriter']/span");
            var accountName = articleWriterNode.Attributes["onclick"].Value.Replace("layerNickName('", "").Replace("','pbNickNameHandler')", "");

            return accountName;
        }

        public static DateTime ExtractWrittenTime(this HtmlDocument htmlDoc)
        {
            var writtenTimeNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='articleInfo']/div[@class='articleDate']");
            var writtenTime = DateTime.Parse(writtenTimeNode.InnerText);

            return writtenTime;
        }

        public static string ExtractTitle(this HtmlDocument htmlDoc)
        {
            var articleTitleNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='articleSubject ']/div[@class='articleTitle']/h1");
            var articleTitle = articleTitleNode.InnerText;

            return articleTitle;
        }

        public static string ExtractContent(this HtmlDocument htmlDoc)
        {
            var articleContentNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='powerbbsContent']");
            var articleHtmlContent = articleContentNode.InnerHtml.Replace("<br>", Environment.NewLine);
            var tempDoc = new HtmlDocument();
            tempDoc.LoadHtml(HtmlEntity.DeEntitize(articleHtmlContent));
            var articleContent = tempDoc.DocumentNode.InnerText.Trim();

            return articleContent;
        }

        public static int ExtractLastArticleId(this HtmlDocument htmlDoc)
        {
            var articleListTableNode = htmlDoc.DocumentNode.SelectNodes("//table[@width='710']").Skip(3).First();
            var textList = articleListTableNode.SelectNodes("//td[@align='center']").Select(e => e.InnerText.Trim()).Where(e => e.Length > 0);
            var lastArticleId = int.Parse(textList.ElementAt(1));

            return lastArticleId;
        }
    }
}
