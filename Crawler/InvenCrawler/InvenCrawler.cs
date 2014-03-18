using CommonHelper;
using DatabaseCore;
using CrawlCore;
using InvenCrawler.Helper;
using InvenCrawler.Scheme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace InvenCrawler
{
    public class InvenCrawler
    {
        protected readonly int CategoryId;
        private int _lastCrawledArticleId;

        public InvenCrawler(int categoryId)
        {
            CategoryId = categoryId;
        }

        public void Start(Database database)
        {
            var lastArticle = database.ExecuteReader<Article>("SELECT * FROM Article ORDER BY ArticleId DESC LIMIT 1");
            if (lastArticle.Count == 0)
            {
                _lastCrawledArticleId = 0;
            }
            else
            {
                _lastCrawledArticleId = lastArticle[0].ArticleId;
            }

            var lastArticleId = GetLastArticleId();
            while (true)
            {
                // crawling 해야 할 글 지정
                var nextArticleId = _lastCrawledArticleId + 1;

                // 웹사이트 주소 구성
                var targetUrl = MakeArticleUrl(CategoryId, nextArticleId);
                
                try
                {

                    // 웹사이트 긁기 - 5초 이후 timeout
                    var rawHtml = targetUrl.CrawlIt(Encoding.GetEncoding(51949), 5000);

                    // 원하는 내용 추출
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(rawHtml);

                    var article = new Article
                    {
                        CategoryId = CategoryId,
                        ArticleId = nextArticleId,
                        RawHtml = rawHtml,
                        CrawlingTime = DateTime.Now,
                    };

                    article.IsDeleted = htmlDoc.IsDeletedArticle();
                    if (!article.IsDeleted)
                    {
                        article.Author = htmlDoc.ExtractAutor();
                        article.WriteTime = htmlDoc.ExtractWrittenTime();
                        article.Title = htmlDoc.ExtractTitle();
                        article.Content = htmlDoc.ExtractContent();
                    }

                    // 데이터베이스에 저장
                    database.SyncData<Article>(article);
                }
                catch (Exception ex)
                {
                    LogHelper.Log(new Exception(targetUrl));
                    LogHelper.Log(ex);
                }

                // 최신 글인지 확인
                while (_lastCrawledArticleId == lastArticleId)
                {
                    lastArticleId = GetLastArticleId();
                    if (_lastCrawledArticleId == lastArticleId)
                    {
                        // 글이 없을 경우 스레드 10분간 휴식
                        Thread.Sleep(10 * 60 * 1000);
                    }
                }

                // 각 글을 crawling 한 후 3초간 휴식
                Thread.Sleep(3 * 1000);

                _lastCrawledArticleId++;
            }
        }

        private int GetLastArticleId()
        {
            // 웹사이트 주소 구성
            var targetUrl = MakeCategoryUrl(CategoryId);

            // 웹 사이트 긁기 - 5초 이후 timeout
            var rawHtml = targetUrl.CrawlIt(Encoding.GetEncoding(51949), 5000);

            // 원하는 내용 추출
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawHtml);

            var lastArticleId = htmlDoc.ExtractLastArticleId();
            return lastArticleId;
        }

        public static string MakeCategoryUrl(int categoryId)
        {
            return string.Format("http://www.inven.co.kr/board/powerbbs.php?come_idx={0}", categoryId);
        }
        
        public static string MakeArticleUrl(int categoryId, int articleId)
        {
            return string.Format("http://www.inven.co.kr/board/powerbbs.php?come_idx={0}&l={1}", categoryId, articleId);
        }
    }
}
