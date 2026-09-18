using System;
using System.IO;

namespace Yuki_PC
{
    public static partial class Logger
    {
        private static string logFolder;
        private static string logFile;

        public static Action<string, LogLevel> OnLog;

        public enum LogLevel
        {
            INFO,
            WARN,
            ERROR,
            DEBUG,
            SUCCESS
        }

        static Logger()
        {
            string projectDir = AppDomain.CurrentDomain.BaseDirectory;
            logFolder = Path.Combine(projectDir, "logs");
            Directory.CreateDirectory(logFolder);
            string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log";
            logFile = Path.Combine(logFolder, fileName);
            Info("=== Logger initialized ===");
            Info($"Log file: {logFile}");
            Info($"Application started at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        public static void Info(string message) => Write("INFO", message, LogLevel.INFO);
        public static void Warning(string message) => Write("WARN", message, LogLevel.WARN);
        public static void Error(string message) => Write("ERROR", message, LogLevel.ERROR);
        public static void Debug(string message) => Write("DEBUG", message, LogLevel.DEBUG);
        public static void Success(string message) => Write("SUCCESS", message, LogLevel.SUCCESS);

        public static string GetLogFolder() => logFolder;
        public static string GetLogFile() => logFile;

        private static void Write(string level, string message, LogLevel logLevel)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            try { File.AppendAllText(logFile, line + Environment.NewLine); } catch { }
            OnLog?.Invoke(line, logLevel);
        }
    }
}