using System;

namespace MuLibrary.Rankings
{
    public class Mapler : IComparable
    {
        public string Name  { get; set; }
        public string Job   { get; set; }
        public int Level    { get; set; }

        public int CompareTo(object obj)
        {
            Mapler other = (Mapler) obj;

            if (this.Job.CompareTo(other.Job) == 0)
            {
                if (this.Level == other.Level)
                    return this.Name.CompareTo(other.Name);

                if (this.Level > other.Level) 
                    return 1;

                return -1;
            }

            return this.Job.CompareTo(other.Job);
        }
    }
}
