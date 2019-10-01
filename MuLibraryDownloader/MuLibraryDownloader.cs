using Microsoft.Extensions.DependencyInjection;
using MuLibraryDownloader.Services;
using MuLibraryDownloader.Services.Mobs;
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
                .AddSingleton<MobsService>();

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.GetRequiredService<DownloadService>().StartDownloadAsync();
        }
    }
}
