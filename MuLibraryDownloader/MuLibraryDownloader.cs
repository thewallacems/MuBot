using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Services;
using MuLibrary.Services.Mobs;
using System.Threading.Tasks;

namespace MuLibraryDownloader
{
    public class MuLibraryDownloader
    {
        public async Task StartAsync()
        {
            var services = new ServiceCollection()
                .AddSingleton<LibraryService>()
                .AddSingleton<JsonService>()
                .AddSingleton<DownloadService>()
                .AddSingleton<MobsService>()
                .AddSingleton<LoggingService>();

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.GetRequiredService<DownloadService>().StartDownloadAsync();
        }
    }
}
