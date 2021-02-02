using System.Globalization;

namespace Tomlet
{
    public static class TomlNumberStyle
    {
        internal static NumberStyles FLOATING_POINT = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign;
        internal static NumberStyles INTEGER = NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign;
    }
}