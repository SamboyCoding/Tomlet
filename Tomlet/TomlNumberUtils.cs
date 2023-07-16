using System;
using System.Globalization;
using System.Linq;
using Tomlet.Extensions;

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
            if (input.Contains("__") || input.Any(c => !c.IsPermittedInIntegerLiteral()))
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
            var skippingFirst = input.Substring(1);

            if (input is "nan" or "inf" || skippingFirst is "nan" or "inf")
            {
                //Special value
                if (input == "nan" || skippingFirst == "nan")
                    return double.NaN;
                if (input == "inf")
                    return double.PositiveInfinity;
                if (skippingFirst == "inf")
                    return input.StartsWith("-") ? double.NegativeInfinity : double.PositiveInfinity;
            }
            
            if (input.Contains("__") || input.Any(c => !c.IsPermittedInFloatLiteral()))
                return null;

            //Underscore without a digit before
            if (input.First() == '_')
                return null;

            //Underscore without a digit after
            if (input.Last() == '_')
                return null;

            input = input.Replace("_", "");
            
            //We have to do one manual check here because TOML states that decimal points must have a value on the right
            //So something like 1.e20 is valid according to the runtime but not according to TOML
            if (input.Contains("e"))
            {
                var parts = input.Split('e');
                if (parts.Length != 2)
                    return null;

                if (parts[0].EndsWith("."))
                    return null;
            }

            //Theoretically we can have hex/octal/binary numbers with floating-point parts. I'm not implementing that.
            //None of the examples use it.
            if (double.TryParse(input, TomlNumberStyle.FloatingPoint, CultureInfo.InvariantCulture, out var val))
                return val;

            return null;
        }
    }
}