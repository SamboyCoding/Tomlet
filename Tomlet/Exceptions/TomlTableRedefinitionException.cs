namespace Tomlet.Exceptions
{
    public class TomlTableRedefinitionException : TomlExceptionWithLine
    {
        private readonly string _key;

        public TomlTableRedefinitionException(int lineNumber, string key) : base(lineNumber)
        {
            _key = key;
        }

        public override string Message => $"TOML document attempts to re-define table '{_key}' on line {LineNumber}";
    }
}