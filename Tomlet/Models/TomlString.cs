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

        public override string SerializedValue
        {
            get
            {
                //We cannot just blind serialize this, because we have to decide now if we want to serialize as a literal string or not.
                //If we have single quotes but no double, use a normal string and escape.
                //If we have double quotes but no single, and no newlines, use a literal string and no escape.
                //If we have double quotes, no single, but newlines, use a multi-line literal.
                //Otherwise, use a normal string with escapes.

                if (Value.Contains("'") && !Value.Contains("\""))
                    //Standard string
                    return $"\"{TomlUtils.EscapeStringValue(Value)}\"";
                if (Value.Contains("\"") && !Value.Contains("'") && !Value.Contains("\n"))
                    //Literal
                    return $"'{Value}'";
                if (Value.Contains("\"") && !Value.Contains("'"))
                    //Multi line literal
                    return $"'''\n{Value}'''";
                
                return $"\"{TomlUtils.EscapeStringValue(Value)}\"";
            }
        }
    }
}