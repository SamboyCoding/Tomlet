using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tomlet.Exceptions;

namespace Tomlet.Models
{
    public class TomlArray : TomlValue, IEnumerable<TomlValue>
    {
        public readonly List<TomlValue> ArrayValues = new();
        internal bool IsTableArray;
        public override string StringValue => $"Toml Array ({ArrayValues.Count} values)";

        public TomlArray()
        {
            
        }
        
        internal TomlArray(List<TomlValue> values)
        {
            ArrayValues = values;

            if (values.All(t => t is TomlTable))
                IsTableArray = true;
        }

        public bool CanBeSerializedInline => !IsTableArray || //Simple array
                                             ArrayValues.All(o => o is TomlTable {ShouldBeSerializedInline: true}) && ArrayValues.Count <= 5; //Table array of inline tables, 5 or fewer of them.

        public TomlValue this[int index] => ArrayValues[index];

        public int Count => ArrayValues.Count;

        public override string SerializedValue
        {
            get
            {
                if (!CanBeSerializedInline)
                    throw new Exception("Cannot serialize table-arrays using this method. Use TomlArray.SerializeTableArray(key)");
                
                var builder = new StringBuilder("[ ");

                builder.Append(string.Join(", ", this.Select(o => o.SerializedValue).ToArray()));

                builder.Append(" ]");

                return builder.ToString();
            }
        }

        public string SerializeTableArray(string key)
        {
            if (!IsTableArray)
                throw new Exception("Cannot serialize normal arrays using this method. Use the normal TomlValue.SerializedValue property.");

            var builder = new StringBuilder();

            foreach (var value in this)
            {
                if (value is not TomlTable table)
                    throw new Exception($"Toml Table-Array contains non-table entry? Value is {value}");
                
                builder.Append("[[").Append(key).Append("]]").Append('\n');

                builder.Append(table.SerializeNonInlineTable(null, false)).Append('\n');
            }

            return builder.ToString();
        }

        public IEnumerator<TomlValue> GetEnumerator()
        {
            return ArrayValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ArrayValues.GetEnumerator();
        }
    }
}