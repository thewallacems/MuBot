using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Logging;
using MuLibrary.Storage;
using System;
using System.Threading.Tasks;

namespace MuBot.Modules.Library
{
    public class LibraryModule : ModuleBase<SocketCommandContext>
    {
        private readonly LoggingService _log;

        private readonly ItemsDatabase _items;
        private readonly MobsDatabase _mobs;
        private readonly NPCsDatabase _npcs;

        public LibraryModule(IServiceProvider provider)
        {
            _log = provider.GetRequiredService<LoggingService>();

            _items = provider.GetRequiredService<ItemsDatabase>();
            _mobs = provider.GetRequiredService<MobsDatabase>();
            _npcs = provider.GetRequiredService<NPCsDatabase>();
        }

        [Command("npc")]
        [Summary("gets npc data")]
        [Remarks("<string npcName>")]
        public async Task NPCAsync([Remainder] string npcName)
        {
            if (npcName == string.Empty)
            {
                await ReplyAsync("Please enter a npc name.");
                return;
            }

            try
            {
                var npc = _npcs.LoadObject(npcName);

                var embed = new EmbedBuilder()
                    .WithTitle(npc.Name)
                    .WithUrl(npc.LibraryUrl)
                    .WithThumbnailUrl(npc.ImageUrl)
                    .WithColor(Color.Blue)
                    .AddField("Found At", npc.FoundAt)
                    .Build();

                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                _log.Log($"{ex.GetType().ToString()} occurred while creating npc embed");
                await ReplyAsync($"What is a `{npcName}`?");
            }
        }

        [Command("item")]
        [Summary("gets item data")]
        [Remarks("<string itemName>")]
        public async Task ItemAsync([Remainder] string itemName)
        {
            if (itemName == string.Empty)
            {
                await ReplyAsync("Please enter a item name.");
                return;
            }

            try
            {
                var item = _items.LoadObject(itemName);

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
                await ReplyAsync($"What is a `{itemName}`?");
            }
        }

        [Command("mob")]
        [Summary("gets mob data")]
        [Remarks("<string mobName>")]
        public async Task MobAsync([Remainder] string mobName)
        {
            if (mobName == string.Empty)
            {
                await ReplyAsync("Please enter a mob name.");
                return;
            }

            try
            {
                var mob = _mobs.LoadObject(mobName);

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
                await ReplyAsync($"What is a `{mobName}`?");
            }
        }
    }
}
