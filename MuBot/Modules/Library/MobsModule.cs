using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MuLibrary;
using MuLibrary.Services;
using MuLibrary.Services.Mobs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MuBot.Modules.Library
{
    public class MobsModule : ModuleBase<SocketCommandContext>
    {
        private readonly LoggingService _log;
        private readonly JsonService _json;

        public MobsModule(IServiceProvider provider)
        {
            _log = provider.GetService<LoggingService>();
            _json = provider.GetService<JsonService>();
        }

        [Command("mob")]
        public async Task MobAsync([Remainder] string mobName)
        {
            if (mobName == string.Empty)
            {
                await ReplyAsync("Please enter a mob name.");
                return;
            }

            _json.ValidateOrCreateFiles();

            try
            {
                var mob = _json.FindLibraryObjectInJson<Mob>(Constants.MOB_JSON_FILE_PATH, mobName);

                var embed = new EmbedBuilder()
                    .WithTitle(mob.Name)
                    .WithDescription(mob.ToString())
                    .WithUrl(mob.LibraryUrl)
                    .WithThumbnailUrl(mob.ImageUrl)
                    .WithColor(Color.Blue)
                    .Build();

                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                _log.Log($"{ex.GetType().ToString()} occurred while creating mob embed");
                await ReplyAsync($"What is a {mobName}?");
            }
        }
    }
}
