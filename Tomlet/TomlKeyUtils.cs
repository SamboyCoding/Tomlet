using System;
using System.Text.RegularExpressions;
using Tomlet.Exceptions;

namespace Tomlet
{
    internal static class TomlKeyUtils
    {
        private static readonly Regex UnquotedKeyRegex = new Regex("^[a-zA-Z0-9-_]+$");

        internal static void GetTopLevelAndSubKeys(string key, out string ourKeyName, out string restOfKey)
        {
            var isBasicString = key.StartsWith("\"");
            var isLiteralString = key.StartsWith("'");

            if (isLiteralString)
            {
                // Literal strings can't be escaped
                var literalEnd = key.IndexOf('\'', 1);
                if (literalEnd + 1 == key.Length)
                {
                    // Full key, no splitting needed.
                    ourKeyName = key;
                    restOfKey = "";
                    return;
                }

                if (key[literalEnd + 1] != '.')
                {
                    // Literal strings cannot contain '
                    // TODO: Find better exception
                    throw new InvalidTomlKeyException(key);
                }

                if (literalEnd + 2 == key.Length)
                {
                    // You cannot have an empty unquoted key
                    // TODO: Find better exception
                    throw new InvalidTomlKeyException(key);
                }

                ourKeyName = key.Substring(0, literalEnd + 1);
                restOfKey = key.Substring(literalEnd + 2);
                return;
            }

            if (!isBasicString)
            {
                var firstDot = key.IndexOf(".", StringComparison.Ordinal);
                if (firstDot == -1)
                {
                    // Key is undotted. 
                    // We could make a check for illegal characters here, but there isn't much point to it.
                    ourKeyName = key;
                    restOfKey = "";
                    return;
                }

                if (firstDot + 1 == key.Length)
                {
                    // You cannot have an empty unquoted key
                    // TODO: Find better exception
                    throw new InvalidTomlKeyException(key);
                }

                ourKeyName = key.Substring(0, firstDot);
                restOfKey = key.Substring(firstDot + 1);
                return;
            }

            var firstUnquote = FindNextUnescapedQuote(key, 1);
            if (firstUnquote == -1)
            {
                // Quoted string was never closed
                // TODO: Find better exception
                throw new InvalidTomlKeyException(key);
            }

            if (firstUnquote + 1 == key.Length)
            {
                // Full key, no splitting needed.
                ourKeyName = key;
                restOfKey = "";
                return;
            }

            if (key[firstUnquote + 1] != '.')
            {
                // Quoted strings cannot contain unescaped "
                // TODO: Find better exception
                throw new InvalidTomlKeyException(key);
            }

            if (firstUnquote + 2 == key.Length)
            {
                // You cannot have an empty unquoted key
                // TODO: Find better exception
                throw new InvalidTomlKeyException(key);
            }

            ourKeyName = key.Substring(0, firstUnquote + 1);
            restOfKey = key.Substring(firstUnquote + 2);
        }


        private static int FindNextUnescapedQuote(string input, int startingIndex)
        {
            var i = startingIndex;
            var isEscaped = false;
            for (; i < input.Length; i++)
            {
                if (input[i] == '\\')
                {
                    isEscaped = !isEscaped;
                    continue;
                }

                if (input[i] != '"' || isEscaped)
                {
                    isEscaped = false;
                    continue;
                }

                return i;
            }

            return -1; // Return -1 if no unescaped quote is found
        }

        internal static string FullStringToProperKey(string key)
        {
            var canBeUnquoted = UnquotedKeyRegex.Match(key).Success;
            return canBeUnquoted ? key : TomlUtils.AddCorrectQuotes(key);
        }
    }
}