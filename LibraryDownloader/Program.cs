using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryDownloader.Services.Mobs;

namespace LibraryDownloader
{
    public class Program
    {
        private static readonly MobsService MobsService = new MobsService();

        public static void Main(string[] args)
        {
            var mobsTask = MobsService.GetMobs();
            Task.WaitAll(mobsTask);
            var mobs = mobsTask.Result;
        }
    }
}
