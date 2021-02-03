using System.Collections.Generic;

namespace Tomlet.Models
{
    public class TomlArray : TomlValue
    {
        public List<TomlValue> ArrayValues = new();
        public override string StringValue => $"Toml Array ({ArrayValues.Count} values)";
    }
}