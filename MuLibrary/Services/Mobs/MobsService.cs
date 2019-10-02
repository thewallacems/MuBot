using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MuLibrary.Services.Mobs
{
    public class MobsService
    {
        private const string MOB_LIBRARY_URL = "https://lib.mapleunity.com/mob/";
        private const string MOB_SEARCH_URL = "https://lib.mapleunity.com/mob?page=";
        private const string MOB_STAT_DATA_PATTERN = @"<h4 class=""mt-2"">\s{13}(?<name>[\S\s]*)<br>\s{13}Level: \d*\s{9}<\/h4>[\S\s]*<strong>Weapon Attack: </strong> (?<weaponAttack>(-?\d*|-))<br>\s{17}<strong>Magic Attack: </strong> (?<magicAttack>(-?\d*|-))<br>\s{17}<strong>Weapon Defense: </strong> (?<weaponDefense>(-?\d*|-))<br>\s{17}<strong>Magic Defense: </strong> (?<magicDefense>(-?\d*|-))<br>\s{17}<strong>Accuracy: </strong> (?<accuracy>(-?\d*|-))<br>\s{17}<strong>Avoidability: </strong> (?<avoidability>(-?\d*|-))<br>\s{17}<strong>Speed: </strong> (?<speed>(-?\d*|-))<br>\s{17}<strong>Knockback: </strong> (?<knockback>(-?\d*|-))<br>";

        private readonly LibraryService _libraryService;
        private readonly LoggingService _loggingService;

        public MobsService(IServiceProvider provider)
        {
            _libraryService = provider.GetService<LibraryService>();
            _loggingService = provider.GetService<LoggingService>();
        }

        public async Task<List<Mob>> GetObjects()
        {
            _loggingService.Log("MobsService starting");

            var mobsList = new List<Mob>();

            var totalPageNumber = await _libraryService.GetTotalPageNumberAsync(MOB_LIBRARY_URL);
            _loggingService.Log($"Total page numbers: {totalPageNumber}");

            var allTasks = new List<Task>();
            using (var slim = new SemaphoreSlim(10, 20))
            {
                foreach (var index in Enumerable.Range(1, totalPageNumber))
                {
                    await slim.WaitAsync();
                    allTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            string searchUrl = MOB_SEARCH_URL + index;
                            await foreach (string mobId in _libraryService.GetObjectIDsFromUrlAsync(searchUrl))
                            {
                                string mobUrl = MOB_LIBRARY_URL + mobId;

                                try
                                {
                                    var mob = await GetObjectFromUrl(mobUrl);
                                    mobsList.Add(mob);
                                    _loggingService.Log($"{mob.Name} downloaded");
                                }
                                catch (ArgumentException ex)
                                {
                                    _loggingService.Log($"{ex.GetType().ToString()} Error occurred loading {mobUrl}");
                                }
                            }
                        }
                        finally
                        {
                            slim.Release();
                        }
                    }));
                }
                await Task.WhenAll(allTasks).ConfigureAwait(false);
            }

            _loggingService.Log($"MobsService completed");
            return mobsList;
        }

        private async Task<Mob> GetObjectFromUrl(string url)
        {
            Mob mob = new Mob();

            string page = await _libraryService.DownloadPageAsync(url);
            var match = _libraryService.GetMatchInPage(MOB_STAT_DATA_PATTERN, page);

            if (!match.Success) throw new ArgumentException();

            mob.Name =          match.Groups["name"].Value;
            mob.Accuracy =      match.Groups["accuracy"].Value;
            mob.Avoidability =  match.Groups["avoidability"].Value;
            mob.Knockback =     match.Groups["knockback"].Value;
            mob.MagicAttack =   match.Groups["magicAttack"].Value;
            mob.MagicDefense =  match.Groups["magicDefense"].Value;
            mob.Speed =         match.Groups["speed"].Value;
            mob.WeaponAttack =  match.Groups["weaponAttack"].Value;
            mob.WeaponDefense = match.Groups["weaponDefense"].Value;

            return mob;
        }
    }
}
