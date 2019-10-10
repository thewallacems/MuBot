using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Library.Items;
using MuLibrary.Library.Mobs;
using MuLibrary.Library.NPCs;
using System;
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
            _mob = provider.GetService<MobsService>();
            _item = provider.GetService<ItemsService>();
            _npcs = provider.GetService<NPCsService>();
            _json = provider.GetService<JsonService>();
        }

        public async Task StartDownloadAsync()
        {
            ValidateOrCreateFiles();

            _log.Log("Starting download of library data");
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

            _log.Log("Finished download of library data");
        }

        private void ValidateOrCreateFiles()
        {
            if (!Directory.Exists(Constants.RESOURCES_FOLDER_PATH))
            {
                Directory.CreateDirectory(Constants.RESOURCES_FOLDER_PATH);
                _log.Log($"{Constants.RESOURCES_FOLDER_PATH} created");
            }
            else
            {
                _log.Log($"Found {Constants.RESOURCES_FOLDER_PATH} at {Path.GetFullPath(Constants.RESOURCES_FOLDER_PATH)}");
            }

            string[] resourceFiles = new string[] { Constants.MOB_JSON_FILE_PATH, Constants.ITEM_JSON_FILE_PATH, };

            foreach (var resourceFile in resourceFiles)
            {
                if (!File.Exists(resourceFile))
                {
                    using FileStream file = new FileStream(resourceFile, FileMode.Create);
                    File.Create(resourceFile);

                    _log.Log($"{resourceFile} created");
                }
                else
                {
                    _log.Log($"Found {resourceFile} at {Path.GetFullPath(resourceFile)}");
                }
            }
        }
    }
}
