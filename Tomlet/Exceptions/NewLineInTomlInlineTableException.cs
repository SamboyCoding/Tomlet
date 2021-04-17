namespace Tomlet.Exceptions
{
    public class NewLineInTomlInlineTableException : TomlExceptionWithLine
    {
        public NewLineInTomlInlineTableException(int lineNumber) : base(lineNumber)
        {
        }

        public override string Message => "Found a new-line character within a TOML inline table. This is not allowed.";
    }
}