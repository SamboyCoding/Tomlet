
using System;
using System.Xml;

namespace Tomlet.Models
{
    public class TomlLocalDateTime : TomlValue, TomlValueWithDateTime
    {
        private readonly DateTime _value;

        public TomlLocalDateTime(DateTime value)
        {
            _value = value;
        }
        
        public DateTime Value => _value;
        
        public override string StringValue => XmlConvert.ToString(Value, XmlDateTimeSerializationMode.Unspecified); //XmlConvert specifies RFC 3339

        public static TomlLocalDateTime? Parse(string input)
        {
            if (!DateTime.TryParse(input, out var dt))
                return null;

            return new TomlLocalDateTime(dt);
        }
    }
}