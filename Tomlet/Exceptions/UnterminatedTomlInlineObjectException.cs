namespace Tomlet.Exceptions
{
    public class UnterminatedTomlInlineObjectException : TomlExceptionWithLine
    {
        public UnterminatedTomlInlineObjectException(int lineNumber) : base(lineNumber)
        {
        }

        public override string Message => $"Found an unterminated TOML inline object on line {LineNumber}.";
    }
}