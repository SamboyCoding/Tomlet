namespace Tomlet.Models
{
    public class TomlDocument : TomlTable
    {
        internal TomlDocument()
        {
            //Non-public ctor.
        }

        public override string StringValue => $"Toml root document ({Entries.Count} entries)";
    }
}