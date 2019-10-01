using Microsoft.Extensions.DependencyInjection;
using MuLibraryDownloader.Services.Mobs;
using System;
using System.IO;
using System.Threading.Tasks;
using static MuLibrary.Utils.Miscellaneous;

namespace MuLibraryDownloader.Services
{
    public class DownloadService
    {
        private readonly MobsService _mobService;
        private readonly JsonService _jsonService;

        private const string RESOURCE_DIRECTORY = "./Resources";
        private const string MOB_RESOURCE_FILE = RESOURCE_DIRECTORY + "/Mobs.json";

        public DownloadService(IServiceProvider provider)
        {
            _mobService = provider.GetService<MobsService>();
            _jsonService = provider.GetService<JsonService>();
        }

        public async Task StartDownloadAsync()
        {
            ValidateOrCreateFiles();

            var mobsList = await _mobService.GetObjects();
            _jsonService.WriteToJson(MOB_RESOURCE_FILE, mobsList);
            PrintToConsole($"Mobs finished downloading");
        }

        private static void ValidateOrCreateFiles()
        {
            if (!Directory.Exists(RESOURCE_DIRECTORY))
            {
                Directory.CreateDirectory(RESOURCE_DIRECTORY);
            }

            string[] resourceFiles = new string[] { MOB_RESOURCE_FILE, };

            foreach (var resourceFile in resourceFiles)
            {
                if (!File.Exists(resourceFile))
                {
                    using FileStream file = new FileStream(resourceFile, FileMode.Create);
                    File.Create(resourceFile);
                }
            }
        }
    }
}
