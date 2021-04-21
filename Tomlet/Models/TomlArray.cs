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

        public void Add<T>(T t) where T: new() {
            ArrayValues.Add(TomletMain.ValueFrom(t));
        }

        public bool CanBeSerializedInline => !IsTableArray || //Simple array
                                             ArrayValues.All(o => o is TomlTable { ShouldBeSerializedInline: true }) && ArrayValues.Count <= 5; //Table array of inline tables, 5 or fewer of them.

        public bool IsSimpleArray => !IsTableArray && !ArrayValues.Any(o => o is TomlArray || o is TomlTable); //Not table-array and not any sub-arrays or tables.

        public TomlValue this[int index] => ArrayValues[index];

        public int Count => ArrayValues.Count;

        public override string SerializedValue => SerializeInline(!IsSimpleArray); //If non-simple, put newlines after commas.

        public string SerializeInline(bool multiline)
        {
            if (!CanBeSerializedInline)
                throw new Exception("Cannot serialize table-arrays using this method. Use TomlArray.SerializeTableArray(key)");

            var builder = new StringBuilder("[");

            if(multiline)
                builder.Append("\n\t");
            else
                builder.Append(' ');

            var sep = multiline ? ",\n\t" : ", ";

            builder.Append(string.Join(sep, this.Select(o => o.SerializedValue).ToArray()));

            if(multiline)
                builder.Append('\n');
            else
                builder.Append(' ');

            builder.Append(']');

            return builder.ToString();
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