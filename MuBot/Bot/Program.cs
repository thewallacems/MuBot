namespace MuBot.Bot
{
    public class Program
    {
        public static void Main(string[] args) => new MuBot().StartAsync().GetAwaiter().GetResult();
    }
}
