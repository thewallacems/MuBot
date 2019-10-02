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

        public DownloadService(IServiceProvider provider)
        {
            _mobService = provider.GetService<MobsService>();
            _jsonService = provider.GetService<JsonService>();
            _log = provider.GetService<LoggingService>();
        }

        public async Task StartDownloadAsync()
        {
            _log.Log("Validating files for download...");
            ValidateOrCreateFiles();

            _log.Log("Starting download of Mob data");
            var mobsList = await _mobService.GetObjects();

            _log.Log("Writing Mob data to JSON");
            _jsonService.WriteToJson(Constants.MOB_JSON_FILE_PATH, mobsList);


            _log.Log("Finished download of Mob data");
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

            string[] resourceFiles = new string[] { Constants.MOB_JSON_FILE_PATH, };

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
