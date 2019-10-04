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
            _commands.CommandExecuted += CommandExecutedAsync;
            _prefix = BotStorage.Config.Prefix;
        }

        private async Task CommandExecutedAsync(Discord.Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                if (result.Error != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync(
                        $"{context.User.Mention}, command failed.\n{result.ErrorReason}");
                }
            }
        }

        private async Task HandleCommand(SocketMessage parameterMessage)
        {
            // Don't handle the command if it is a system message
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;

            // Create a Command Context
            var context = new SocketCommandContext(_client, message);
            // Don't handle the command if it's from a bot
            if (context.User.IsBot) return;

            // Mark where the prefix ends and the command begins
            int argPos = 0;

            // Execute the Command, store the result    
            if (message.HasStringPrefix(_prefix, ref argPos)) 
                await _commands.ExecuteAsync(context, argPos, _provider);
        }
    }
}
