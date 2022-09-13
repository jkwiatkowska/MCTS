using System;

namespace MCTS
{
    public static class Utility
    {
        public static ConsoleKey ReadKey(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Console.WriteLine(text);
            }

            var key = Console.ReadKey(true).Key;
            Console.WriteLine();

            if (key == ConsoleKey.Escape)
            {
                Environment.Exit(0);
            }

            return key;
        }

        public static char ReadKeyLine(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Console.Write(text);
            }

            var key = Console.ReadKey();
            Console.WriteLine();

            if (key.Key == ConsoleKey.Escape)
            {
                Environment.Exit(0);
            }

            return key.KeyChar;
        }
    }
}
