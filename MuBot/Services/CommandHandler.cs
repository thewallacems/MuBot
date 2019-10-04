using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using MuBot.Bot;
using MuLibrary.Services;

namespace MuBot.Services
{
    public class CommandHandler : ServiceBase
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;
        private readonly string _prefix;

        public CommandHandler(IServiceProvider provider) : base(provider)
        {
            _provider = provider;
            _client = _provider.GetService<DiscordSocketClient>();
            _commands = _provider.GetService<CommandService>();
            _client.MessageReceived += HandleCommand;
            _prefix = BotStorage.Config.Prefix;
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            // Don't handle the command if it is a system message
            var message = parameterMessage as SocketUserMessage;
            if (message.Equals(null)) return;

            // Create a Command Context
            var context = new SocketCommandContext(_client, message);
            // Don't handle the command if it's from a bot
            if (context.User.IsBot) return;

            // Mark where the prefix ends and the command begins
            int argPos = 0;

            // Execute the Command, store the result    
            if (message.HasStringPrefix(_prefix, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _provider);

                // If the command failed, notify the user
                if (!result.IsSuccess)
                {
                    if (result.ErrorReason != "Unknown command.")
                    {
                        await message.Channel.SendMessageAsync($"{context.User.Mention}, command failed.\n{result.ErrorReason}");
                    }
                }
            }   
        }
    }
}
