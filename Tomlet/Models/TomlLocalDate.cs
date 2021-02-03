using System;
using System.Globalization;

namespace Tomlet.Models
{
    public class TomlLocalDate : TomlValue
    {
        private readonly DateTime _value;

        public TomlLocalDate(DateTime value)
        {
            _value = value;
        }
        
        public DateTime Value => _value;
        
        public override string StringValue => Value.ToString(CultureInfo.InvariantCulture);

        public static TomlLocalDate? Parse(string input)
        {
            if (!DateTime.TryParse(input, out var dt))
                return null;

            return new TomlLocalDate(dt);
        }
    }
}