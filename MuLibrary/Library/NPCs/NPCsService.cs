using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MuLibrary.Library.NPCs
{
    public class NPCsService : ServiceBase
    {
        private const string NPC_LIBRARY_URL = "https://lib.mapleunity.com/npc/";
        private const string NPC_SEARCH_URL = "https://lib.mapleunity.com/npc?page=";
        private readonly Regex NPC_CHECK_REGEX = new Regex(@"<h2 class=""mt-2"">(?<name>[^\n]*)</h2>\s*Found at:\s*((?<map>Nowhere!)\s*|(<a href=""/map/\d{9}"">(?<map>[^\n]*)</a><br>\s*)*)<br><br>");

        private readonly LibraryService _lib;

        public NPCsService(IServiceProvider provider) : base(provider)
        {
            _lib = provider.GetService<LibraryService>();
        }

        public async Task<List<NPC>> GetObjects()
        {
            var npcsList = await _lib.GetObjects(NPC_LIBRARY_URL, NPC_SEARCH_URL, GetObjectFromId);
            return npcsList;
        }

        private async Task<NPC> GetObjectFromId(string npcId)
        {
            var npc = new NPC();

            string url = NPC_LIBRARY_URL + npcId;
            string page = await _lib.DownloadPageAsync(url);

            var match = _lib.GetMatchInPage(NPC_CHECK_REGEX, page);
            if (!match.Success) { _log.Log($"Error occured downloading {npcId} at {url}"); throw new ArgumentException(); }

            npc.Name =          HttpUtility.HtmlDecode(match.Groups["name"].Value);
            npc.ImageUrl =      $"https://lib.mapleunity.com/images/npc/{npcId}.png";
            npc.LibraryUrl =    $"https://lib.mapleunity.com/npc/{npcId}";

            var maps = string.Empty;
            foreach (Capture capture in match.Groups["map"].Captures)
            {
                maps += $"{capture.Value}\n";
            }

            npc.FoundAt = maps;

            return npc;
        }
    }
}
