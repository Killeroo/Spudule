using System;

namespace Spudule
{
    /// <summary>
    /// Constructs and displays log messages to standard output
    /// </summary>
    public class Logger
    {
        public string Identifier { get; set; } = "Spudule";
        private ConsoleColor startFrontColor;
        private ConsoleColor startBackColor;

        public Logger()
        {
            startFrontColor = Console.ForegroundColor;
            startBackColor = Console.BackgroundColor;
        }

        public Logger(string identifier)
        {
            Identifier = identifier;
            startFrontColor = Console.ForegroundColor;
            startBackColor = Console.BackgroundColor;
        }

        public void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green; ;
            Console.WriteLine($"[{DateTime.Now}] [{Identifier}] [Debug] " + message);
            ResetColor();
        }
        public void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now}] [{Identifier}] [Info] " + message);
            ResetColor();
        }
        public void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now}] [{Identifier}] [Warning] " + message);
            ResetColor();
        }
        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now}] [{Identifier}] [Error] " + message);
            ResetColor();
        }
        public void Critical(string message)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[{DateTime.Now}] [{Identifier}] [Critical] " + message);
            ResetColor();
        }

        private void ResetColor()
        {
            Console.ForegroundColor = startFrontColor;
            Console.BackgroundColor = startBackColor;
        }
    }
}
