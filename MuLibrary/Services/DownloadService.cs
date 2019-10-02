using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Services.Mobs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MuLibrary.Services
{
    public class DownloadService
    {
        private readonly MobsService _mobService;
        private readonly JsonService _jsonService;
        private readonly LoggingService _log;

        private const string RESOURCE_DIRECTORY = "./Resources";
        private const string MOB_RESOURCE_FILE = RESOURCE_DIRECTORY + "/Mobs.json";

        public DownloadService(IServiceProvider provider)
        {
            _mobService = provider.GetService<MobsService>();
            _jsonService = provider.GetService<JsonService>();
            _log = provider.GetService<LoggingService>();
        }

        public async Task StartDownloadAsync()
        {
            ValidateOrCreateFiles();

            var mobsList = await _mobService.GetObjects();
            _jsonService.WriteToJson(MOB_RESOURCE_FILE, mobsList);
        }

        private void ValidateOrCreateFiles()
        {
            if (!Directory.Exists(RESOURCE_DIRECTORY))
            {
                Directory.CreateDirectory(RESOURCE_DIRECTORY);
                _log.Log($"{RESOURCE_DIRECTORY} created");
            }

            string[] resourceFiles = new string[] { MOB_RESOURCE_FILE, };

            foreach (var resourceFile in resourceFiles)
            {
                if (!File.Exists(resourceFile))
                {
                    using FileStream file = new FileStream(resourceFile, FileMode.Create);
                    File.Create(resourceFile);

                    _log.Log($"{resourceFile} created");
                }
            }
        }
    }
}
