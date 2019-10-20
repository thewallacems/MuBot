using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MuLibrary.Downloading
{
    public class ScrapingService : ServiceBase
    {
        private readonly HtmlWeb _client = new HtmlWeb();

        public ScrapingService(IServiceProvider provider) : base(provider) { }

        public async IAsyncEnumerable<Match> GetMatchesInPageAsync(Regex regex, string page)
        {
            var matches = await Task.Run( () => regex.Matches(page) );
            if (matches.Count == 0)
                yield break;

            foreach (Match match in matches)
                yield return match;
        }

        public Match GetMatchInPage(Regex regex, string page)
        {
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
                    var innerHtml = doc.DocumentNode.InnerHtml;

                    return innerHtml;
                }
                catch (Exception ex)
                {
                    retries += 1;
                    Thread.Sleep(500 * retries);

                    _log.Log($"{ex.GetType().Name} occurred on retry {retries} loading {url}");
                    continue;
                }
            }
        }
    }
}
