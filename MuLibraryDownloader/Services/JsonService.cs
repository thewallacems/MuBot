using Newtonsoft.Json;
using System.IO;

namespace MuLibraryDownloader.Services
{
    public class JsonService
    {
        public T ReadFromJson<T>(string filePath)
        {
            using StreamReader reader = new StreamReader(filePath);
            string json = reader.ReadToEnd();

            var contents = JsonConvert.DeserializeObject<T>(json);
            return contents;
        }

        public void WriteToJson(string filePath, object contents)
        {
            string json = JsonConvert.SerializeObject(contents, Formatting.Indented);

            using StreamWriter writer = new StreamWriter(filePath);
            writer.WriteLine(json);
        }
    }
}
