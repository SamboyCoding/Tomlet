using System;
using System.Text.RegularExpressions;
using Tomlet.Exceptions;

namespace Tomlet
{
    internal static class TomlKeyUtils
    {
        private static readonly Regex UnquotedKeyRegex = new Regex("^[a-zA-Z0-9-_]+$");

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