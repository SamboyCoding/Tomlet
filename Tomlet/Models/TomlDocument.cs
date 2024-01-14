using System.Text;
using Tomlet.Extensions;

namespace Tomlet.Models
{
    public class TomlDocument : TomlTable
    {
        // ReSharper disable once UnusedMember.Global
        public static TomlDocument CreateEmpty() => new();

        public string? TrailingComment { get; set; }

        internal TomlDocument()
        {
            //Non-public ctor.
        }

        internal TomlDocument(TomlTable from)
        {
            foreach (var (key, value) in from)
            {
                PutValue(TomlKeyUtils.FullStringToProperKey(key), value);
            }
        }

        private string SerializeDocument()
        {
            var sb = new StringBuilder();
            sb.Append(SerializeNonInlineTable(null, false));

            if (TrailingComment != null)
            {
                var comment = new TomlCommentData {PrecedingComment = TrailingComment};
                sb.Append('\n');
                sb.Append(comment.FormatPrecedingComment());
            }

            return sb.ToString();
        }

        public override string SerializedValue => SerializeDocument();

        public override string StringValue => $"Toml root document ({Entries.Count} entries)";
    }
}