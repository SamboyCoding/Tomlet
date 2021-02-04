namespace Tomlet.Exceptions
{
    public class TomlInlineTableSeparatorException : TomlExceptionWithLine
    {
        private readonly char _found;

        public TomlInlineTableSeparatorException(int lineNumber, char found) : base(lineNumber)
        {
            _found = found;
        }

        public override string Message => $"Expected '}}' or ',' after key-value pair in TOML inline table, found '{_found}'";
    }
}