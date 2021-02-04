namespace Tomlet.Exceptions
{
    public class TomlContainsDottedKeyNonTableException : TomlException
    {
        internal readonly string _key;

        public TomlContainsDottedKeyNonTableException(string key)
        {
            _key = key;
        }

        public override string Message => $"A call was made on a TOML table which attempted to access a sub-key of {_key}, but the value it refers to is not a table";
    }
}