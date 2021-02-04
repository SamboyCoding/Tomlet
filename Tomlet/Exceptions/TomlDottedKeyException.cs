namespace Tomlet.Exceptions
{
    public class TomlDottedKeyException : TomlException
    {
        private readonly string _key;

        public TomlDottedKeyException(string key)
        {
            _key = key;
        }

        public override string Message => $"Tried to redefine key {_key} as a table (by way of a dotted key) when it's already defined as not being a table.";
    }
}