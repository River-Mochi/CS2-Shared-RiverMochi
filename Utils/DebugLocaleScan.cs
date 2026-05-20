// File: Utils/DebugLocaleScan.cs
// Version: 0.1.0
// Purpose: DEBUG-only helpers for finding CS2 localization keys while making mods.
// Based on River-Mochi shared CS2 utilities.
//
// Why:
// - CS2 often already has localized text for game concepts such as notifications,
//   milestones, tooltips, menus, notifications, policies, and services.
// - These helpers let a modder search the active game localization dictionary so
//   a mod can reuse game keys instead of hard-coding English text.
//
// How to use in a mod:
// 1. Keep calls inside #if DEBUG so Release builds stay quiet.
// 2. Call one of these from Mod.OnLoad() or another debug-only test point:
//
//    #if DEBUG
//    DebugLocaleScan.DumpStarterExamples(s_Log);
//    DebugLocaleScan.DumpByPrefix("Notifications.TITLE[", s_Log, printLimit: 15);
//    DebugLocaleScan.DumpWhereValueContains("not enough electricity", s_Log);
//    #endif
//
// What to change:
// - To find notification title keys, change the prefix to "Notifications.TITLE[".
// - To find milestone keys, change the prefix to "Progression.MILESTONE_NAME:".
// - To search by visible text, change the text in DumpWhereValueContains(...).
//
// Safety:
// - Read-only. This only reads GameManager.instance.localizationManager.activeDictionary.
// - Wrapped in #if DEBUG so it does not compile into normal Release builds.

#if DEBUG
namespace CS2Shared.RiverMochi
{
    using Colossal.Localization;
    using Colossal.Logging;
    using Game.SceneFlow;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public static class DebugLocaleScan
    {
        private const int DefaultPrintLimit = 20;

        /// <summary>
        /// Small known-good example for first-time testing. This should produce only a few log lines.
        /// After this works, replace the key/prefix/search text with whatever your mod needs.
        /// </summary>
        public static void DumpStarterExamples(ILog log)
        {
            if (!TryGetDictionary(log, out LocalizationDictionary? dict))
            {
                return;
            }

            log.Info($"[LocaleScan] Active locale: {dict.localeID}; entries: {CountEntries(dict)}");
            DumpKey("Progression.MILESTONE_NAME:1", log);
            DumpByPrefix("Progression.MILESTONE_NAME:", log, printLimit: 5);
        }

        /// <summary>
        /// Print one exact key if it exists in the active language.
        /// Example: DumpKey("Progression.MILESTONE_NAME:1", s_Log);
        /// </summary>
        public static void DumpKey(string key, ILog log)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (!TryGetDictionary(log, out LocalizationDictionary? dict))
            {
                return;
            }

            if (dict.TryGetValue(key, out string value) && !string.IsNullOrWhiteSpace(value))
            {
                log.Info($"[LocaleScan] KEY: {key} = {value}");
                return;
            }

            log.Info($"[LocaleScan] KEY MISS: {key} (locale={dict.localeID})");
        }

        /// <summary>
        /// Print keys that start with a prefix, then print the total count.
        /// Examples:
        /// - DumpByPrefix("Notifications.TITLE[", s_Log)
        /// - DumpByPrefix("Progression.MILESTONE_NAME:", s_Log)
        /// - DumpByPrefix("Menu.", s_Log, printLimit: 10)
        /// </summary>
        public static void DumpByPrefix(string prefix, ILog log, int printLimit = DefaultPrintLimit, bool skipAssets = true)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return;
            }

            DumpMatches(
                log,
                $"Prefix '{prefix}'",
                printLimit,
                skipAssets,
                kv => kv.Key.StartsWith(prefix, StringComparison.Ordinal));
        }

        /// <summary>
        /// Search by key text, not translated value.
        /// Example: DumpWhereKeyContains("MILESTONE", s_Log);
        /// </summary>
        public static void DumpWhereKeyContains(string needle, ILog log, int printLimit = DefaultPrintLimit, bool skipAssets = true)
        {
            if (string.IsNullOrWhiteSpace(needle))
            {
                return;
            }

            DumpMatches(
                log,
                $"Key contains '{needle}'",
                printLimit,
                skipAssets,
                kv => kv.Key.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// Search by visible translated text.
        /// Example: DumpWhereValueContains("not enough electricity", s_Log);
        /// </summary>
        public static void DumpWhereValueContains(string needle, ILog log, int printLimit = DefaultPrintLimit, bool skipAssets = true)
        {
            if (string.IsNullOrWhiteSpace(needle))
            {
                return;
            }

            DumpMatches(
                log,
                $"Value contains '{needle}'",
                printLimit,
                skipAssets,
                kv => kv.Value?.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static void DumpMatches(
            ILog log,
            string label,
            int printLimit,
            bool skipAssets,
            Func<KeyValuePair<string, string>, bool> predicate)
        {
            if (!TryGetDictionary(log, out LocalizationDictionary? dict))
            {
                return;
            }

            int count = 0;
            int printed = 0;
            int safePrintLimit = Math.Max(0, printLimit);

            foreach (KeyValuePair<string, string> kv in dict.entries)
            {
                if (skipAssets && kv.Key.StartsWith("Assets.", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!predicate(kv))
                {
                    continue;
                }

                count++;
                if (printed < safePrintLimit)
                {
                    printed++;
                    log.Info($"[LocaleScan] MATCH: {kv.Key} = {kv.Value}");
                }
            }

            log.Info($"[LocaleScan] {label}: total {count}, printed {printed} (locale={dict.localeID})");
        }

        private static int CountEntries(LocalizationDictionary dict)
        {
            int count = 0;
            foreach (KeyValuePair<string, string> _ in dict.entries)
            {
                count++;
            }

            return count;
        }

        private static bool TryGetDictionary(ILog log, [NotNullWhen(true)] out LocalizationDictionary? dict)
        {
            dict = null;

            try
            {
                LocalizationManager? manager = GameManager.instance?.localizationManager;
                dict = manager?.activeDictionary;
            }
            catch (Exception ex)
            {
                log.Warn($"[LocaleScan] Could not read active localization dictionary: {ex.GetType().Name}: {ex.Message}");
                return false;
            }

            if (dict == null)
            {
                log.Info("[LocaleScan] No active localization dictionary is available yet.");
                return false;
            }

            return true;
        }
    }
}
#endif
