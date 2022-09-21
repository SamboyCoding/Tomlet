using System;
using System.Collections.Generic;

namespace Tomlet.Models
{
    public class TomlString : TomlValue
    {
        public static TomlString Empty => new("");

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
                
                //If we have backslashes in the string (as in, actual backslashes, not escape sequences) we could consider serializing as a literal string.
                //(but not if the string contains a single quote).
                //Additionally if it has newlines, we could serialize as a multiline literal

                if (!Value.RuntimeCorrectContains('\''))
                {
                    //Ok, we could potentially serialize as a literal string
                    if (Value.RuntimeCorrectContains('\\'))
                        return Value.RuntimeCorrectContains('\n') ? MultiLineLiteralStringSerializedForm : LiteralStringSerializedForm;
                }
                
                //Ok, no special casing, just fall back to sensible defaults:
                //  If we have single quotes but no double, use a normal string and escape.
                //  If we have double quotes but no single, and no newlines, use a literal string and no escape.
                //  If we have double quotes, no single, but newlines, use a multi-line literal.
                //  Otherwise, use a normal string with escapes.

                if (Value.RuntimeCorrectContains('\'') && !Value.RuntimeCorrectContains('"'))
                    //Standard string
                    return StandardStringSerializedForm;
                if (Value.RuntimeCorrectContains('"') && !Value.RuntimeCorrectContains('\'') && !Value.RuntimeCorrectContains('\n'))
                    //Literal
                    return LiteralStringSerializedForm;
                if (Value.RuntimeCorrectContains('"') && !Value.RuntimeCorrectContains('\''))
                    //Multi line literal
                    return MultiLineLiteralStringSerializedForm;
                
                return StandardStringSerializedForm;
            }
        }
        
        internal string StandardStringSerializedForm => $"\"{TomlUtils.EscapeStringValue(Value)}\"";
        internal string LiteralStringSerializedForm => $"'{Value}'";
        internal string MultiLineLiteralStringSerializedForm => $"'''\n{Value}'''";
    }
}