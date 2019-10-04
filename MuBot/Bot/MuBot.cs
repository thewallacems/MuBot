using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MuBot.Services;
using MuLibrary.Services;
using MuLibrary.Services.Items;
using MuLibrary.Services.Mobs;
using MuLibrary.Services.Rankings;
using System.Threading.Tasks;

namespace MuBot.Bot
{
    public class MuBot
    {
        public async Task StartAsync()
        {
            var services = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 1000
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Verbose,
                    CaseSensitiveCommands = false,
                    ThrowOnError = true
                }))
                .AddSingleton<LoggingService>()
                .AddSingleton<ServiceBase>()

                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddSingleton<ScrapingService>()

                .AddSingleton<ItemsService>()
                .AddSingleton<MobsService>()

                .AddSingleton<DownloadService>()
                .AddSingleton<JsonService>()
                .AddSingleton<LibraryService>()
                .AddSingleton<RankingService>();

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.GetRequiredService<StartupService>().StartAsync();

            serviceProvider.GetRequiredService<CommandHandler>();

            await Task.Delay(-1);
        }
    }
}
