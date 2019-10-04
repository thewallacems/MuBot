using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MuLibrary.Services
{
    public class ScrapingService : ServiceBase
    {
        private readonly HtmlWeb _client = new HtmlWeb();

        public ScrapingService(IServiceProvider provider) : base(provider) { }

        public async IAsyncEnumerable<Match> GetMatchesInPageAsync(Regex regex, string page)
        {
            var matches = await Task.Run( () => regex.Matches(page));
            if (matches.Count == 0) { _log.Log($"Ah ha ha... No matches found {regex.ToString()}"); yield break; }
            foreach (Match match in matches) yield return match;
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
                    return doc.DocumentNode.InnerHtml;
                }
                catch (Exception ex) when (ex is TimeoutException || ex is IOException)
                {
                    retries += 1;
                    Thread.Sleep(500 * retries);
                    _log.Log($"{ex.GetType().ToString()} occurred on retry {retries} loading {url}");
                    continue;
                }
            }
        }
    }
}
