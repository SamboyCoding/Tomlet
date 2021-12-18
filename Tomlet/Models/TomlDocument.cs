namespace Tomlet.Models
{
    public class TomlDocument : TomlTable
    {
        // ReSharper disable once UnusedMember.Global
        public static TomlDocument CreateEmpty() => new();
        
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

        public override string SerializedValue => SerializeNonInlineTable(null, false);

        public override bool ShouldBeSerializedInline => false;

        public override string StringValue => $"Toml root document ({Entries.Count} entries)";
    }
}