using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static MuLibrary.Utils.Miscellaneous;

namespace MuBot.Modules
{
    public class RankingsModule : ModuleBase<SocketCommandContext>
    {
        public readonly RankingService _rankings;

        public RankingsModule(IServiceProvider provider)
        {
            _rankings = provider.GetService<RankingService>();
        }

        [Command("jobs")]
        [Remarks("Displays the count of each job")]
        public async Task JobsAync()
        {
            var watch = Stopwatch.StartNew();
            
            Dictionary<string, int> jobToJobCount = await _rankings.GetJobs();

            watch.Stop();

            int totalNumberOfCharacters = GetTotalNumberOfCharacters(jobToJobCount);

            string[] warriorJobTitles =     new string[] { "Fighter", "Crusader", "Hero", "Spearman", "Dragon Knight", "Dark Knight", "Page", "White Knight", "Paladin", "Warrior" };
            string[] magicianJobTitles =    new string[] { "Wizard (Fire/Poison)", "Mage (Fire/Poison)", "Archmage (Fire/Poison)", "Wizard (Ice/Lightning)", "Mage (Ice/Lightning)", "Archmage (Ice/Lightning)", "Cleric", "Priest", "Bishop", "Magician" };
            string[] bowmanJobTitles =      new string[] { "Crossbowman", "Sniper", "Marksman", "Hunter", "Ranger", "Bowmaster", "Bowman" };
            string[] thiefJobTitles =       new string[] { "Bandit", "Chief Bandit", "Shadower", "Assassin", "Hermit", "Night Lord", "Thief" };
            string[] pirateJobTitles =      new string[] { "Gunslinger", "Outlaw", "Corsair", "Brawler", "Marauder", "Buccaneer", "Pirate" };
            string[] oddJobTitles =         new string[] { "Beginner (30+)", "Beginner (70+)", "Beginner (120+)", "Beginner" };
            

            PrintToConsole("Creating embeds...");

            var generalEmbed =  CreateGeneralEmbed(watch.Elapsed.TotalMinutes);
            var warriorEmbed =  CreateJobEmbed("Warrior",   warriorJobTitles,   jobToJobCount, totalNumberOfCharacters);
            var magicianEmbed = CreateJobEmbed("Magician",  magicianJobTitles,  jobToJobCount, totalNumberOfCharacters);
            var bowmanEmbed =   CreateJobEmbed("Bowman",    bowmanJobTitles,    jobToJobCount, totalNumberOfCharacters);
            var thiefEmbed =    CreateJobEmbed("Thief",     thiefJobTitles,     jobToJobCount, totalNumberOfCharacters);
            var pirateEmbed =   CreateJobEmbed("Pirate",    pirateJobTitles,    jobToJobCount, totalNumberOfCharacters);
            var oddJobEmbed =   CreateJobEmbed("Odd Job",   oddJobTitles,       jobToJobCount, totalNumberOfCharacters);

            Embed[] embeds = new Embed[] { generalEmbed, warriorEmbed, magicianEmbed, bowmanEmbed, thiefEmbed, pirateEmbed, oddJobEmbed, };

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

            PrintToConsole("General embed created");

            return generalEmbed;
        }

        private static Embed CreateJobEmbed(string job, string[] jobTitles, Dictionary<string, int> jobToJobCount, decimal totalNumberOfCharacters)
        {
            decimal totalJobCountInClass = GetTotalJobCountInClass(jobTitles, jobToJobCount);
            decimal ratioOfJobToTotalCharacters = (totalJobCountInClass / totalNumberOfCharacters) * 100.00m;

            var embed = new EmbedBuilder()
                .WithTitle(job)
                .WithDescription($"Total {job}: {totalJobCountInClass} ({ ratioOfJobToTotalCharacters.ToString("F") }%)")
                .WithFooter("Obtained through https://mapleunity.com/rankings/all")
                .WithColor(Color.Blue)
                .WithTimestamp(DateTimeOffset.UtcNow);

            foreach (var jobTitle in jobTitles)
            {
                foreach (var key in jobToJobCount.Keys)
                {
                    if (jobTitle == key)
                    {
                        int totalAmountOfJob = jobToJobCount[jobTitle];
                        decimal ratioOfJobBranchToTotalCharacters = (totalAmountOfJob / totalNumberOfCharacters) * 100.00m;

                        embed.AddField(jobTitle, $"{totalAmountOfJob} ({ ratioOfJobBranchToTotalCharacters.ToString("F") }%)", true);
                        break;
                    }
                }
            }

            PrintToConsole($"{ job } embed created");
            return embed.Build();
        }

        private static int GetTotalJobCountInClass(string[] jobTitles, Dictionary<string, int> jobToJobCount)
        {
            var totalJobCount = 0;

            foreach (var jobTitle in jobTitles)
            {
                foreach (var key in jobToJobCount.Keys)
                {
                    if (jobTitle == key)
                    {
                        totalJobCount += jobToJobCount[jobTitle];
                        break;
                    }
                }
            }

            return totalJobCount;
        }

        private static int GetTotalNumberOfCharacters(Dictionary<string, int> jobToJobCount)
        {
            int totalNumberOfCharacters = 0;

            foreach (var key in jobToJobCount.Keys)
            {
                totalNumberOfCharacters += jobToJobCount[key];
            }

            return totalNumberOfCharacters;
        }
    }
}
