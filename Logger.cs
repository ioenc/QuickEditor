using System;
using System.IO;

namespace QuickEditor
{
    /// <summary>
    /// Simple logging utility for debugging purposes
    /// </summary>
    public static class Logger
    {
        private static Config _config;
        private static readonly object _lockObject = new object();

        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }

        /// <summary>
        /// Initialize logger with configuration
        /// </summary>
        public static void Initialize(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// Log debug message
        /// </summary>
        public static void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// Log info message
        /// </summary>
        public static void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public static void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        /// <summary>
        /// Log error message
        /// </summary>
        public static void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        private static void Log(LogLevel level, string message)
        {
            if (_config == null || !_config.EnableLogging)
                return;

            var configLogLevel = ParseLogLevel(_config.LogLevel);
            if (level < configLogLevel)
                return;

            try
            {
                lock (_lockObject)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var levelStr = level.ToString().ToUpper().PadRight(7);
                    var logEntry = $"[{timestamp}] {levelStr} {message}";

                    // Write to file
                    var logPath = Path.IsPathRooted(_config.LogFilePath) 
                        ? _config.LogFilePath 
                        : Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "", _config.LogFilePath);
                    
                    File.AppendAllText(logPath, logEntry + Environment.NewLine);

                    // Also write to debug output for development
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
            }
            catch
            {
                // Ignore logging errors to avoid infinite loops
            }
        }

        private static LogLevel ParseLogLevel(string level)
        {
            return level?.ToLower() switch
            {
                "debug" => LogLevel.Debug,
                "info" => LogLevel.Info,
                "warning" => LogLevel.Warning,
                "error" => LogLevel.Error,
                _ => LogLevel.Info
            };
        }
    }
}