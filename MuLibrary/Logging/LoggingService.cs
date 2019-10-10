using Serilog;
using Serilog.Core;
using System.IO;

namespace MuLibrary.Logging
{
    public class LoggingService
    {
        private readonly Logger Logger;
        private const string LOG_FILE = "log.txt";

        public LoggingService()
        {
            CreateOrCleanLogFile();

            Logger = new LoggerConfiguration().
                WriteTo.Console().
                WriteTo.File(LOG_FILE).
                CreateLogger();
        }

        public void Log(string message)
        {
            Logger.Information(message);
        }

        private void CreateOrCleanLogFile()
        {
            if (!File.Exists(LOG_FILE))
            {
                using FileStream file = File.Create(LOG_FILE);
            }
            else
            {
                 File.WriteAllText(LOG_FILE, string.Empty);
            }
        }
    }
}
