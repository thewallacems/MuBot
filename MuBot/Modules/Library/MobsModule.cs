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

            if (!ValidateOrCreateFiles())
            {
                await ReplyAsync("Please run the Mob downloader.");
                return;
            }

            _log.Log("Mob.json valid.");
            try
            {
                var mob = _json.FindLibraryObjectInJson(Constants.MOB_JSON_FILE_PATH, mobName);

                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithTitle(mob.Name)
                    .WithDescription(mob.ToString())
                    .WithImageUrl(mob.ImageUrl)
                    .Build();

                await ReplyAsync(embed: embed);
            }
            catch
            {
                await ReplyAsync($"What is a {mobName}?");
            }
        }

        private bool ValidateOrCreateFiles()
        {
            if (!Directory.Exists(Constants.RESOURCES_FOLDER_PATH))
            {
                Directory.CreateDirectory(Constants.RESOURCES_FOLDER_PATH);
                _log.Log("Resources folder not found.");

                return false;
            } 
            else if (!File.Exists(Constants.MOB_JSON_FILE_PATH))
            {
                using FileStream file = File.Create(Constants.MOB_JSON_FILE_PATH);
                _log.Log("Mob.json not found.");
                
                return false;
            } 
            else
            {
                using FileStream file = File.Open(Constants.MOB_JSON_FILE_PATH, FileMode.Open);
                if (file.Length == 0)
                {
                    _log.Log("Mob.json has no contents.");

                    return false;
                }

                return true;
            }
        }
    }
}
