using System.Collections.Generic;

namespace Tomlet.Models
{
    public class TomlDocument
    {
        public Dictionary<string, TomlValue> Entries = new();

        internal void PutValue(string key, TomlValue value)
        {
            //TODO Check for dupe key, handle tables, arrays, etc.
            Entries[key] = value;
        }
    }
}