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

        public override string StringValue => $"Toml root document ({Entries.Count} entries)";
    }
}