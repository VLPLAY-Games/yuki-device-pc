using System;
using System.IO;
using System.Windows.Forms;

namespace Yuki_PC
{
    public static class Logger
    {
        private static string logFolder;
        private static string logFile;

        public static Action<string> OnLog;

        static Logger()
        {
            logFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Yuki",
                "logs"
            );

            Directory.CreateDirectory(logFolder);

            logFile = Path.Combine(logFolder, "latest.log");
        }

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Warning(string message)
        {
            Write("WARN", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        public static string GetLogFolder()
        {
            return logFolder;
        }

        private static void Write(string level, string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";

            try
            {
                File.AppendAllText(logFile, line + Environment.NewLine);
            }
            catch
            {
            }

            OnLog?.Invoke(line);
        }
    }
}