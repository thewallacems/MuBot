using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MuLibrary.Services.Items
{
    public class ItemsService : ServiceBase
    {
        private const string ITEM_LIBRARY_URL = "https://lib.mapleunity.com/item/";
        private const string ITEM_SEARCH_URL = "https://lib.mapleunity.com/item?page=";
        private readonly Regex ITEM_CHECK_REGEX = new Regex(@"<title>(?<itemType>[a-zA-Z]*) - (?<itemName>[^\n]*) \| MapleUnity Library</title>");

        private readonly LibraryService _lib;

        public ItemsService(IServiceProvider provider) : base(provider)
        {
            _lib = provider.GetService<LibraryService>();
        }

        public async Task<List<Item>> GetObjects()
        {
            var itemsList = await _lib.GetObjects(ITEM_LIBRARY_URL, ITEM_SEARCH_URL, GetObjectFromId);
            return itemsList;
        }

        private async Task<Item> GetObjectFromId(string itemId)
        {
            var item = new Item();

            string url = ITEM_LIBRARY_URL + itemId;
            string page = await _lib.DownloadPageAsync(url);

            var match = _lib.GetMatchInPage(ITEM_CHECK_REGEX, page);
            if (!match.Success) { _log.Log($"Error occured downloading {itemId} at {url}"); throw new ArgumentException(); }

            item.Name =         match.Groups["itemName"].Value;
            item.ItemType =     match.Groups["itemType"].Value;
            item.ImageUrl =     $"https://lib.mapleunity.com/images/item/{itemId}.png";
            item.LibraryUrl =   $"https://lib.mapleunity.com/item/{itemId}";

            return item;
        }
    }
}
