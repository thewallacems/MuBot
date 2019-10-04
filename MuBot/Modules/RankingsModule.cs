using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Services;
using MuLibrary.Services.Rankings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MuBot.Modules
{
    public class RankingsModule : ModuleBase<SocketCommandContext>
    {
        private readonly RankingService _rankings;
        private readonly LoggingService _log;

        public RankingsModule(IServiceProvider provider)
        {
            _rankings = provider.GetService<RankingService>();
            _log = provider.GetService<LoggingService>();
        }

        [Command("jobs")]
        [Remarks("Displays the count of each job")]
        public async Task JobsAync(int pages = -1)
        {
            if (Context.User.Id != 476226626464645135)
            {
                await ReplyAsync("You are lacking permissions to use this command.");
                return;
            }

            var watch = Stopwatch.StartNew();
            var maplersList = await _rankings.GetMaplers(pages);
            watch.Stop();

            decimal totalNumberOfCharacters = maplersList.Count;

            string[] warriorJobTitles =     new string[] { "Fighter", "Crusader", "Hero", "Spearman", "Dragon Knight", "Dark Knight", "Page", "White Knight", "Paladin", "Warrior" };
            string[] magicianJobTitles =    new string[] { "Wizard (Fire/Poison)", "Mage (Fire/Poison)", "Archmage (Fire/Poison)", "Wizard (Ice/Lightning)", "Mage (Ice/Lightning)", "Archmage (Ice/Lightning)", "Cleric", "Priest", "Bishop", "Magician" };
            string[] thiefJobTitles =       new string[] { "Bandit", "Chief Bandit", "Shadower", "Assassin", "Hermit", "Night Lord", "Thief" };
            string[] oddJobTitles =         new string[] { "Beginner (30+)", "Beginner (70+)", "Beginner (120+)", "Beginner" };
            string[] bowmanJobTitles =      new string[] { "Crossbowman", "Sniper", "Marksman", "Hunter", "Ranger", "Bowmaster", "Bowman" };
            string[] pirateJobTitles =      new string[] { "Gunslinger", "Outlaw", "Corsair", "Brawler", "Marauder", "Buccaneer", "Pirate" };

            var generalEmbed =  CreateGeneralEmbed(watch.Elapsed.TotalMinutes);
            var magicianEmbed = CreateJobEmbed("Magician",  magicianJobTitles,  maplersList, totalNumberOfCharacters);
            var warriorEmbed =  CreateJobEmbed("Warrior",   warriorJobTitles,   maplersList, totalNumberOfCharacters);
            var thiefEmbed =    CreateJobEmbed("Thief",     thiefJobTitles,     maplersList, totalNumberOfCharacters);
            var oddJobEmbed =   CreateJobEmbed("Odd Job",   oddJobTitles,       maplersList, totalNumberOfCharacters);
            var bowmanEmbed =   CreateJobEmbed("Bowman",    bowmanJobTitles,    maplersList, totalNumberOfCharacters);
            var pirateEmbed =   CreateJobEmbed("Pirate",    pirateJobTitles,    maplersList, totalNumberOfCharacters);

            Embed[] embeds = new Embed[] { generalEmbed, warriorEmbed, magicianEmbed, thiefEmbed, oddJobEmbed, bowmanEmbed, pirateEmbed, };
            foreach (Embed embed in embeds) { await ReplyAsync(embed: embed); Thread.Sleep(1250); }
        }

        private static Embed CreateGeneralEmbed(double minutesElapsed)
        {
            var minutes = ((int) Math.Truncate(minutesElapsed)).ToString();
            var seconds = ((int) Math.Truncate((minutesElapsed - Math.Truncate(minutesElapsed)) * 60)).ToString();

            var embedAuthor = new EmbedAuthorBuilder()
                                .WithName("Jacob#0828")
                                .WithIconUrl("https://cdn.discordapp.com/avatars/476226626464645135/91bb9cf69a7d6939caeab9c653d13ff9.png?size=128");

            var generalEmbed = new EmbedBuilder()
                            .WithTitle("MuBot MapleUnity Rankings Scraper")
                            .WithDescription($"This program ran in { minutes } minutes and { seconds } seconds.")
                            .WithAuthor(embedAuthor)
                            .WithColor(Color.Blue)
                            .WithTimestamp(DateTimeOffset.UtcNow)
                            .Build();


            return generalEmbed;
        }

        private Embed CreateJobEmbed(string classTitle, string[] jobTitles, List<Mapler> maplersList, decimal totalNumberOfCharacters)
        {
            decimal totalClassCount = maplersList.Count(x => jobTitles.Contains(x.Job));
            decimal ratioOfJobToTotalCharacters = (totalClassCount / totalNumberOfCharacters) * 100.00m;

            var embed = new EmbedBuilder()
                .WithTitle(classTitle)
                .WithDescription($"Total {classTitle}: {totalClassCount} ({ ratioOfJobToTotalCharacters.ToString("F") }%)")
                .WithFooter("Obtained through https://mapleunity.com/rankings/all")
                .WithColor(Color.Blue)
                .WithTimestamp(DateTimeOffset.UtcNow);

            foreach (var jobTitle in jobTitles)
            {
                decimal totalJobCount = GetTotalJobCount(jobTitle, maplersList);
                decimal ratioOfJobBranchToTotalCharacters = (totalJobCount / totalNumberOfCharacters) * 100.00m;

                embed.AddField(jobTitle, $"{totalJobCount} ({ ratioOfJobBranchToTotalCharacters.ToString("F") }%)", true);
            }

            _log.Log($"{classTitle} embed created");
            return embed.Build();
        }

        private int GetTotalJobCount(string jobTitle, List<Mapler> maplersList)
        {
            _log.Log($"Getting total job count for {jobTitle}");

            int totalJobCount = 0;
            int maplersListLength = maplersList.Count;

            for (int i = 0; i < maplersListLength; i++)
            {
                if (maplersList[i].Job == jobTitle)
                {
                    maplersList.Remove(maplersList[i]);

                    totalJobCount++;
                    i--;
                    maplersListLength--;
                }
            }

            return totalJobCount;
        }
    }
}
