using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static MuLibrary.Utils.Miscellaneous;

namespace MuLibrary.Services
{
    public class RankingService
    {
        private const string RANKINGS_SEARCH_URL = "https://mapleunity.com/rankings/all?page=";

        private const string JOB_PATTERN =      @"<!--job-->\s{5}<td class=""align-middle""><img src=""/static/images/rank/\w{5,8}\.png""><br>(?<job>[\w()/\s]{4,25})</td>\s{10}<!--level & exp -->\s{5}<td class=""align-middle""><b>(?<level>\d{1,3})</b><br>";
        private const string LEVEL_PATTERN =    @"<!--level & exp -->\s{5}<td class=""align-middle""><b>(?<level>\d{1,3})</b><br>";

        private static ScrapingService _scraper;

        public RankingService(IServiceProvider provider)
        {
            _scraper = provider.GetService<ScrapingService>();
        }

        public async Task<Dictionary<string, int>> GetJobs()
        {
            var jobToJobCount = new Dictionary<string, int>()
            {
                { "Fighter", 0 },
                { "Crusader", 0 },
                { "Hero", 0 },

                { "Page", 0 },
                { "White Knight", 0 },
                { "Paladin", 0 },

                { "Spearman", 0 },
                { "Dragon Knight", 0 },
                { "Dark Knight", 0 },

                { "Warrior", 0 },

                { "Wizard (Fire/Poison)", 0 },
                { "Mage (Fire/Poison)", 0 },
                { "Archmage (Fire/Poison)", 0 },

                { "Wizard (Ice/Lightning)", 0 },
                { "Mage (Ice/Lightning)", 0 },
                { "Archmage (Ice/Lightning)", 0 },

                { "Cleric", 0 },
                { "Priest", 0 },
                { "Bishop", 0 },

                { "Magician", 0 },

                { "Crossbowman", 0 },
                { "Sniper", 0 },
                { "Marksman", 0 },

                { "Hunter", 0 },
                { "Ranger", 0 },
                { "Bowmaster", 0 },

                { "Bowman", 0 },

                { "Bandit", 0 },
                { "Chief Bandit", 0 },
                { "Shadower", 0 },

                { "Assassin", 0 },
                { "Hermit", 0 },
                { "Night Lord", 0 },

                { "Thief", 0 },

                { "Brawler", 0 },
                { "Marauder", 0 },
                { "Buccaneer", 0 },

                { "Gunslinger", 0 },
                { "Outlaw", 0 },
                { "Corsair", 0 },

                { "Pirate", 0 },

                { "Beginner (30+)", 0 },
                { "Beginner (70+)", 0 },
                { "Beginner (120+)", 0 },

                { "Beginner", 0 },
            };

            (int, int) totalPageNumbersAndFirstPageNumberWithOnlyBeginners = GetTotalPageNumbersAndFirstPageNumberWithOnlyBeginners();
            int totalPageNumbers = totalPageNumbersAndFirstPageNumberWithOnlyBeginners.Item1;
            int firstPageNumberWithOnlyBeginners = totalPageNumbersAndFirstPageNumberWithOnlyBeginners.Item2;

            PrintToConsole("Starting rankings processing");

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
                            if (index >= firstPageNumberWithOnlyBeginners && index != totalPageNumbers)
                            {
                                jobToJobCount["Beginner"] = jobToJobCount["Beginner"] + 5;
                            }
                            else
                            {
                                string url = RANKINGS_SEARCH_URL + index;
                                await foreach (string job in GetJobsEnumerableFromUrlAsync(url))
                                {
                                    jobToJobCount[job] = jobToJobCount[job] + 1;
                                }
                            }
                        }
                        finally
                        {
                            PrintToConsole($"Successfully parsed page { index }");
                            slim.Release();
                        }
                    }));
                }
                await Task.WhenAll(allTasks).ConfigureAwait(false);
            }

            PrintToConsole("Rankings processing completed");

            return jobToJobCount;
        }

        private static (int, int) GetTotalPageNumbersAndFirstPageNumberWithOnlyBeginners()
        {
            Task<int>[] tasks = new Task<int>[]{ GetTotalNumberOfPagesAsync(), GetFirstPageWithOnlyBeginnersAsync() };

            Task.WaitAll(tasks);

            int totalPageNumbers = tasks[0].Result;
            int firstPageWithOnlyBeginners = tasks[1].Result;

            return (totalPageNumbers, firstPageWithOnlyBeginners);
        }

        private static async IAsyncEnumerable<string> GetJobsEnumerableFromUrlAsync(string url)
        {
            string page = await _scraper.DownloadPageAsync(url).ConfigureAwait(false);

            await foreach (Match match in _scraper.GetMatchesInPageAsync(JOB_PATTERN, page))
            {
                var job = match.Groups["job"].Value;
                if (job == "Beginner")
                {
                    var level = int.Parse(match.Groups["level"].Value);

                    job = level switch
                    {
                        int x when x < 30 =>                "Beginner",
                        int x when x >= 30 && x < 70 =>     "Beginner (30+)",
                        int x when x >= 70 && x < 120 =>    "Beginner (70+)",
                        int x when x >= 120 =>              "Beginner (120+)",

                        _ => throw new ArgumentException(),
                    };
                }

                yield return job;
            }

        }

        private static async Task<List<string>> GetJobsFromUrlAsync(string url)
        { 
            string page = await _scraper.DownloadPageAsync(url).ConfigureAwait(false);
            List<string> jobsList = new List<string>();

            await foreach (Match match in _scraper.GetMatchesInPageAsync(JOB_PATTERN, page))
            {
                string job = match.Groups["job"].Value;
                if (job == "Beginner")
                {
                    var level = int.Parse(match.Groups["level"].Value);

                    job = level switch
                    {
                        int x when x < 30 =>                "Beginner",
                        int x when x >= 30 && x < 70 =>     "Beginner (30+)",
                        int x when x >= 70 && x < 120 =>    "Beginner (70+)",
                        int x when x >= 120 =>              "Beginner (120+)",
                        
                        _ => throw new ArgumentException(),
                    };
                }

                jobsList.Add(job);
            }

            return jobsList;
        }

        private static async Task<int> GetTotalNumberOfPagesAsync()
        {
            int currentPageNumber;
            int lowestPageNumber = 0;
            int highestPageNumber = 9999;

            int lastPageWithCharacters = 0;

            while (lowestPageNumber <= highestPageNumber)
            {
                currentPageNumber = (highestPageNumber + lowestPageNumber) / 2;
                string url = RANKINGS_SEARCH_URL + currentPageNumber;
                List<string> jobs = await GetJobsFromUrlAsync(url);

                PrintToConsole($"Checking for page number for total number of pages on page { currentPageNumber }");

                if (!jobs.Any())
                {
                    highestPageNumber = currentPageNumber - 1;
                    continue;
                }

                int jobsCount = jobs.Count();
                
                if (jobsCount == 5)
                {
                    lowestPageNumber = currentPageNumber + 1;
                    lastPageWithCharacters = currentPageNumber;
                    continue;
                }
                else if (jobsCount < 5)
                {
                    PrintToConsole($"Total number of pages on page { currentPageNumber }");
                    return currentPageNumber;
                }
            }

            PrintToConsole($"Total number of pages on page { lastPageWithCharacters }");
            return lastPageWithCharacters;
        }

        private static async Task<List<int>> GetLevelsFromUrlAsync(string url)
        {
            string page = await _scraper.DownloadPageAsync(url).ConfigureAwait(false);
            List<int> levelsList = new List<int>();

            await foreach (Match match in _scraper.GetMatchesInPageAsync(LEVEL_PATTERN, page))
            {
                int level = int.Parse(match.Groups["level"].Value);
                levelsList.Add(level);
            }

            return levelsList;
        }

        private static async Task<int> GetFirstPageWithOnlyBeginnersAsync()
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

                PrintToConsole($"Checking for page number with only level sevens on page { currentPageNumber }");

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
                    PrintToConsole($"First page with only level sevens { currentPageNumber + 1 }");
                    return currentPageNumber + 1;
                }
            }

            return lastPageWithOnlyBeginners;
        }
    }
}
