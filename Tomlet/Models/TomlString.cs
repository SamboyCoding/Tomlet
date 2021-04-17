namespace Tomlet.Models
{
    public class TomlString : TomlValue
    {
        public static readonly TomlString EMPTY = new("");
        
        private string value;

        public TomlString(string value)
        {
            this.value = value;
        }
        
        public string Value => value;

        public override string StringValue => Value;
        public override string SerializedValue => $"\"{StringValue}\"";
    }
}