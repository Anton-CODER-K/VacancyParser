using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyParser.Logger
{
    public static class FileLogger
    {
        private static readonly string _logPath = $"Logs/log_{DateTime.Now:yyyyMMdd}.txt";

        static FileLogger()
        {
            Directory.CreateDirectory("Logs");
        }

        public static void Info(string message)
        {
            Write("INFO", message, ConsoleColor.Green);
        }

        public static void Warn(string message)
        {
            Write("WARN", message, ConsoleColor.Yellow);
        }

        public static void Error(string message)
        {
            Write("ERROR", message, ConsoleColor.Red);
        }

        private static void Write(string level, string message, ConsoleColor color)
        {
            var log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

            
            Console.ForegroundColor = color;
            Console.WriteLine(log);
            Console.ResetColor();

           
            File.AppendAllText(_logPath, log + Environment.NewLine);
        }
    }
}
