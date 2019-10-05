using System;

namespace MuLibrary.Services.Rankings
{
    public class Mapler : IComparable
    {
        public string Job   { get; set; }
        public int Level    { get; set; }

        public int CompareTo(object obj)
        {
            Mapler other = (Mapler) obj;

            if (this.Job.CompareTo(other.Job) == 0)
            {
                if (this.Level > other.Level) return 1;
                else if (this.Level < other.Level) return -1;
                else return 0;
            }

            return this.Job.CompareTo(other.Job);
        }
    }
}
