// File: Utils/LogUtils.cs
// Version: 0.6.0
// Purpose: popup-safe direct-file logging helpers for CS2 mods.
// Based on River-Mochi shared CS2 utilities.

namespace CS2Shared.RiverMochi
{
    using Colossal.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection.Emit;

    public static class LogUtils
    {
        private static readonly object s_WarnOnceLock = new object();
        private static readonly object s_FileWriteLock = new object();

        private static readonly HashSet<string> s_WarnOnceKeys =
            new HashSet<string>(StringComparer.Ordinal);

        private const int MaxWarnOnceKeys = 2048;

        private static string s_FallbackLogName = string.Empty;

        public static void Configure(string fallbackLogName)
        {
            if (string.IsNullOrWhiteSpace(fallbackLogName))
            {
                return;
            }

            string cleaned = Path.GetFileNameWithoutExtension(fallbackLogName.Trim());
            if (!string.IsNullOrWhiteSpace(cleaned))
            {
                s_FallbackLogName = cleaned;
            }
        }

        public static bool WarnOnce(ILog log, string key, Func<string> messageFactory, Exception? exception = null)
        {
            if (log == null || string.IsNullOrEmpty(key) || messageFactory == null)
            {
                return false;
            }

            if (!IsLevelEnabled(log, Level.Warn))
            {
                return false;
            }

            string logName = GetLogName(log);
            string fullKey = string.IsNullOrEmpty(logName) ? key : logName + "|" + key;

            lock (s_WarnOnceLock)
            {
                if (s_WarnOnceKeys.Count >= MaxWarnOnceKeys)
                {
                    s_WarnOnceKeys.Clear();
                }

                if (!s_WarnOnceKeys.Add(fullKey))
                {
                    return false;
                }
            }

            TryLog(log, Level.Warn, messageFactory, exception);
            return true;
        }

        public static void Info(ILog log, Func<string> messageFactory)
        {
            TryLog(log, Level.Info, messageFactory);
        }

        public static void Warn(ILog log, Func<string> messageFactory, Exception? exception = null)
        {
            TryLog(log, Level.Warn, messageFactory, exception);
        }

        public static void Debug(ILog log, Func<string> messageFactory)
        {
            TryLog(log, Level.Debug, messageFactory);
        }

        public static void Error(ILog log, Func<string> messageFactory, Exception? exception = null)
        {
            TryLog(log, Level.Error, messageFactory, exception);
        }

        public static void TryLog(ILog log, Level level, Func<string> messageFactory, Exception? exception = null)
        {
            if (log == null || messageFactory == null)
            {
                return;
            }

            if (!IsLevelEnabled(log, level))
            {
                return;
            }

            string message;
            try
            {
                message = messageFactory() ?? string.Empty;
            }
            catch (Exception ex)
            {
                SafeLogNoException(log, Level.Warn, "Log message factory threw: " + ex.GetType().Name + ": " + ex.Message);
                return;
            }

            try
            {
                AppendDirect(log, level, message, exception);
            }
            catch
            {
            }
        }

        private static void SafeLogNoException(ILog log, Level level, string message)
        {
            try
            {
                if (log != null && IsLevelEnabled(log, level))
                {
                    AppendDirect(log, level, message, null);
                }
            }
            catch
            {
            }
        }

        private static void AppendDirect(ILog log, Level level, string message, Exception? exception)
        {
            string logPath = GetLogPath(log);
            if (string.IsNullOrEmpty(logPath))
            {
                return;
            }

            lock (s_FileWriteLock)
            {
                string? dir = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using FileStream stream = new FileStream(
                    logPath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite);

                using StreamWriter writer = new StreamWriter(stream);

                writer.Write('[');
                writer.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"));
                writer.Write("] [");
                writer.Write(GetLevelName(level));
                writer.Write("]  ");
                writer.WriteLine(message ?? string.Empty);

                if (exception != null)
                {
                    writer.WriteLine(exception);
                }
            }
        }

        private static string GetLogPath(ILog log)
        {
            try
            {
                if (!string.IsNullOrEmpty(log.logPath))
                {
                    return log.logPath;
                }

                string logName = GetLogName(log);
                if (!string.IsNullOrEmpty(logName))
                {
                    return Path.Combine(LogManager.kDefaultLogPath, logName + ".log");
                }

                return string.Empty;
            }
            catch
            {
                if (string.IsNullOrEmpty(s_FallbackLogName))
                {
                    return string.Empty;
                }

                return Path.Combine(LogManager.kDefaultLogPath, s_FallbackLogName + ".log");
            }
        }

        private static string GetLogName(ILog log)
        {
            try
            {
                if (!string.IsNullOrEmpty(log.name))
                {
                    return log.name;
                }

                return s_FallbackLogName;
            }
            catch
            {
                return s_FallbackLogName;
            }
        }

        private static bool IsLevelEnabled(ILog log, Level level)
        {
            try
            {
                return log.isLevelEnabled(level);
            }
            catch
            {
                return true;
            }
        }

        private static string GetLevelName(Level level)
        {
            if (level == Level.Warn)
            {
                return "WARN";
            }

            if (level == Level.Error)
            {
                return "ERROR";
            }

            if (level == Level.Debug)
            {
                return "DEBUG";
            }

            if (level == Level.Trace)
            {
                return "TRACE";
            }

            if (level == Level.Verbose)
            {
                return "VERBOSE";
            }

            return "INFO";
        }
    }
}
