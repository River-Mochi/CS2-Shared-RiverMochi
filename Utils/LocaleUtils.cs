// File: Utils/LocaleUtils.cs
// version : 0.2.0
// Purpose: safe localization lookup and formatting helpers for Options UI strings.
// Based on River-Mochi shared CS2 utilities.

namespace CS2Shared.RiverMochi
{
    using Colossal.Localization;
    using Game.SceneFlow;
    using System;
    using System.Globalization;

    public static class LocaleUtils
    {
        public static string Localize(string entryId, string fallback)
        {
            if (string.IsNullOrEmpty(entryId))
            {
                return fallback;
            }

            try
            {
                // During early load the active dictionary may be unavailable; fallback keeps UI safe.
                LocalizationDictionary? dict = GameManager.instance.localizationManager.activeDictionary;
                if (dict != null && dict.TryGetValue(entryId, out string value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            catch
            {
            }

            return fallback;
        }

        public static string SafeFormat(string entryId, string fallbackFormat, params object[] args)
        {
            string format = Localize(entryId, fallbackFormat);

            try
            {
                // Locale strings are hand-edited, so tolerate a bad placeholder count.
                return string.Format(CultureInfo.CurrentCulture, format, args);
            }
            catch (FormatException)
            {
                try
                {
                    return string.Format(CultureInfo.CurrentCulture, fallbackFormat, args);
                }
                catch
                {
                    return fallbackFormat;
                }
            }
            catch
            {
                return fallbackFormat;
            }
        }

        public static string FormatN0(long value)
        {
            // Shared number formatting keeps UI rows and log reports visually consistent.
            return value.ToString("N0", CultureInfo.CurrentCulture);
        }
    }
}
