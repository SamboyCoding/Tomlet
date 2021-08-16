using System.Globalization;

namespace Tomlet
{
    internal static class TomlNumberStyle
    {
        internal static NumberStyles FloatingPoint = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign;
        internal static NumberStyles Integer = NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign;
    }
}