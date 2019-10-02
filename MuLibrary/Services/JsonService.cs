using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Services.Mobs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MuLibrary.Services
{
    public class JsonService
    {
        private readonly LoggingService _log;

        public JsonService(IServiceProvider provider)
        {
            _log = provider.GetService<LoggingService>();
        }

        public T ReadFromJson<T>(string filePath)
        {
            _log.Log($"Reading JSON from {filePath}");

            var serializer = new JsonSerializer();
            using StreamReader sr = new StreamReader(filePath);
            using JsonTextReader reader = new JsonTextReader(sr);
            var contents = serializer.Deserialize<T>(reader);

            return contents;
        }

        public Mob FindLibraryObjectInJson(string filePath, string name)
        {
            var mobsList = ReadFromJson<IEnumerable<Mob>>(filePath);

            var query = from m in mobsList
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
    }
}
