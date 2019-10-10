using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MuLibrary.Library.Mobs
{
    public class MobsService : ServiceBase
    {
        private const string MOB_LIBRARY_URL = "https://lib.mapleunity.com/mob/";
        private const string MOB_SEARCH_URL = "https://lib.mapleunity.com/mob?page=";
        private readonly Regex MOB_STAT_DATA_REGEX = new Regex(@"<h4 class=""mt-2"">\s{13}(?<name>[^\n]*)<br>\s{13}Level: [0-9]*\s{9}</h4>[\S\s]*<strong>Weapon Attack: </strong> (?<weaponAttack>(-?[0-9]*|-))<br>\s{17}<strong>Magic Attack: </strong> (?<magicAttack>(-?[0-9]*|-))<br>\s{17}<strong>Weapon Defense: </strong> (?<weaponDefense>(-?[0-9]*|-))<br>\s{17}<strong>Magic Defense: </strong> (?<magicDefense>(-?[0-9]*|-))<br>\s{17}<strong>Accuracy: </strong> (?<accuracy>(-?[0-9]*|-))<br>\s{17}<strong>Avoidability: </strong> (?<avoidability>(-?[0-9]*|-))<br>\s{17}<strong>Speed: </strong> (?<speed>(-?[0-9]*|-))<br>\s{17}<strong>Knockback: </strong> (?<knockback>(-?[0-9]*|-))<br>");

        private readonly LibraryService _lib;

        public MobsService(IServiceProvider provider) : base(provider)
        {
            _lib = provider.GetService<LibraryService>();
        }

        public async Task<List<Mob>> GetObjects()
        {
            var mobsList = await _lib.GetObjects(MOB_LIBRARY_URL, MOB_SEARCH_URL, GetObjectFromId);
            return mobsList;
        }

        private async Task<Mob> GetObjectFromId(string mobId)
        {
            Mob mob = new Mob();

            string url = MOB_LIBRARY_URL + mobId;
            string page = await _lib.DownloadPageAsync(url);

            var match = _lib.GetMatchInPage(MOB_STAT_DATA_REGEX, page);
            if (!match.Success) { _log.Log($"Error occured downloading {mobId} at {url}"); throw new ArgumentException(); }

            mob.Name =          HttpUtility.HtmlDecode(match.Groups["name"].Value);
            mob.ImageUrl =      $"https://lib.mapleunity.com/images/mob/{mobId}.png";
            mob.LibraryUrl =    $"https://lib.mapleunity.com/mob/{mobId}";
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
