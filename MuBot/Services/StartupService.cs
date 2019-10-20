using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MuBot.Bot;
using MuLibrary;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace MuBot.Services
{
    public class StartupService : ServiceBase
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private string _token;
        private string _prefix;

        public StartupService(IServiceProvider provider) : base(provider)
        {
            _provider = provider;
            
            _client = _provider.GetRequiredService<DiscordSocketClient>();
            _commands = _provider.GetRequiredService<CommandService>();
            
            _token = BotStorage.Config.Token;
            _prefix = BotStorage.Config.Prefix;
        }

        public async Task StartAsync()
        {
            string token = _token;
            string prefix = _prefix;

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Please enter a bot token.");
                
                token = Console.ReadLine();
                _token = token;
                
                BotStorage.Config.Token = token;
                BotStorage.SaveConfig(BotStorage.Config);
            }

            if (string.IsNullOrEmpty(prefix))
            {
                Console.WriteLine("Please enter a bot prefix.");
                
                prefix = Console.ReadLine();
                _prefix = prefix;

                BotStorage.Config.Prefix = prefix;
                BotStorage.SaveConfig(BotStorage.Config);
            }

            _client.Log += _client_Log;

            while (true)
            {
                try
                {
                    await _client.LoginAsync(TokenType.Bot, token);
                    break;
                }
                catch (Exception ex ) when (ex is ArgumentException || ex is Discord.Net.HttpException)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Supplied token invalid. Please re-enter your token.");

                    token = Console.ReadLine();
                    _token = token;
                    
                    BotStorage.Config.Token = token;
                    BotStorage.SaveConfig(BotStorage.Config);
                    
                    continue;
                }
            }

            await _client.StartAsync();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private Task _client_Log(LogMessage arg)
        {
            _log.Log(arg.Message);
            return Task.CompletedTask;
        }
    }
}
