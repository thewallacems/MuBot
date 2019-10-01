namespace MuLibraryDownloader
{
    public class Program
    {
        public static void Main(string[] args) => new MuLibraryDownloader().StartAsync().GetAwaiter().GetResult();
    }
}
