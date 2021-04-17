namespace Tomlet.Models
{
    public class TomlLong : TomlValue
    {
        private long _value;

        public TomlLong(long value)
        {
            _value = value;
        }
        
        internal static TomlLong? Parse(string valueInToml)
        {
            var nullableDouble = TomlNumberUtils.GetLongValue(valueInToml);

            if (!nullableDouble.HasValue)
                return null;

            return new TomlLong(nullableDouble.Value);
        }

        public long Value => _value;
        public override string StringValue => Value.ToString();
        
        public override string SerializedValue => StringValue;
    }
}