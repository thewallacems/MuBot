using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Downloading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MuLibrary.Rankings
{
    public class RankingService : ServiceBase
    {
        private const string RANKINGS_SEARCH_URL = "https://mapleunity.com/rankings/all?page=";

        private readonly Regex JOB_REGEX = new Regex(@"<!--job-->\s{5}<td class=""align-middle""><img src=""/static/images/rank/[a-zA-Z]{5,8}\.png""><br>(?<job>[\w()/\s]{4,25})</td>\s{10}<!--level & exp -->\s{5}<td class=""align-middle""><b>(?<level>\d{1,3})</b><br>");
        private readonly Regex LEVEL_REGEX = new Regex(@"<!--level & exp -->\s{5}<td class=""align-middle""><b>(?<level>\d{1,3})</b><br>");

        private static ScrapingService _scraper;
            
        public RankingService(IServiceProvider provider) : base(provider)
        {
            _scraper = provider.GetService<ScrapingService>();
        }

        public async Task<List<Mapler>> GetMaplers(int pages = -1)
        {
            var maplersList = new List<Mapler>();

            var totalPageNumbersTask = GetTotalNumberOfPagesAsync();
            var firstPageWithOnlyBeginnersTask = GetFirstPageWithOnlyBeginnersAsync();

            Task.WaitAll(totalPageNumbersTask, firstPageWithOnlyBeginnersTask);

            int totalPageNumbers = totalPageNumbersTask.Result; 
            int firstPageWithOnlyBeginners = firstPageWithOnlyBeginnersTask.Result;

            if (pages >= 0)
            {
                totalPageNumbers = pages;
            }

            _log.Log("Starting rankings processing");

            var allTasks = new List<Task>();
            using (var slim = new SemaphoreSlim(10, 20))
            {
                foreach (var index in Enumerable.Range(1, totalPageNumbers))
                {
                    await slim.WaitAsync();
                    allTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            if (index <= firstPageWithOnlyBeginners)
                            {
                                string url = RANKINGS_SEARCH_URL + index;
                                await foreach (var mapler in GetMaplersEnumerableFromUrlAsync(url))
                                {
                                    if (mapler.Job == "Beginner" && mapler.Level <= 10) continue;

                                    maplersList.Add(mapler);
                                }
                            }
                        }
                        finally
                        {
                            _log.Log($"Successfully parsed page { index }");
                            slim.Release();
                        }
                    }));
                }
                await Task.WhenAll(allTasks).ConfigureAwait(false);
            }

            _log.Log("Rankings processing completed");

            return maplersList;
        }

        private async IAsyncEnumerable<Mapler> GetMaplersEnumerableFromUrlAsync(string url)
        {
            string page = await _scraper.DownloadPageAsync(url).ConfigureAwait(false);

            await foreach (Match match in _scraper.GetMatchesInPageAsync(JOB_REGEX, page))
            {
                var mapler = new Mapler()
                {
                    Job = match.Groups["job"].Value,
                    Level = int.Parse(match.Groups["level"].Value),
                };

                if (mapler.Job == "Beginner")
                {
                    mapler.Job = mapler.Level switch
                    {
                        var x when x < 30 =>                "Beginner",
                        var x when x >= 30 && x < 70 =>     "Beginner (30+)",
                        var x when x >= 70 && x < 120 =>    "Beginner (70+)",
                        _ =>                                "Beginner (120+)",
                    };
                }

                yield return mapler;
            }
        }

        private async Task<List<Mapler>> GetMaplersFromUrlAsync(string url)
        { 
            string page = await _scraper.DownloadPageAsync(url).ConfigureAwait(false);
            var maplersList = new List<Mapler>();

            await foreach (Match match in _scraper.GetMatchesInPageAsync(JOB_REGEX, page))
            {
                var mapler = new Mapler()
                {
                    Job = match.Groups["job"].Value,
                    Level = int.Parse(match.Groups["level"].Value),
                };
                
                maplersList.Add(mapler);
            }

            return maplersList;
        }

        private async Task<int> GetTotalNumberOfPagesAsync()
        {
            int currentPageNumber;
            int lowestPageNumber = 0;
            int highestPageNumber = 9999;

            int lastPageWithMaplers = 0;

            while (lowestPageNumber <= highestPageNumber)
            {
                currentPageNumber = (highestPageNumber + lowestPageNumber) / 2;
                string url = RANKINGS_SEARCH_URL + currentPageNumber;
                var maplersList = await GetMaplersFromUrlAsync(url);

                _log.Log($"Checking for page number for total number of pages on page { currentPageNumber }");

                if (!maplersList.Any())
                {
                    highestPageNumber = currentPageNumber - 1;
                    continue;
                }

                int maplersCount = maplersList.Count();
                
                if (maplersCount == 5)
                {
                    lowestPageNumber = currentPageNumber + 1;
                    lastPageWithMaplers = currentPageNumber;
                    continue;
                }
                else if (maplersCount < 5 && maplersCount > 0)
                {
                    _log.Log($"Between 5 and 0 maplers found at index { currentPageNumber }");
                    return currentPageNumber;
                }
            }

            _log.Log($"Last page with maplers at index { lastPageWithMaplers }");
            return lastPageWithMaplers;
        }

        private async Task<List<int>> GetLevelsFromUrlAsync(string url)
        {
            string page = await _scraper.DownloadPageAsync(url).ConfigureAwait(false);
            List<int> levelsList = new List<int>();

            await foreach (Match match in _scraper.GetMatchesInPageAsync(LEVEL_REGEX, page))
            {
                int level = int.Parse(match.Groups["level"].Value);
                levelsList.Add(level);
            }

            return levelsList;
        }

        private async Task<int> GetFirstPageWithOnlyBeginnersAsync()
        {
            int currentPageNumber;
            int lowestPageNumber = 0;
            int highestPageNumber = 9999;
            int lastPageWithOnlyBeginners = 0;

            while (lowestPageNumber < highestPageNumber)
            {
                currentPageNumber = (lowestPageNumber + highestPageNumber) / 2;
                string url = RANKINGS_SEARCH_URL + currentPageNumber;
                List<int> levels = await GetLevelsFromUrlAsync(url);

                _log.Log($"Checking for page number with only level sevens on page { currentPageNumber }");

                if (!levels.Any())
                {
                    highestPageNumber = currentPageNumber - 1;
                    continue;
                }

                double levelsAverage = levels.Average();

                if (levelsAverage <= 7.0)
                {
                    highestPageNumber = currentPageNumber - 1;
                    lastPageWithOnlyBeginners = currentPageNumber;
                    continue;
                }
                else if (levelsAverage >= 8.0)
                {
                    lowestPageNumber = currentPageNumber + 1;
                    continue;
                }
                else if (levelsAverage < 8.0 && levelsAverage > 7.0)
                {
                    _log.Log($"First page with only level sevens { currentPageNumber + 1 }");
                    return currentPageNumber + 1;
                }
            }

            return lastPageWithOnlyBeginners;
        }
    }
}
