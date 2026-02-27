using System;
using System.IO;

namespace KutuphaneOtomasyonuUygulamasi.Utilities
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "logs", 
            $"app_{DateTime.Now:yyyyMMdd}.log"
        );

        static Logger()
        {
            string logDir = Path.GetDirectoryName(LogFilePath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }

        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public static void LogError(string message, Exception ex = null)
        {
            string errorMessage = message;
            if (ex != null)
            {
                errorMessage += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
            Log("ERROR", errorMessage);
        }

        public static void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        private static void Log(string level, string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Silently fail if logging fails
            }
        }
    }
}
