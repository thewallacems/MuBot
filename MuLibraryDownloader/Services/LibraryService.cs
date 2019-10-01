using HtmlAgilityPack;
using MuLibrary.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MuLibraryDownloader.Services
{
    public class LibraryService : ScrapingService
    {
        private const string TOTAL_PAGE_NUMBER_PATTERN = @"<li class=""page-item disabled"" aria-disabled=""true""><span class=""page-link"">...</span></li>\s{158}<li class=""page-item""><a class=""page-link"" href=""https://lib\.mapleunity\.com/\w*\?page=\d*"">\d*</a></li>\s{81}<li class=""page-item""><a class=""page-link"" href=""https://lib\.mapleunity\.com/\w*\?page=\d*"">(?<totalPageNumber>\d*)</a></li>";
        private const string OBJECT_IDS_PATTERN = @"<img src=""/images/\w*/\d{7}\.png"" alt="".*"">\s{29}</a>\s{25}</td>\s{25}<td class=""text-left""><a href=""/\w*/(?<id>\d{7})"">.*</a></td>";

        public static async Task<int> GetTotalPageNumberAsync(HtmlWeb client, string url)
        {
            string page = await DownloadPageAsync(client, url);

            try
            {
                var match = GetMatchInPage(TOTAL_PAGE_NUMBER_PATTERN, page);
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
            string page = await DownloadPageAsync(client, url);

            await foreach (Match match in GetMatchesInPageAsync(OBJECT_IDS_PATTERN, page))
            {
                var id = match.Groups["id"].Value;
                yield return id;
            }
        }
    }
}