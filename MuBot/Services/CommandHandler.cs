using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using MuBot.Bot;
using MuLibrary;
using System.Collections.Generic;

namespace MuBot.Services
{
    public class CommandHandler : ServiceBase
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;
        private readonly List<SocketUser> _usersOnCooldown;
        private readonly string _prefix;

        public CommandHandler(IServiceProvider provider) : base(provider)
        {
            _provider = provider;
            _client = _provider.GetRequiredService<DiscordSocketClient>();
            _commands = _provider.GetRequiredService<CommandService>();
            _client.MessageReceived += HandleCommand;
            _commands.CommandExecuted += CommandExecutedAsync;

            _usersOnCooldown = new List<SocketUser>();
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
            if (!(parameterMessage is SocketUserMessage message)) return;

            // Create a Command Context
            var context = new SocketCommandContext(_client, message);
            // Don't handle the command if it's from a bot
            if (context.User.IsBot) return;

            if (_usersOnCooldown.Contains(context.User))
                return;

            // Mark where the prefix ends and the command begins
            int argPos = 0;

            // Execute the Command, store the result    
            if (message.HasStringPrefix(_prefix, ref argPos))
            {
                _usersOnCooldown.Add(context.User);
                Task task = Task.Run(async () =>
                {
                    await Task.Delay(2 * 1000);
                    _usersOnCooldown.Remove(context.User);
                });

                await _commands.ExecuteAsync(context, argPos, _provider);
            }
        }
    }
}
