using System;
using System.Globalization;

namespace Tomlet.Models
{
    public class TomlDouble : TomlValue
    {
        private double _value;

        public TomlDouble(double value)
        {
            _value = value;
        }

        internal static TomlDouble? Parse(string valueInToml) => 
            double.TryParse(valueInToml, TomlNumberStyle.FLOATING_POINT, NumberFormatInfo.InvariantInfo, out var val) ? new TomlDouble(val) : null;

        public double Value => _value;
        public override string StringValue => Value.ToString(CultureInfo.CurrentCulture);
    }
}