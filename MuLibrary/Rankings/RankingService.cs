﻿using Microsoft.Extensions.DependencyInjection;
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
        private const string RANKINGS_SEARCH_URL =  "https://mapleunity.com/rankings/all?page=";
        private const string ISLANDER_SEARCH_URL =  "https://mapleunity.com/rankings/islander?page=";
        private const string CAMPER_SEARCH_URL =    "https://mapleunity.com/rankings/camper?page=";

        private readonly Regex _maplerRegex;
        private static ScrapingService _scraper;
            
        public RankingService(IServiceProvider provider) : base(provider)
        {
            _scraper = provider.GetRequiredService<ScrapingService>();
            _maplerRegex = new Regex(@"<td class=""align-middle"">(<img src=""/static/images/rank/guild_master\.png"">(</img>)?(<br>|<br/>))?<b>(?<name>[A-Za-z0-9]{4,12})</b>(<br>|<br/>)\s*(<img src=""/static/images/rank/emblem/\d{8}\.\d{2}\s*\.png"">\s*)?\w{0,12}</td>\s*<!--job-->\s{5}<td class=""align-middle""><img src=""/static/images/rank/[a-zA-Z]{5,8}\.png"">(<br>|<br/>)(?<job>[\w()/\s]{4,25})</td>\s{10}<!--level & exp -->\s{5}<td class=""align-middle""><b>(?<level>\d{1,3})</b>(<br>|<br/>)");
        }

        public async Task<List<Mapler>> GetMaplers()
        {
            _log.Log("Starting rankings processing");

            var maplersList = await GetJobsAsync(RANKINGS_SEARCH_URL);
            var islandersList = await GetJobsAsync(ISLANDER_SEARCH_URL);
            var campersList = await GetJobsAsync(CAMPER_SEARCH_URL);

            foreach (var islander in islandersList)
            {
                if (maplersList.Contains(islander))
                {
                    maplersList.Remove(islander);
                    maplersList.Add(islander);
                }
            }

            foreach (var camper in campersList)
            {
                if (maplersList.Contains(camper))
                {
                    maplersList.Remove(camper);
                    maplersList.Add(camper);
                }
            }

            _log.Log("Rankings processing completed");

            return maplersList;
        }

        private async Task<List<Mapler>> GetJobsAsync(string searchUrl)
        {
            var maplersList = new List<Mapler>();

            var totalPageNumbers = await GetTotalNumberOfPagesAsync(searchUrl);

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
                            string url = RANKINGS_SEARCH_URL + index;
                            await foreach (var mapler in GetMaplersEnumerableFromUrlAsync(url))
                            {
                                maplersList.Add(mapler);
                            }
                        }
                        finally
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Successfully parsed page { index }");
                            slim.Release();
                        }
                    }));
                }
                await Task.WhenAll(allTasks).ConfigureAwait(false);
            }

            return maplersList;
        }

        private async IAsyncEnumerable<Mapler> GetMaplersEnumerableFromUrlAsync(string url)
        {
            string page = await _scraper.DownloadPageAsync(url).ConfigureAwait(false);

            await foreach (Match match in _scraper.GetMatchesInPageAsync(_maplerRegex, page))
            {
                var mapler = new Mapler()
                {
                    Name = match.Groups["name"].Value,
                    Job = match.Groups["job"].Value,
                    Level = int.Parse(match.Groups["level"].Value),
                };

                if (mapler.Job == "Beginner")
                {
                    mapler.Job = mapler.Level switch
                    {
                        var x when x < 30 => "Beginner",
                        var x when x >= 30 && x < 70 => "Beginner (30+)",
                        var x when x >= 70 && x < 120 => "Beginner (70+)",
                        _ => "Beginner (120+)",
                    };
                }
                else if (mapler.Job == "Islander")
                {
                    mapler.Job = mapler.Level switch
                    {
                        var x when x < 30 => "Islander",
                        var x when x >= 30 && x < 70 => "Islander (30+)",
                        var x when x >= 70 && x < 120 => "Islander (70+)",
                        _ => "Islander (120+)",
                    };
                }
                else if (mapler.Job == "Camper")
                {
                    mapler.Job = mapler.Level switch
                    {
                        var x when x < 30 => "Camper",
                        var x when x >= 30 && x < 70 => "Camper (30+)",
                        var x when x >= 70 && x < 120 => "Camper (70+)",
                        _ => "Camper (120+)",
                    };
                }

                yield return mapler;
            }
        }

        private async Task<List<Mapler>> GetMaplersFromUrlAsync(string url)
        { 
            string page = await _scraper.DownloadPageAsync(url).ConfigureAwait(false);
            var maplersList = new List<Mapler>();

            await foreach (Match match in _scraper.GetMatchesInPageAsync(_maplerRegex, page))
            {
                var mapler = new Mapler()
                {
                    Name = match.Groups["name"].Value,
                    Job = match.Groups["job"].Value,
                    Level = int.Parse(match.Groups["level"].Value),
                };
                
                maplersList.Add(mapler);
            }

            return maplersList;
        }

        private async Task<int> GetTotalNumberOfPagesAsync(string searchUrl)
        {
            int currentPageNumber;
            int lowestPageNumber = 0;
            int highestPageNumber = 9999;

            int lastPageWithMaplers = 0;

            while (lowestPageNumber <= highestPageNumber)
            {
                currentPageNumber = (highestPageNumber + lowestPageNumber) / 2;
                string url = searchUrl + currentPageNumber;
                var maplersList = await GetMaplersFromUrlAsync(url);

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Checking for page number for total number of pages on page { currentPageNumber }");

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
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Between 5 and 0 maplers found at index { currentPageNumber }");
                    return currentPageNumber;
                }
            }

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Last page with maplers at index { lastPageWithMaplers }");
            return lastPageWithMaplers;
        }
    }
}
