using Microsoft.Extensions.DependencyInjection;
using MuLibrary;
using MuLibrary.Downloading;
using MuLibrary.Library;
using MuLibrary.Library.Items;
using MuLibrary.Library.Mobs;
using MuLibrary.Library.NPCs;
using MuLibrary.Logging;
using MuLibrary.Rankings;
using System.Threading.Tasks;

namespace MuLibraryDownloader
{
    public class MuLibraryDownloader
    {
        public async Task StartAsync()
        {
            var services = new ServiceCollection()
                .AddSingleton<LoggingService>()

                .AddSingleton<ServiceBase>()
                .AddSingleton<ScrapingService>()

                .AddSingleton<ItemsService>()
                .AddSingleton<MobsService>()
                .AddSingleton<NPCsService>()

                .AddSingleton<DownloadService>()
                .AddSingleton<JsonService>()
                .AddSingleton<LibraryService>()
                .AddSingleton<RankingService>();

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.GetRequiredService<DownloadService>().StartDownloadAsync();
        }
    }
}
