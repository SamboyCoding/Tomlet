using System;

namespace Tomlet.Models
{
    public class TomlString : TomlValue
    {
        public static readonly TomlString EMPTY = new("");
        
        private readonly string value;

        public TomlString(string? value)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value), "TomlString's value cannot be null");
        }
        
        public string Value => value;

        public override string StringValue => Value;
        public override string SerializedValue => TomlUtils.AddCorrectQuotes(TomlUtils.EscapeStringValue(StringValue));
    }
}