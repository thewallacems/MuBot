using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MuLibrary.Services
{
    public class JsonService : ServiceBase
    {
        public JsonService(IServiceProvider provider) : base(provider) { }

        public T ReadFromJson<T>(string filePath)
        {
            _log.Log($"Reading JSON from {filePath}");

            var serializer = new JsonSerializer();
            using StreamReader sr = new StreamReader(filePath);
            using JsonTextReader reader = new JsonTextReader(sr);
            var contents = serializer.Deserialize<T>(reader);
            
            return contents;
        }

        public T FindLibraryObjectInJson<T>(string filePath, string name) where T : ILibraryObject
        {
            var list = ReadFromJson<IEnumerable<T>>(filePath);

            var query = from m in list
                        where m.Name.ToLower() == name.ToLower()
                        select m;

            if (!query.Any()) throw new ArgumentException();

            var mob = query.FirstOrDefault();
            return mob;
        }

        public void WriteToJson(string filePath, object contents)
        {
            string json = JsonConvert.SerializeObject(contents, Formatting.Indented);

            using StreamWriter writer = new StreamWriter(filePath);
            writer.WriteLine(json);
        }

        public void ValidateOrCreateFiles()
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
