using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MuLibrary;
using MuLibrary.Downloading;
using MuLibrary.Library.NPCs;
using MuLibrary.Logging;
using System;
using System.Threading.Tasks;

namespace MuBot.Modules.Library
{
    public class NPCsModule : ModuleBase<SocketCommandContext>
    {
        private readonly LoggingService _log;
        private readonly JsonService _json;

        public NPCsModule(IServiceProvider provider)
        {
            _log = provider.GetService<LoggingService>();
            _json = provider.GetService<JsonService>();
        }

        [Command("npc")]
        public async Task NPCAsync([Remainder] string npcName)
        {
            if (npcName == string.Empty)
            {
                await ReplyAsync("Please enter a npc name.");
                return;
            }

            _json.ValidateOrCreateFiles();

            try
            {
                var npc = _json.FindLibraryObjectInJson<NPC>(Constants.NPC_JSON_FILE_PATH, npcName);

                var embed = new EmbedBuilder()
                    .WithTitle(npc.Name)
                    .WithUrl(npc.LibraryUrl)
                    .WithThumbnailUrl(npc.ImageUrl)
                    .WithColor(Color.Blue)
                    .Build();

                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                _log.Log($"{ex.GetType().ToString()} occurred while creating npc embed");
                await ReplyAsync($"What is a {npcName}?");
            }
        }
    }
}
