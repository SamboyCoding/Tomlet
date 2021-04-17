using System;
using System.Collections.Generic;
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
                
                var keys = new List<string>(Keys);

                keys.Sort(SortComplexTypesToEnd);

                foreach (var key in Keys)
                {
                    var value = GetValue(key);
                    switch (value)
                    {
                        case TomlTable table:
                            if (table.ShouldBeSerializedInline)
                            {
                                //key = {key1 = value1, key2 = value2, etc}
                                builder.Append('[').Append(key).Append("]\n");
                                builder.Append(table.SerializedValue).Append('\n');
                            }
                            else
                                //[table name]
                                //key = value
                                //key2 = value2
                                //etc
                                builder.Append(table.SerializeNonInlineTable(key)).Append('\n');

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