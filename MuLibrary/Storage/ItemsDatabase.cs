using MuLibrary.Library.Items;
using System;

namespace MuLibrary.Storage
{
    public class ItemsDatabase : Database<Item>
    {
        public ItemsDatabase(IServiceProvider provider) : base(provider, Constants.ITEM_JSON_FILE_PATH) { }
    }
}
