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

        private readonly LibraryService _lib;
        private readonly LoggingService _log;

        public MobsService(IServiceProvider provider)
        {
            _lib = provider.GetService<LibraryService>();
            _log = provider.GetService<LoggingService>();
        }

        public async Task<List<Mob>> GetObjects()
        {
            var mobsList = new List<Mob>();

            _log.Log("Finding total page numbers...");
            var totalPageNumber = await _lib.GetTotalPageNumberAsync(MOB_LIBRARY_URL);
            _log.Log($"Total page numbers: {totalPageNumber}");

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
                            await foreach (string mobId in _lib.GetObjectIDsFromUrlAsync(searchUrl))
                            {
                                try
                                {
                                    var mob = await GetObjectFromId(mobId);
                                    mobsList.Add(mob);
                                    _log.Log($"{mob.Name} downloaded");
                                }
                                catch (ArgumentException ex)
                                {
                                    _log.Log($"{ex.GetType().ToString()} Error occurred loading { MOB_LIBRARY_URL + mobId }");
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

            _log.Log($"MobsService completed");
            return mobsList;
        }

        private async Task<Mob> GetObjectFromId(string mobId)
        {
            Mob mob = new Mob();

            string url = MOB_LIBRARY_URL + mobId;
            string page = await _lib.DownloadPageAsync(url);

            var match = _lib.GetMatchInPage(MOB_STAT_DATA_PATTERN, page);
            if (!match.Success) throw new ArgumentException();

            mob.Name =          match.Groups["name"].Value;
            mob.ImageUrl =      $"https://lib.mapleunity.com/images/mob/{mobId}.png";
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
