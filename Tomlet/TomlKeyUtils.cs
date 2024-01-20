using System;
using System.Text.RegularExpressions;
using Tomlet.Exceptions;

namespace Tomlet
{
    internal static class TomlKeyUtils
    {
        private static readonly Regex UnquotedKeyRegex = new Regex("^[a-zA-Z0-9-_]+$");
        internal static string FullStringToProperKey(string key)
        {
            var canBeUnquoted = UnquotedKeyRegex.Match(key).Success;
            return canBeUnquoted ? key : TomlUtils.AddCorrectQuotes(key);
        }
    }
}