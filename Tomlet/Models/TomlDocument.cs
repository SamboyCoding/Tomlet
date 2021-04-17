using System;
using System.Text;

namespace Tomlet.Models
{
    public class TomlDocument : TomlTable
    {
        internal TomlDocument()
        {
            //Non-public ctor.
        }

        internal TomlDocument(TomlTable from)
        {
            foreach (var key in from.Keys)
            {
                PutValue(key, from.GetValue(key));
            }
        }

        public override string SerializedValue
        {
            get
            {
                var builder = new StringBuilder();
                foreach (var (key, value) in Entries)
                {
                    switch (value)
                    {
                        case TomlTable table:
                            //[table name]
                            //key = value
                            //key2 = value2
                            //etc
                            builder.Append('[').Append(key).Append("]\n");
                            builder.Append(table.SerializedValue).Append('\n');
                            continue;
                        case TomlArray {IsTableArray: true} array:
                            //[[table-array name]]
                            //key = value
                            //
                            //[[table-array name]]
                            //key = value
                            //etc
                            builder.Append(array.SerializeTableArray(key)).Append('\n');
                            continue;
                        default:
                            //Normal `key = value` pair.
                            builder.Append(key).Append(" = ").Append(value.SerializedValue).Append("\n");
                            break;
                    }
                }

                return builder.ToString();
            }
        }

        public override bool ShouldBeSerializedInline => false;

        public override string StringValue => $"Toml root document ({Entries.Count} entries)";
    }
}