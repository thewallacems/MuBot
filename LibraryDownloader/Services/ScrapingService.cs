using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryDownloader.Services
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

        public static async Task<string> DownloadPageAsync(HtmlWeb client, string url)
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

        public static async Task<int> GetTotalPageNumberAsync(HtmlWeb client, string url)
        {
            string pattern = @"<li class=""page-item disabled"" aria-disabled=""true""><span class=""page-link"">...</span></li>\s{158}<li class=""page-item""><a class=""page-link"" href=""https://lib\.mapleunity\.com/\w*\?page=\d*"">\d*</a></li>\s{81}<li class=""page-item""><a class=""page-link"" href=""https://lib\.mapleunity\.com/\w*\?page=\d*"">(?<totalPageNumber>\d*)</a></li>";
            string page = await DownloadPageAsync(client, url);

            try
            {
                var match = GetMatchInPage(pattern, page);
                var totalPageNumber = int.Parse(match.Groups["totalPageNumber"].Value);
                return totalPageNumber;
            }
            catch
            {
                throw new ArgumentException("Invalid URL supplied or pattern is invalid.");
            }
        }

        public static async IAsyncEnumerable<string> GetObjectIDsFromUrlAsync(HtmlWeb client, string url)
        {
            string pattern = @"<img src=""/images/\w*/\d{7}\.png"" alt="".*"">\s{29}</a>\s{25}</td>\s{25}<td class=""text-left""><a href=""/\w*/(?<id>\d{7})"">.*</a></td>";
            string page = await DownloadPageAsync(client, url);

            await foreach (Match match in GetMatchesInPageAsync(pattern, page))
            {
                var mobId = match.Groups["mobId"].Value;
                yield return mobId;
            }
        }
    }
}
