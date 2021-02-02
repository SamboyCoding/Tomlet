using System.Globalization;

namespace Tomlet.Models
{
    public class TomlLong : TomlValue
    {
        private long _value;

        public TomlLong(long value)
        {
            _value = value;
        }
        
        internal static TomlLong? Parse(string valueInToml) => 
            long.TryParse(valueInToml, TomlNumberStyle.INTEGER, NumberFormatInfo.InvariantInfo, out var val) ? new TomlLong(val) : null;

        public long Value => _value;
        public override string StringValue { get; }
    }
}