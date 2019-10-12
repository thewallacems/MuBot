using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MuLibrary;
using MuLibrary.Downloading;
using MuLibrary.Logging;
using System;
using System.Threading.Tasks;

namespace MuBot.Modules.Library
{
    public class DownloadModule : ModuleBase<SocketCommandContext>
    {
        private readonly DownloadService _download;

        public DownloadModule(IServiceProvider provider)
        {
            _download = provider.GetService<DownloadService>();
        }

        [Command("download")]
        [Remarks("Downloads the data from lib.mapleunity.com")]
        public async Task DownloadAsync()
        {
            if (Context.User.Id != Constants.OWNER_ID)
            {
                await ReplyAsync($"You lack permissions to use this command.");
                return;
            }

            decimal minutesElapsed = await _download.DownloadAsync();
            await ReplyAsync($"Download completed in {minutesElapsed:F}!");
        }
    }
}
