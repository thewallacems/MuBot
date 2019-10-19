using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MuLibrary;
using MuLibrary.Logging;
using MuLibrary.Rankings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MuBot.Modules
{
    public class RankingsModule : ModuleBase<SocketCommandContext>
    {
        private readonly RankingService _rankings;
        private readonly LoggingService _log;
        private readonly DiscordSocketClient _client;

        public RankingsModule(IServiceProvider provider)
        {
            _rankings = provider.GetRequiredService<RankingService>();
            _log = provider.GetRequiredService<LoggingService>();
            _client = provider.GetRequiredService<DiscordSocketClient>();
        }

        [Command("jobs")]
        [RequireOwner]
        [Summary("displays the count of each job")]
        public async Task JobsAsync()
        {
            string[] warriorJobTitles =     new string[] { "Fighter", "Crusader", "Hero", "Spearman", "Dragon Knight", "Dark Knight", "Page", "White Knight", "Paladin", "Warrior" }; // 10
            string[] magicianJobTitles =    new string[] { "Wizard (Fire/Poison)", "Mage (Fire/Poison)", "Archmage (Fire/Poison)", "Wizard (Ice/Lightning)", "Mage (Ice/Lightning)", "Archmage (Ice/Lightning)", "Cleric", "Priest", "Bishop", "Magician" }; // 10
            string[] thiefJobTitles =       new string[] { "Bandit", "Chief Bandit", "Shadower", "Assassin", "Hermit", "Night Lord", "Thief" }; // 7
            string[] bowmanJobTitles =      new string[] { "Crossbowman", "Sniper", "Marksman", "Hunter", "Ranger", "Bowmaster", "Bowman" }; // 7
            string[] pirateJobTitles =      new string[] { "Gunslinger", "Outlaw", "Corsair", "Brawler", "Marauder", "Buccaneer", "Pirate" }; // 7 
            string[] oddJobTitles =         new string[] { "Beginner (30+)", "Beginner (70+)", "Beginner (120+)", "Islander (30+)", "Islander (70+)", "Islander (120+)", "Islander", "Camper (30+)", "Camper (70+)", "Camper (120+)", "Camper", }; // 11
            string[] beginnerTitles =       new string[] { "Beginner", }; // 1

            var watch = Stopwatch.StartNew();
            var maplersList = await _rankings.GetMaplers();
            watch.Stop();

            decimal totalNumberOfCharacters = maplersList.Count;

            var generalEmbed =  CreateGeneralEmbed(watch.Elapsed.TotalMinutes);

            var warriorEmbed =  CreateJobEmbed("Warrior",   warriorJobTitles,   maplersList, totalNumberOfCharacters);
            var magicianEmbed = CreateJobEmbed("Magician",  magicianJobTitles,  maplersList, totalNumberOfCharacters);
            var thiefEmbed =    CreateJobEmbed("Thief",     thiefJobTitles,     maplersList, totalNumberOfCharacters);
            var bowmanEmbed =   CreateJobEmbed("Bowman",    bowmanJobTitles,    maplersList, totalNumberOfCharacters);
            var pirateEmbed =   CreateJobEmbed("Pirate",    pirateJobTitles,    maplersList, totalNumberOfCharacters);
            var beginnerEmbed = CreateJobEmbed("Beginner",  beginnerTitles,     maplersList, totalNumberOfCharacters);
            var oddJobEmbed =   CreateJobEmbed("Odd Job",   oddJobTitles,       maplersList, totalNumberOfCharacters);

            Embed[] embeds = new Embed[] { generalEmbed, magicianEmbed, warriorEmbed, thiefEmbed, bowmanEmbed, pirateEmbed, beginnerEmbed, oddJobEmbed, };
            foreach (Embed embed in embeds) { await ReplyAsync(embed: embed); await Task.Delay(1750); }
        }

        private Embed CreateGeneralEmbed(double minutesElapsed)
        {
            var minutes = ((int) Math.Truncate(minutesElapsed)).ToString();
            var seconds = ((int) Math.Truncate((minutesElapsed - Math.Truncate(minutesElapsed)) * 60)).ToString();

            var owner = _client.GetUser(476226626464645135);

            var embedAuthor = new EmbedAuthorBuilder()
                                .WithName($"{owner.Username}#{owner.Discriminator}")
                                .WithIconUrl(owner.GetAvatarUrl());

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
            decimal averageClassLevel = (decimal) maplersList.Where(x => jobTitles.Contains(x.Job)).Average(x => x.Level);
            decimal totalClassCount = maplersList.Count(x => jobTitles.Contains(x.Job));
            decimal totalClassToTotalCharactersRatio = (totalClassCount / totalNumberOfCharacters) * 100.00m;

            var embed = new EmbedBuilder()
                .WithTitle(classTitle)
                .WithDescription($"{totalClassCount} ({ totalClassToTotalCharactersRatio:F}%)\nAverage Level: {averageClassLevel:F}")
                .WithFooter("Obtained through https://mapleunity.com/rankings/all")
                .WithColor(Color.Blue)
                .WithTimestamp(DateTimeOffset.UtcNow);

            int maplersListLength = maplersList.Count;

            foreach (var jobTitle in jobTitles)
            {
                decimal totalJobCount = 0.00m;
                decimal totalLevelCount = 0.00m;

                for (int currentIndex = 0; currentIndex < maplersListLength; currentIndex++)
                {
                    var mapler = maplersList[currentIndex];
                    if (mapler.Job == jobTitle)
                    {
                        maplersList.Remove(mapler);

                        totalJobCount += 1.00m;
                        totalLevelCount += mapler.Level;

                        currentIndex--;
                        maplersListLength--;
                    }
                }

                decimal averageLevel = 0.00m;

                if (totalJobCount != 0)
                {
                    averageLevel = totalLevelCount / totalJobCount;
                }

                decimal ratioOfJobBranchToTotalCharacters = (totalJobCount / totalNumberOfCharacters) * 100.00m;

                embed.AddField(jobTitle, $"{(int) totalJobCount} ({ ratioOfJobBranchToTotalCharacters:F}%)\nAverage Level: {averageLevel:F}", true);
            }

            _log.Log($"{classTitle} embed created");
            return embed.Build();
        }
    }
}
