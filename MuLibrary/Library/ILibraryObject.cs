using System;

namespace MuLibrary.Library
{
    public interface ILibraryObject : IComparable
    {
        public string Name { get; set; }
    }
}
