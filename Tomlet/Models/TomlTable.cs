using System.Collections.Generic;

namespace Tomlet.Models
{
    public class TomlTable : TomlValue
    {
        public Dictionary<string, TomlValue> Entries = new();

        public override string StringValue => $"Table ({Entries.Count} entries)";
    }
}