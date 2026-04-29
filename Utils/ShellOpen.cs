// File: Utils/ShellOpen.cs
// version : 0.2.0
// Purpose: Cross-platform-ish file/folder opening helpers for Options UI buttons.
// Based on River-Mochi shared CS2 utilities.

namespace CS2Shared.RiverMochi
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using UnityEngine;

    internal static class ShellOpen
    {
        internal static void OpenModLogOrLogsFolder()
        {
            string logsFolder = GetLogsFolder();
            string logPath = string.IsNullOrEmpty(logsFolder)
                ? string.Empty
                : Path.Combine(logsFolder, Mod.ModId + ".log");

            // Prefer the exact mod log; fall back to folder before the first log file exists.
            if (!string.IsNullOrEmpty(logPath) && File.Exists(logPath))
            {
                OpenPathSafe(logPath, isFolder: false, "OpenLogFile");
                return;
            }

            OpenPathSafe(logsFolder, isFolder: true, "OpenLogsFolder");
        }

        internal static string GetLogsFolder()
        {
            try
            {
                // CS2 puts Player.log beside the Logs folder; use that as the install-independent anchor.
                string consoleLogPath = Application.consoleLogPath;
                if (string.IsNullOrEmpty(consoleLogPath))
                {
                    return string.Empty;
                }

                string? rootFolder = Path.GetDirectoryName(consoleLogPath);
                if (string.IsNullOrEmpty(rootFolder))
                {
                    return string.Empty;
                }

                string logsFolder = Path.Combine(rootFolder, "Logs");
                return Directory.Exists(logsFolder) ? logsFolder : rootFolder;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void OpenPathSafe(string path, bool isFolder, string logLabel)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    LogUtils.Info(Mod.s_Log, () => $"{Mod.ModTag} {logLabel}: path is empty.");
                    return;
                }

                string fullPath = Path.GetFullPath(path);
                if (isFolder)
                {
                    if (!Directory.Exists(fullPath))
                    {
                        LogUtils.Info(Mod.s_Log, () => $"{Mod.ModTag} {logLabel}: folder not found: {fullPath}");
                        return;
                    }
                }
                else if (!File.Exists(fullPath))
                {
                    LogUtils.Info(Mod.s_Log, () => $"{Mod.ModTag} {logLabel}: file not found: {fullPath}");
                    return;
                }

                if (TryOpenWithUnityFileUrl(fullPath, isFolder))
                {
                    return;
                }

                // If Unity's opener is ignored by the platform/Proton layer, fall back to OS shell tools.
                TryOpenWithOsShell(fullPath);
            }
            catch (Exception ex)
            {
                LogUtils.Warn(Mod.s_Log, () => $"{Mod.ModTag} {logLabel}: failed opening path: {ex.GetType().Name}: {ex.Message}", ex);
            }
        }

        private static bool TryOpenWithUnityFileUrl(string fullPath, bool isFolder)
        {
            try
            {
                string path = fullPath;
                if (isFolder &&
                    !path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) &&
                    !path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    path += Path.DirectorySeparatorChar;
                }

                // Unity handles many platforms, but Proton/macOS/Linux file associations vary.
                Application.OpenURL(new Uri(path).AbsoluteUri);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void TryOpenWithOsShell(string fullPath)
        {
            try
            {
                RuntimePlatform platform = Application.platform;

                if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
                {
                    Process.Start(new ProcessStartInfo(fullPath)
                    {
                        UseShellExecute = true,
                        ErrorDialog = false,
                        Verb = "open",
                    });
                    return;
                }

                if (platform == RuntimePlatform.OSXPlayer || platform == RuntimePlatform.OSXEditor)
                {
                    Process.Start(new ProcessStartInfo("open", QuoteArg(fullPath))
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    });
                    return;
                }

                if (platform == RuntimePlatform.LinuxPlayer || platform == RuntimePlatform.LinuxEditor)
                {
                    // Common desktop-opener fallback for Linux and Proton users.
                    Process.Start(new ProcessStartInfo("xdg-open", QuoteArg(fullPath))
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    });
                    return;
                }

                Process.Start(new ProcessStartInfo(fullPath)
                {
                    UseShellExecute = true,
                    ErrorDialog = false,
                });
            }
            catch (Exception ex)
            {
                LogUtils.Warn(Mod.s_Log, () => $"{Mod.ModTag} ShellOpen OS fallback failed: {ex.GetType().Name}: {ex.Message}", ex);
            }
        }

        private static string QuoteArg(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            return value.IndexOfAny(new[] { ' ', '\t', '"' }) >= 0
                ? "\"" + value.Replace("\"", "\\\"") + "\""
                : value;
        }
    }
}
