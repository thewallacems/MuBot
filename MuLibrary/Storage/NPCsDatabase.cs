using MuLibrary.Library.NPCs;
using System;

namespace MuLibrary.Storage
{
    public class NPCsDatabase : Database<NPC>
    {
        public NPCsDatabase(IServiceProvider provider) : base(provider, Constants.NPC_JSON_FILE_PATH) { }
    }
}
