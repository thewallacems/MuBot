using MuLibrary.Library.Mobs;
using System;

namespace MuLibrary.Storage
{
    public class MobsDatabase : Database<Mob>
    {
        public MobsDatabase(IServiceProvider provider) : base(provider, Constants.MOB_JSON_FILE_PATH) { }
    }
}
