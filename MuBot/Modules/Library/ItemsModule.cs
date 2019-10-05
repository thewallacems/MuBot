using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MuLibrary;
using MuLibrary.Services;
using MuLibrary.Services.Items;
using System;
using System.Threading.Tasks;

namespace MuBot.Modules.Library
{
    public class ItemsModule : ModuleBase<SocketCommandContext>
    {
        private readonly LoggingService _log;
        private readonly JsonService _json;

        public ItemsModule(IServiceProvider provider)
        {
            _log = provider.GetService<LoggingService>();
            _json = provider.GetService<JsonService>();
        }

        [Command("item")]
        public async Task ItemAsync([Remainder] string itemName)
        {
            if (itemName == string.Empty)
            {
                await ReplyAsync("Please enter a item name.");
                return;
            }

            _json.ValidateOrCreateFiles();

            try
            {
                var item = _json.FindLibraryObjectInJson<Item>(Constants.ITEM_JSON_FILE_PATH, itemName);

                var embed = new EmbedBuilder()
                    .WithTitle(item.Name)
                    .WithDescription(item.ItemType)
                    .WithUrl(item.LibraryUrl)
                    .WithThumbnailUrl(item.ImageUrl)
                    .WithColor(Color.Blue)
                    .Build();

                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                _log.Log($"{ex.GetType().ToString()} occurred while creating item embed");
                await ReplyAsync($"What is a {itemName}?");
            }
        }
    }
}
