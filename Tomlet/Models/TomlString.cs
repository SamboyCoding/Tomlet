using System;

namespace Tomlet.Models
{
    public class TomlString : TomlValue
    {
        public static readonly TomlString Empty = new("");
        
        private readonly string _value;

        public TomlString(string? value)
        {
            this._value = value ?? throw new ArgumentNullException(nameof(value), "TomlString's value cannot be null");
        }
        
        public string Value => _value;

        public override string StringValue => Value;
        public override string SerializedValue => TomlUtils.AddCorrectQuotes(TomlUtils.EscapeStringValue(StringValue));
    }
}