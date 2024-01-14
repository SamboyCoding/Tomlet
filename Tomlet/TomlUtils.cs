using System.Text.RegularExpressions;
using Tomlet.Exceptions;

namespace Tomlet
{
    internal static class TomlUtils
    {
        // Characters that can't be in either literal or quoted strings. *Technically* these can be converted to \u
        // characters, but somebody else can implement this functionality.
        private static readonly Regex CanBeBasicRegex =
            new Regex(@"^[\x08-\x0A\x0C-\x0D\x20-\x7E\x80-\uD7FF\uE000-\uFFFF]+$");

        // Toml defines non-ascii as %x80-D7FF / %xE000-10FFFF, so this will break hard for UTF16
        private static readonly Regex CanBeLiteralRegex =
            new Regex(@"^[\x09\x20-\x26\x28-\x7E\x80-\uD7FF\uE000-\uFFFF]+$");

        public static string EscapeStringValue(string key)
        {
            // Escaped characters allowed in simple strings: 
            // https://github.com/toml-lang/toml/blob/8eae5e1c005bc5836098505f85a7aa06568999dd/toml.abnf#L74
            var escaped =
                key.Replace(@"\", @"\\")
                    .Replace("\n", @"\n")
                    .Replace("\t", @"\t")
                    .Replace("\"", @"\""")
                    .Replace("\b", @"\b") // Backspace
                    .Replace("\f", @"\f") // Form Feed
                    .Replace("\r", @"\r") // Carriage Return
                    // \uXXXX and \UXXXXXXXX get parsed as unicode, thus we should escape strings that the parser
                    // would mistake for such an escape value. Since unicode symbols are allowed we don't need to
                    // escape *actual* unicode characters in the text
                    .Replace(@"\u", @"\\u")
                    .Replace(@"\U", @"\\U");
            return escaped;
        }

        public static string AddCorrectQuotes(string key)
        {
            var literal = CanBeLiteralRegex.Match(key).Success;
            if (literal)
            {
                // Literal strings aren't escaped
                return $"'{key}'";
            }

            var basic = CanBeBasicRegex.Match(key).Success;
            if (!basic)
            {
                throw new InvalidTomlKeyException(key);
            }

            key = EscapeStringValue(key);
            return $"\"{key}\"";
        }
    }
}