// DebugLocaleScan.cs

#if DEBUG
using System;
using Colossal.Localization;   // LocalizationManager, LocalizationDictionary
using Colossal.Logging;        // ILog
using Game.SceneFlow;          // GameManager

namespace AchievementFixer
{
    /*
     * DebugLocaleScan (DEBUG-only)
     * ---------------------------
     * Might Spin-off into a separate Utility Mod to help other modders.
     *
     * What it does:
     *   - Analyzes the game's ACTIVE localization dictionary for the current language
     *     (GameManager.instance.localizationManager.activeDictionary).
     *   - Lets you quickly list/count achievement-related keys (TITLE / DESCRIPTION), and
     *     search for keys by their VALUE text.
     *
     * Why this is useful:
     *   - Verify the exact keys the game exposes (e.g., "Achievements.TITLE[MyFirstCity]").
     *   - Find which key maps to a particular phrase (search by VALUE, case-insensitive).
     *   - Detect patches: if counts drop to 0 after an update, the keys likely moved/changed.
     *
     * How to run:
     *   1) Call these from Mod.OnLoad() ONLY in DEBUG builds, e.g.:
     *
     *        #if DEBUG
     *        DebugLocaleScan.DumpAchievements(Mod.log);
     *        // Optional targeted searches by visible text:
     *        DebugLocaleScan.DumpWhereValueContains("My First City", Mod.log);
     *        DebugLocaleScan.DumpWhereValueContains("Key to the City", Mod.log);
     *        #endif
     *
     *   2) Can also re-run these after a language change if you subscribe to
     *      LocalizationManager.onActiveDictionaryChanged and want to log the new locale’s data.
     *
     * Output:
     *   - Writes concise lines to the mod log. For DumpAchievements, it prints a few sample keys/values
     *     (so logs don’t explode) and then a final summary:
     *       [Locale] Achievements keys — TITLE:NN  DESC:NN  OTHER:NN  (locale=en-US)
     *
     * Generic usage (not only Achievements):
     *   - DumpWhereValueContains(...) works across ALL keys (except Assets.* if you keep skipAssets=true).
     *   - Optional DumpByPrefix(...) below can list any tables you care about:
     *       "Menu.", "Options.", "Tutorial.", "Tooltips.", "Notifications.",
     *       "Infoviews.", "Progression.", "Policies.", etc.
     *
     * Build/Packaging:
     *   - This file is wrapped in #if DEBUG so it will NOT compile into Release builds as long as
     *     your Release configuration doesn’t define DEBUG (default).
     *   - If you also want to exclude the file from the Release project entirely,
     *     add this to your .csproj:
     *
     *       <ItemGroup Condition="'$(Configuration)'=='Release'">
     *         <Compile Remove="Utilities\DebugLocaleScan.cs" />
     *       </ItemGroup>
     *
     * Safety:
     *   - Read-only: just logs what keys/values are present. Does not modify game state.
     */

    internal static class DebugLocaleScan
    {
        /// <summary>
        /// Dump counts and a few sample entries for achievement-related keys.
        /// TITLE[...] are the friendly names; DESCRIPTION[...] are their descriptions.
        /// OTHER covers keys under "Achievements." that aren't title/description.
        /// </summary>
        public static void DumpAchievements(ILog log)
        {
            var lm = GameManager.instance?.localizationManager as LocalizationManager;
            LocalizationDictionary? dict = lm?.activeDictionary;
            if (dict == null)
            {
                log.Info("[Locale] No activeDictionary.");
                return;
            }

            int title = 0, desc = 0, other = 0, printed = 0;

            foreach (System.Collections.Generic.KeyValuePair<string, string> kv in dict.entries)
            {
                // Print a handful of examples so logs stay readable.
                if (kv.Key.StartsWith("Achievements.TITLE[", StringComparison.Ordinal))
                {
                    if (printed++ < 10)
                        log.Info($"[Locale] TITLE: {kv.Key} = {kv.Value}");
                    title++;
                }
                else if (kv.Key.StartsWith("Achievements.DESCRIPTION[", StringComparison.Ordinal))
                {
                    if (printed++ < 20)
                        log.Info($"[Locale] DESC : {kv.Key} = {kv.Value}");
                    desc++;
                }
                else if (kv.Key.StartsWith("Achievements.", StringComparison.Ordinal))
                {
                    if (printed++ < 25)
                        log.Info($"[Locale] OTHER: {kv.Key} = {kv.Value}");
                    other++;
                }
            }

            log.Info($"[Locale] Achievements keys — TITLE:{title}  DESC:{desc}  OTHER:{other}  (locale={dict.localeID})");
        }

        /// <summary>
        /// Search by VISIBLE TEXT (value), case-insensitive.
        /// Example: DebugLocaleScan.DumpWhereValueContains("companies", log);
        /// By default, skips "Assets.*" keys to reduce noise.
        /// </summary>
        public static void DumpWhereValueContains(string needle, ILog log, bool skipAssets = true)
        {
            if (string.IsNullOrWhiteSpace(needle))
                return;

            var lm = GameManager.instance?.localizationManager as LocalizationManager;
            LocalizationDictionary? dict = lm?.activeDictionary;
            if (dict == null)
            {
                log.Info("[Locale] No activeDictionary.");
                return;
            }

            foreach (System.Collections.Generic.KeyValuePair<string, string> kv in dict.entries)
            {
                if (skipAssets && kv.Key.StartsWith("Assets.", StringComparison.Ordinal))
                    continue;
                if (kv.Value?.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    log.Info($"[Locale] MATCH: {kv.Key}\t{kv.Value}");
            }
        }

        /// <summary>
        /// (Optional) Generic prefix dumper. Handy for exploring other tables:
        /// e.g., "Menu.", "Options.", "Tutorial.", "Tooltips.", "Notifications.", "Infoviews.", etc.
        /// </summary>
        public static void DumpByPrefix(string prefix, ILog log, int printLimit = 20)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return;

            var lm = GameManager.instance?.localizationManager as LocalizationManager;
            LocalizationDictionary? dict = lm?.activeDictionary;
            if (dict == null)
            {
                log.Info("[Locale] No activeDictionary.");
                return;
            }

            int count = 0, printed = 0;
            foreach (System.Collections.Generic.KeyValuePair<string, string> kv in dict.entries)
            {
                if (kv.Key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    if (printed++ < printLimit)
                        log.Info($"[Locale] {prefix}: {kv.Key} = {kv.Value}");
                    count++;
                }
            }

            log.Info($"[Locale] Prefix '{prefix}' — total: {count} (printed first {Math.Min(printed, printLimit)})");
        }
    }
}
#endif
