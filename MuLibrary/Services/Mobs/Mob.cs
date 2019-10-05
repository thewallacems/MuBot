using System;

namespace MuLibrary.Services.Mobs
{
    public class Mob : ILibraryObject, IComparable
    {
        public string Name          { get; set; }
        public string ImageUrl      { get; set; }
        public string LibraryUrl    { get; set; }
        public string WeaponAttack  { get; set; }
        public string MagicAttack   { get; set; }
        public string WeaponDefense { get; set; }
        public string MagicDefense  { get; set; }
        public string Accuracy      { get; set; }
        public string Avoidability  { get; set; }
        public string Speed         { get; set; }
        public string Knockback     { get; set; }

        public int CompareTo(object obj)
        {
            Mob other = (Mob) obj;
            return string.Compare(this.Name, other.Name);
        }

        public override string ToString()
        {
            string message = $"Weapon Attack: {this.WeaponAttack}\n" +
                $"Magic Attack: {this.MagicAttack}\n" +
                $"Weapon Defense: {this.WeaponDefense}\n" +
                $"Magic Defense: {this.MagicDefense}\n" +
                $"Accuracy: {this.Accuracy}\n" +
                $"Avoidability: {this.Avoidability}\n" +
                $"Speed: {this.Speed}\n" +
                $"Knockback: {this.Knockback}";

            return message;
        }
    }
}
