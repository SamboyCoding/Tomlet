
using System;
using System.Globalization;

namespace Tomlet.Models
{
    public class TomlLocalDateTime : TomlValue
    {
        private readonly DateTime _value;

        public TomlLocalDateTime(DateTime value)
        {
            _value = value;
        }
        
        public DateTime Value => _value;
        
        public override string StringValue => Value.ToString(CultureInfo.InvariantCulture);

        public static TomlLocalDateTime? Parse(string input)
        {
            if (!DateTime.TryParse(input, out var dt))
                return null;

            return new TomlLocalDateTime(dt);
        }
    }
}