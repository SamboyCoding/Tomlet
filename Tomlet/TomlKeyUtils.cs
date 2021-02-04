using System;
using System.Collections.Generic;

namespace Tomlet
{
    internal static class TomlKeyUtils
    {
        internal static bool IsSimpleKey(string key)
        {
            return GetTopLevelAndSubKeys(key).restOfKey == "";
        }

        internal static IEnumerable<string> GetKeyComponents(string key)
        {
            while (!string.IsNullOrEmpty(key))
            {
                var (ourKeyName, restOfKey) = GetTopLevelAndSubKeys(key);
                yield return ourKeyName;

                key = restOfKey;
            }
        }
        
        internal static (string ourKeyName, string restOfKey) GetTopLevelAndSubKeys(string key)
        {
            var wholeKeyIsQuoted = key.StartsWith("\"") && key.EndsWith("\"") || key.StartsWith("'") && key.EndsWith("'");
            var firstPartOfKeyIsQuoted = !wholeKeyIsQuoted && (key.StartsWith("\"") || key.StartsWith("'"));

            if (!key.Contains(".") || wholeKeyIsQuoted)
                return (key, "");

            //Unquoted dotted key means we put this in a sub-table.

            //First get the name of the key in *this* table.
            string ourKeyName;
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
            var restOfKey = key.Substring(ourKeyName.Length + 1);

            ourKeyName = ourKeyName.Trim();

            return (ourKeyName, restOfKey);
        }
    }
}