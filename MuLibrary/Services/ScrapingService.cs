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
        private readonly HtmlWeb _client = new HtmlWeb();

        public async IAsyncEnumerable<Match> GetMatchesInPageAsync(string pattern, string page)
        {
            var regex = new Regex(pattern);
            var matches = await Task.Run(() => regex.Matches(page));

            foreach (Match match in matches) yield return match;
        }

        public Match GetMatchInPage(string pattern, string page)
        {
            var regex = new Regex(pattern);
            return regex.Match(page);
        }

        public async Task<string> DownloadPageAsync(string url)
        {
            int retries = 0;

            while (true)
            {
                try
                {
                    var doc = await _client.LoadFromWebAsync(url).ConfigureAwait(false);
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
