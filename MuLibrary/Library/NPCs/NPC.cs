using System;

namespace MuLibrary.Library.NPCs
{
    public class NPC : ILibraryObject, IComparable
    {
        public string Name          { get; set; }
        public string ImageUrl      { get; set; }
        public string LibraryUrl    { get; set; }
        public string FoundAt       { get; set; }

        public int CompareTo(object obj)
        {
            NPC other = (NPC)obj;
            return string.Compare(this.Name, other.Name);
        }
    }
}
