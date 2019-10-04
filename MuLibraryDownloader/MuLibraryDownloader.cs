using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Services;
using MuLibrary.Services.Items;
using MuLibrary.Services.Mobs;
using MuLibrary.Services.Rankings;
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

                .AddSingleton<DownloadService>()
                .AddSingleton<JsonService>()
                .AddSingleton<LibraryService>()
                .AddSingleton<RankingService>();

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.GetRequiredService<DownloadService>().StartDownloadAsync();
        }
    }
}
