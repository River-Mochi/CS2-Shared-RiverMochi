// File: Utils/LogUtils.cs
// Version: 0.6.3 based on River-Mochi shared CS2 utilities.
// Purpose: popup-safe direct-file logging helpers for CS2 mods.
// Why: routine Info/Warn are written with .NET FileStream/StreamWriter
//   instead of sending every message through Colossal's logger write path, which
//   can surface UI popups if its internal stream fails.
// Usage:
// 1. Create your mod logger normally in Mod.cs:
//    static readonly ILog s_Log = LogManager.GetLogger("YourModId").SetShowsErrorsInUI(false);
// 2. Optional: call LogUtils.Configure("YourModId") once on load for fallback file naming.
// 3. Log with explicit logger: LogUtils.Info(s_Log, () => "message");
// The logger variable can be named anything; s_Log is just the name used in River-Mochi mods.

namespace CS2Shared.RiverMochi
{
    using Colossal.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class LogUtils
    {
        private static readonly object s_WarnOnceLock = new object();
        private static readonly object s_FileWriteLock = new object();

        // Per-process key cache so hot-path warnings show once instead of repeating every update.
        private static readonly HashSet<string> s_WarnOnceKeys =
            new HashSet<string>(StringComparer.Ordinal);

        private const int MaxWarnOnceKeys = 2048;

        // Used only if the passed ILog is null or its metadata throws during early startup/shutdown.
        private static string s_FallbackLogName = string.Empty;

        // Optional one-time setup: pass your mod id so fallback writes can still find ModName.log.
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

        // Test/mod-reload helper: lets a mod reset once-only warnings without restarting the game.
        public static void ClearWarnOnceKeys()
        {
            lock (s_WarnOnceLock)
            {
                s_WarnOnceKeys.Clear();
            }
        }

        // Logs a warning only once per logger+key so hot update loops cannot spam the log.
        public static bool WarnOnce(ILog? log, string key, Func<string> messageFactory, Exception? exception = null)
        {
            if (string.IsNullOrEmpty(key) || messageFactory == null)
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

        // Routine status/debugging info. Pass your mod's s_Log explicitly for clear call sites.
        public static void Info(ILog? log, Func<string> messageFactory)
        {
            TryLog(log, Level.Info, messageFactory);
        }

        // Recoverable problem worth showing in the mod log, optionally with an exception stack trace.
        public static void Warn(ILog? log, Func<string> messageFactory, Exception? exception = null)
        {
            TryLog(log, Level.Warn, messageFactory, exception);
        }

        // Serious problem that should still avoid Colossal logger UI popups when possible.
        public static void Error(ILog? log, Func<string> messageFactory, Exception? exception = null)
        {
            TryLog(log, Level.Error, messageFactory, exception);
        }

        // Debug output obeys the logger's enabled level before building the message string.
        public static void Debug(ILog? log, Func<string> messageFactory)
        {
            TryLog(log, Level.Debug, messageFactory);
        }

        // Very detailed diagnostics for rare deep investigations.
        public static void Trace(ILog? log, Func<string> messageFactory)
        {
            TryLog(log, Level.Trace, messageFactory);
        }

        // Player-enabled verbose logs: useful for test builds without making normal logs noisy.
        public static void Verbose(ILog? log, Func<string> messageFactory)
        {
            TryLog(log, Level.Verbose, messageFactory);
        }

        // Central safe entrypoint: checks level first, builds message safely, then direct-appends.
        public static void TryLog(ILog? log, Level level, Func<string> messageFactory, Exception? exception = null)
        {
            if (messageFactory == null)
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

        // Last-chance warning path used when the original message factory itself throws.
        private static void SafeLogNoException(ILog? log, Level level, string message)
        {
            try
            {
                if (IsLevelEnabled(log, level))
                {
                    AppendDirect(log, level, message, null);
                }
            }
            catch
            {
            }
        }

        // Writes directly to ModName.log using .NET, bypassing Colossal's logger write path.
        private static void AppendDirect(ILog? log, Level level, string message, Exception? exception)
        {
            string logPath = GetLogPath(log);
            if (string.IsNullOrEmpty(logPath))
            {
                return;
            }

            lock (s_FileWriteLock)
            {
                // Direct append keeps routine mod diagnostics out of Colossal's UI-log path.
                // ShareReadWrite keeps the file readable while the game is running.
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

        // Prefer Colossal's assigned file path; fallback to Logs/FallbackName.log if needed.
        private static string GetLogPath(ILog? log)
        {
            try
            {
                if (log != null && !string.IsNullOrEmpty(log.logPath))
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

        // Keeps the logger name lookup isolated because ILog metadata can be fragile during startup.
        private static string GetLogName(ILog? log)
        {
            try
            {
                if (log != null && !string.IsNullOrEmpty(log.name))
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

        // If level checks fail because logging is in flux, keep direct-file logging available.
        private static bool IsLevelEnabled(ILog? log, Level level)
        {
            try
            {
                return log == null || log.isLevelEnabled(level);
            }
            catch
            {
                // If Colossal logging state is in flux, prefer keeping direct-file logging alive.
                return true;
            }
        }

        // Format level names like Colossal logs so ModName.log remains easy to grep.
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
