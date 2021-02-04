using System.Collections;
using System.Collections.Generic;

namespace Tomlet.Models
{
    public class TomlArray : TomlValue, IEnumerable<TomlValue>
    {
        public readonly List<TomlValue> ArrayValues = new();
        public override string StringValue => $"Toml Array ({ArrayValues.Count} values)";

        public TomlValue this[int index] => ArrayValues[index];

        public int Count => ArrayValues.Count;
        
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