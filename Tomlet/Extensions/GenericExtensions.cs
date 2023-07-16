using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Tomlet.Exceptions;

namespace Tomlet.Extensions
{
    internal static class GenericExtensions
    {
        private static readonly HashSet<int> IllegalChars = new()
        {
            '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
            '\u0008', '\u000b', '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013',
            '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b',
            '\u001c', '\u001d', '\u001e', '\u001f', '\u007f'
        };

        internal static bool IsWhitespace(this int val) => !val.IsNewline() && char.IsWhiteSpace((char)val);

        internal static bool IsEquals(this int val) => val == '=';

        internal static bool IsSingleQuote(this int val) => val == '\'';

        internal static bool IsDoubleQuote(this int val) => val == '"';

        internal static bool IsHashSign(this int val) => val == '#';

        internal static bool IsNewline(this int val) => val is '\r' or '\n';

        internal static bool IsDigit(this int val) => char.IsDigit((char)val);

        internal static bool IsComma(this int val) => val == ',';

        internal static bool IsPeriod(this int val) => val == '.';

        internal static bool IsEndOfArrayChar(this int val) => val == ']';

        internal static bool IsEndOfInlineObjectChar(this int val) => val == '}';

        private static bool IsPermittedInNumberLiteral(this char val)
            => val is '_' or '+' or '-' or 'e' or (>= '0' and <= '9');

        internal static bool IsPermittedInIntegerLiteral(this char val)
            => val is >= 'a' and <= 'f' || val.IsPermittedInNumberLiteral();

        internal static bool IsPermittedInFloatLiteral(this char val)
            => val is '.' || val.IsPermittedInNumberLiteral();

        internal static bool IsHexDigit(this char c)
        {
            var val = (int)c;

            if (val.IsDigit())
                return true;

            var upper = char.ToUpperInvariant((char)val);
            return upper is >= 'A' and <= 'F';
        }

        internal static bool TryPeek(this TomletStringReader reader, out int nextChar)
        {
            nextChar = reader.Peek();
            return nextChar != -1;
        }

        internal static int SkipWhitespace(this TomletStringReader reader) => reader.ReadWhile(c => c.IsWhitespace()).Length;

        internal static void SkipPotentialCarriageReturn(this TomletStringReader reader)
        {
            if (reader.TryPeek(out var nextChar) && nextChar == '\r')
                reader.Read();
        }

        internal static void SkipAnyComment(this TomletStringReader reader)
        {
            //Skip anything up until the \r or \n if we start with a hash.
            if (reader.TryPeek(out var maybeHash) && maybeHash.IsHashSign())
                reader.ReadWhile(commentChar => !commentChar.IsNewline());
        }

        internal static int SkipAnyNewlineOrWhitespace(this TomletStringReader reader)
        {
            return reader.ReadWhile(c => c.IsNewline() || c.IsWhitespace()).Count(c => c == '\n');
        }

        internal static int SkipAnyCommentNewlineWhitespaceEtc(this TomletStringReader reader)
        {
            var countRead = 0;
            while (reader.TryPeek(out var nextChar))
            {
                if (!nextChar.IsHashSign() && !nextChar.IsNewline() && !nextChar.IsWhitespace())
                    break;

                if (nextChar.IsHashSign())
                    reader.SkipAnyComment();
                countRead += reader.SkipAnyNewlineOrWhitespace();
            }

            return countRead;
        }

        internal static int SkipAnyNewline(this TomletStringReader reader) => reader.ReadWhile(c => c.IsNewline()).Count(c => c == '\n');

        internal static char[] ReadChars(this TomletStringReader reader, int count)
        {
            char[] result = new char[count];
            reader.ReadBlock(result, 0, count);

            return result;
        }

        internal static string ReadWhile(this TomletStringReader reader, Predicate<int> predicate)
        {
            var ret = new StringBuilder();
            //Read up until whitespace or an equals
            while (reader.TryPeek(out var nextChar) && predicate(nextChar))
            {
                ret.Append((char)reader.Read());
            }

            return ret.ToString();
        }

        internal static bool ExpectAndConsume(this TomletStringReader reader, char expectWhat)
        {
            if (!reader.TryPeek(out var nextChar))
                return false;

            if (nextChar == expectWhat)
            {
                reader.Read();
                return true;
            }

            return false;
        }

#if !NET5_0_OR_GREATER
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey one, out TValue two)
        {
            one = pair.Key;
            two = pair.Value;
        }
#endif

        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrEmpty(s) || string.IsNullOrEmpty(s.Trim());

        internal static T? GetCustomAttribute<T>(this MemberInfo info) where T : Attribute => info.GetCustomAttributes(false).Where(a => a is T).Cast<T>().FirstOrDefault();

        internal static void EnsureLegalChar(this int c, int currentLineNum)
        {
            if (IllegalChars.Contains(c))
                throw new TomlUnescapedUnicodeControlCharException(currentLineNum, c);
        }

        /// <summary>
        /// Shim to properly support Contains(char) on net3.5. We prefer using the char version because it's hardware-accelerated on modern .NET.
        /// </summary>
        [MethodImpl((MethodImplOptions)0x0100)] // AggressiveInlining in constant form because net3.5 doesn't define it
#if NET6_0 || NETSTANDARD2_0
        public static bool RuntimeCorrectContains(this string original, char c)
            => original.Contains(c);
#else
        public static bool RuntimeCorrectContains(this string original, char c)
            => original.Contains(c.ToString());
#endif
    }
}