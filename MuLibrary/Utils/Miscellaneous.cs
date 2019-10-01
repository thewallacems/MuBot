using System;

namespace MuLibrary.Utils
{
    public static class Miscellaneous
    {
        public static void PrintToConsole(string message)
        {
            Console.WriteLine($"{ DateTime.Now.ToString("HH:mm:ss") } MuBot       { message }");
        }
    }
}
