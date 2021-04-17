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
        internal bool IsTableArray = false;
        public override string StringValue => $"Toml Array ({ArrayValues.Count} values)";

        public TomlArray()
        {
            
        }
        
        internal TomlArray(List<TomlValue> values)
        {
            ArrayValues = values;
        }

        public TomlValue this[int index] => ArrayValues[index];

        public int Count => ArrayValues.Count;

        public override string SerializedValue
        {
            get
            {
                if (IsTableArray)
                    throw new Exception("Cannot serialize table-arrays using this method. Use TomlArray.SerializeTableArray(key)");
                
                var builder = new StringBuilder("[ ");

                builder.Append(string.Join(", ", this.Select(o => o.SerializedValue)));

                builder.Append(" ]");

                return builder.ToString();
            }
        }

        public string SerializeTableArray(string key)
        {
            if (!IsTableArray)
                throw new Exception("Cannot serialize normal arrays using this method. Use the normal TomlValue.SerializedValue property.");
            
            throw new NotImplementedException();
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