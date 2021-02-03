using System;
using System.Globalization;
using System.Linq;

namespace Tomlet
{
    public static class TomlNumberUtils
    {
        public static long? GetLongValue(string input)
        {
            var isOctal = input.StartsWith("0o");
            var isHex = input.StartsWith("0x");
            var isBinary = input.StartsWith("0b");

            if (isBinary || isHex || isOctal)
                input = input.Substring(2);

            //Invalid characters, double underscores
            if (input.Contains("__") || input.Any(c => c != '_' && !char.IsDigit(c) && (c < 'a' || c > 'f')))
                return null;

            //Underscore without a digit before
            if (input.First() == '_')
                return null;

            //Underscore without a digit after
            if (input.Last() == '_')
                return null;

            input = input.Replace("_", "");

            try
            {
                if (isBinary)
                    return Convert.ToInt64(input, 2);

                if (isOctal)
                    return Convert.ToInt64(input, 8);

                if (isHex)
                    return Convert.ToInt64(input, 16);

                return Convert.ToInt64(input, 10);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static double? GetDoubleValue(string input)
        {
            if (input.Contains("__") || input.Any(c => c != '_' && c != 'e' && c != '.' && !char.IsDigit(c)))
                return null;

            //Underscore without a digit before
            if (input.First() == '_')
                return null;

            //Underscore without a digit after
            if (input.Last() == '_')
                return null;

            input = input.Replace("_", "");

            //Theoretically we can have hex/octal/binary numbers with floating-point parts. I'm not implementing that.
            //None of the examples use it.
            if (long.TryParse(input, TomlNumberStyle.FLOATING_POINT, CultureInfo.InvariantCulture, out var val))
                return val;

            return null;
        }
    }
}