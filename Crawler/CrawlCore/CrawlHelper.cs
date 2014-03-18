using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommonHelper;

namespace CrawlCore
{
    public static class CrawlHelper
    {
        public static string CrawlIt(this string url, Encoding encoding, int timeout)
        {
            var tryCount = 0;
            while (true)
            {
                try
                {
                    var webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)";
                    webRequest.CookieContainer = new CookieContainer();
                    webRequest.AllowAutoRedirect = true;
                    webRequest.Timeout = timeout;

                    using (var webResponse = (HttpWebResponse) webRequest.GetResponse())
                    using (var reader = new StreamReader(webResponse.GetResponseStream(), encoding))
                    {
                        var rawHtml = reader.ReadToEnd();
                        var statusCode = webResponse.StatusCode;

                        return rawHtml;
                    }
                }
                catch (WebException ex)
                {
                    LogHelper.Log(new Exception(url));
                    LogHelper.Log(ex);
                    tryCount++;

                    // 3번 시도한 후 안될 경우 더 이상 시도하지 않음
                    if (tryCount == 3)
                        throw;
                }
            }
        }
    }
}
