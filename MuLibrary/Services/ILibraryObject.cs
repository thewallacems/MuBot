using System;

namespace MuLibrary
{
    public interface ILibraryObject : IComparable
    {
        public string Name { get; set; }
    }
}
