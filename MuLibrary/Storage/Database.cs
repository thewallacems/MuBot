using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Downloading;
using MuLibrary.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MuLibrary.Storage
{
    public abstract class Database<T> : ServiceBase where T : ILibraryObject
    {
        private readonly JsonService _json;
        private readonly string _file;

        protected List<T> _database;

        public Database(IServiceProvider provider, string file) : base(provider)
        {
            _json = provider.GetRequiredService<JsonService>();
            _file = file;

            LoadDatabase();
        }

        public void ReloadDatabase()
        {
            LoadDatabase();
        }

        public T LoadObject(string name)
        {
            return _database.Where(e => e.Name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        private void LoadDatabase()
        {
            _log.Log($"Loading {typeof(T).Name} database");

            if (!File.Exists(_file))
            {
                using FileStream fs = File.Create(_file);
                fs.Close();

                _database = new List<T>();
                SaveDatabase();

                return;
            }

            _database = _json.ReadFromJson<List<T>>(_file);
        }

        private void SaveDatabase()
        {
            _json.WriteToJson(_file, _database);
        }
    }
}