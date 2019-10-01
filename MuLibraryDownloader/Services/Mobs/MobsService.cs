using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Newtonsoft.Json;

namespace MuLibraryDownloader.Services.Mobs
{
    public class MobsService : ScrapingService
    {
        private static readonly HtmlWeb Web = new HtmlWeb();

        private static readonly string ResourcesDirectory = "./Resources/";
        private static readonly string MobFilePath = "./Resources/Mob.json";

        private static readonly string MobLibraryUrl = "https://lib.mapleunity.com/mob/";
        private static readonly string MobSearchUrl = "https://lib.mapleunity.com/mob?page=";

        private static readonly string MobStatDataPattern = @"<strong>Weapon Attack: </strong> (?<weaponAttack>(\d*|-))<br>\s{17}<strong>Magic Attack: </strong> (?<magicAttack>(\d*|-))<br>\s{17}<strong>Weapon Defense: </strong> (?<weaponDefense>(\d*|-))<br>\s{17}<strong>Magic Defense: </strong> (?<magicDefense>(\d*|-))<br>\s{17}<strong>Accuracy: </strong> (?<accuracy>(\d*|-))<br>\s{17}<strong>Avoidability: </strong> (?<avoidability>(\d*|-))<br>\s{17}<strong>Speed: </strong> (?<speed>(-?\d*|-))<br>\s{17}<strong>Knockback: </strong> (?<knockback>(\d*|-))<br>";

        public async Task<List<Mob>> GetMobs()
        {
            List<Mob> mobsList = new List<Mob>();

            var totalPageNumber = await GetTotalPageNumberAsync(Web, MobLibraryUrl);
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
                            string searchUrl = MobSearchUrl + index;
                            await foreach (string mobId in GetObjectIDsFromUrlAsync(Web, searchUrl))
                            {
                                string mobUrl = MobLibraryUrl + mobId;
                                Mob mob = await GetMobFromUrl(mobUrl);
                                mobsList.Add(mob);
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

        private static async Task<Mob> GetMobFromUrl(string url)
        {
            Mob mob = new Mob();

            string page = await DownloadPageAsync(Web, url);
            var match = GetMatchInPage(MobStatDataPattern, page);

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
