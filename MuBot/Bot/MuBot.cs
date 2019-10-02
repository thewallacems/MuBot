using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MuBot.Services;
using MuLibrary.Services;
using MuLibrary.Services.Mobs;
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
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddSingleton<RankingService>()
                .AddSingleton<ScrapingService>()
                .AddSingleton<MobsService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<JsonService>();

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.GetRequiredService<StartupService>().StartAsync();

            serviceProvider.GetRequiredService<CommandHandler>();

            await Task.Delay(-1);
        }
    }
}
