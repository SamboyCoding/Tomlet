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

        internal static TomlDouble? Parse(string valueInToml)
        {
            var nullableDouble = TomlNumberUtils.GetDoubleValue(valueInToml);

            if (!nullableDouble.HasValue)
                return null;

            return new TomlDouble(nullableDouble.Value);
        }

        public bool HasDecimal => Value != (int) Value;
        public double Value => _value;
        public override string StringValue => HasDecimal ? Value.ToString(CultureInfo.InvariantCulture) : $"{Value:F1}";
        
        public override string SerializedValue => StringValue;
    }
}
