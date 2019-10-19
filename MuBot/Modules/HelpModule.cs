using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MuBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;

        public HelpModule(IServiceProvider provider)
        {
            _commands = provider.GetRequiredService<CommandService>();
        }

        [Command("help")]
        [Summary("displays the list of commands and their usage")]
        public async Task HelpAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("MuBot Commands")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp();

            foreach (var module in _commands.Modules)
            {
                string title = module.Name.Substring(0, module.Name.IndexOf("Module")) + " Commands";
                string description = "";

                foreach (var command in module.Commands)
                {
                    description += $"${command.Name} " + (command.Remarks ?? "") + " - " + (command.Summary ?? "no summary available") + "\n";
                }

                if (string.IsNullOrEmpty(description)) continue;
                embed.AddField(title, description);
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
