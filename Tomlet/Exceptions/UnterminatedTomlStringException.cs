namespace Tomlet.Exceptions
{
    public class UnterminatedTomlStringException : TomlExceptionWithLine
    {
        public UnterminatedTomlStringException(int lineNumber) : base(lineNumber)
        {
        }

        public override string Message => $"Found an unterminated TOML string on line {LineNumber}";
    }
}