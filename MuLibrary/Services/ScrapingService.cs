using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MuLibrary.Services
{
    public class ScrapingService
    {
        public static async IAsyncEnumerable<Match> GetMatchesInPageAsync(string pattern, string page)
        {
            var regex = new Regex(pattern);
            var matches = await Task.Run(() => regex.Matches(page));

            foreach (Match match in matches) yield return match;
        }

        public static Match GetMatchInPage(string pattern, string page)
        {
            var regex = new Regex(pattern);
            if (!regex.IsMatch(page)) throw new ArgumentException();

            return regex.Match(page);
        }

        protected static async Task<string> DownloadPageAsync(HtmlWeb client, string url)
        {
            int retries = 0;

            while (true)
            {
                try
                {
                    var doc = await client.LoadFromWebAsync(url).ConfigureAwait(false);
                    return doc.DocumentNode.InnerHtml;
                }
                catch (Exception ex) when (ex is TimeoutException || ex is IOException)
                {
                    retries += 1;
                    Thread.Sleep(500 * retries);
                    continue;
                }
            }
        }
    }
}
