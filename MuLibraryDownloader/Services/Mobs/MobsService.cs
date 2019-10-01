using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MuLibraryDownloader.Services.Mobs
{
    public class MobsService : ILibraryObjectService<Mob>
    {
        private const string MOB_LIBRARY_URL = "https://lib.mapleunity.com/mob/";
        private const string MOB_SEARCH_URL = "https://lib.mapleunity.com/mob?page=";
        private const string MOB_STAT_DATA_PATTERN = @"<strong>Weapon Attack: </strong> (?<weaponAttack>(\d*|-))<br>\s{17}<strong>Magic Attack: </strong> (?<magicAttack>(\d*|-))<br>\s{17}<strong>Weapon Defense: </strong> (?<weaponDefense>(\d*|-))<br>\s{17}<strong>Magic Defense: </strong> (?<magicDefense>(\d*|-))<br>\s{17}<strong>Accuracy: </strong> (?<accuracy>(\d*|-))<br>\s{17}<strong>Avoidability: </strong> (?<avoidability>(\d*|-))<br>\s{17}<strong>Speed: </strong> (?<speed>(-?\d*|-))<br>\s{17}<strong>Knockback: </strong> (?<knockback>(\d*|-))<br>";

        private readonly IServiceProvider _provider;
        private readonly LibraryService _libraryService;

        public MobsService(IServiceProvider provider)
        {
            _provider = provider;
            _libraryService = provider.GetService<LibraryService>();
        }

        public async Task<List<Mob>> GetObjects()
        {
            var mobsList = new List<Mob>();

            var totalPageNumber = await _libraryService.GetTotalPageNumberAsync(MOB_LIBRARY_URL);
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
                                    Mob mob = await GetObjectFromUrl(mobUrl, mobId);
                                    mobsList.Add(mob);
                                    Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Mob downloaded");
                                }
                                catch (ArgumentException ex)
                                {
                                    Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Error loading mob at {mobUrl}");
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

            return mobsList;
        }

        private async Task<Mob> GetObjectFromUrl(string url, string id)
        {
            Mob mob = new Mob();

            string page = await _libraryService.DownloadPageAsync(url);
            var match = _libraryService.GetMatchInPage(MOB_STAT_DATA_PATTERN, page);

            if (match == Match.Empty) throw new ArgumentException();

            mob.ID              = id;
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
