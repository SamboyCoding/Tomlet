using System;

namespace Tomlet
{
    internal static class TomlKeyUtils
    {
        internal static void GetTopLevelAndSubKeys(string key, out string ourKeyName, out string restOfKey)
        {
            var wholeKeyIsQuoted = key.StartsWith("\"") && key.EndsWith("\"") || key.StartsWith("'") && key.EndsWith("'");
            var firstPartOfKeyIsQuoted = !wholeKeyIsQuoted && (key.StartsWith("\"") || key.StartsWith("'"));

            if (!key.Contains(".") || wholeKeyIsQuoted)
            {
                ourKeyName = key;
                restOfKey = "";
                return;
            }

            //Unquoted dotted key means we put this in a sub-table.

            //First get the name of the key in *this* table.
            if (!firstPartOfKeyIsQuoted)
            {
                var split = key.Split('.');
                ourKeyName = split[0];
            }
            else
            {
                ourKeyName = key;
                var keyNameWithoutOpeningQuote = ourKeyName.Substring(1);
                if (ourKeyName.Contains("\""))
                    ourKeyName = ourKeyName.Substring(0, 2 + keyNameWithoutOpeningQuote.IndexOf("\"", StringComparison.Ordinal));
                else
                    ourKeyName = ourKeyName.Substring(0, 2 + keyNameWithoutOpeningQuote.IndexOf("'", StringComparison.Ordinal));
            }

            //And get the remainder of the key, relative to the sub-table.
            restOfKey = key.Substring(ourKeyName.Length + 1);

            ourKeyName = ourKeyName.Trim();
        }

        public static string FullStringToProperKey(string key)
        {
            GetTopLevelAndSubKeys(key, out var a, out var b);
            var keyLooksQuoted = key.StartsWith("\"") || key.StartsWith("'");
            var keyLooksDotted = key.Contains(".");

            if (keyLooksQuoted || keyLooksDotted || !string.IsNullOrEmpty(b))
            {
                return TomlUtils.AddCorrectQuotes(key);
            }
            
            return key;
        }
    }
}