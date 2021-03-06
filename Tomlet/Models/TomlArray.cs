﻿using System.Collections;
using System.Collections.Generic;

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