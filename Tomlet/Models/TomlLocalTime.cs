using System;

namespace Tomlet.Models
{
    public class TomlLocalTime : TomlValue
    {
        private readonly TimeSpan _value;

        public TomlLocalTime(TimeSpan value)
        {
            _value = value;
        }
        
        public TimeSpan Value => _value;
        
        public override string StringValue => Value.ToString();

        public static TomlLocalTime? Parse(string input)
        {
            if (!TimeSpan.TryParse(input, out var dt))
                return null;

            return new TomlLocalTime(dt);
        }
        
        public override string SerializedValue => StringValue;
    }
}