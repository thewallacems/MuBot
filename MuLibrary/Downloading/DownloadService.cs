using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Library.Items;
using MuLibrary.Library.Mobs;
using MuLibrary.Library.NPCs;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MuLibrary.Downloading
{
    public class DownloadService : ServiceBase
    {
        private readonly MobsService _mob;
        private readonly ItemsService _item;
        private readonly NPCsService _npcs;
        private readonly JsonService _json;

        public DownloadService(IServiceProvider provider) : base(provider)
        {
            _mob = provider.GetRequiredService<MobsService>();
            _item = provider.GetRequiredService<ItemsService>();
            _npcs = provider.GetRequiredService<NPCsService>();
            _json = provider.GetRequiredService<JsonService>();
        }

        public async Task<decimal> DownloadAsync()
        {
            var watch = Stopwatch.StartNew();

            _json.ValidateOrCreateFiles();

            var itemsTask = _item.GetObjects();
            var mobsTask =  _mob.GetObjects();
            var npcsTask =  _npcs.GetObjects();

            await Task.WhenAll(mobsTask, itemsTask, npcsTask);

            var mobsList =  await mobsTask;
            var itemsList = await itemsTask;
            var npcsList =  await npcsTask;

            _json.WriteToJson(Constants.MOB_JSON_FILE_PATH,     mobsList);
            _json.WriteToJson(Constants.ITEM_JSON_FILE_PATH,    itemsList);
            _json.WriteToJson(Constants.NPC_JSON_FILE_PATH,     npcsList);

            watch.Stop();

            return (decimal) watch.Elapsed.TotalMinutes;
        }
    }
}
