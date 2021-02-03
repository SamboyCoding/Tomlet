namespace Tomlet.Exceptions
{
    public class UnterminatedTomlArrayException : TomlExceptionWithLine
    {
        public UnterminatedTomlArrayException(int lineNumber) : base(lineNumber)
        {
        }

        public override string Message => $"Found an unterminated TOML array on line {LineNumber}";
    }
}