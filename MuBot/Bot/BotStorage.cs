using Newtonsoft.Json;
using System.IO;

namespace MuBot.Bot
{
    public static class BotStorage
    {
        private static readonly string _directory = @".\Config\";
        private static readonly string _file = _directory + "Config.json";

        public static Config Config { get; set; }
        
        static BotStorage()
        {
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }

            if (!File.Exists(_file))
            {
                Config = new Config();
                using StreamWriter file = new StreamWriter(_file);
                file.WriteLine(Config);
            }
            else
            {
                string json = File.ReadAllText(_file);
                Config = JsonConvert.DeserializeObject<Config>(json);
            }
        }

        public static void SaveConfig(Config config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(_file, json);
        }
    }
}
