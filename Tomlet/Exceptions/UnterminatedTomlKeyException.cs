namespace Tomlet.Exceptions
{
    public class UnterminatedTomlKeyException : TomlExceptionWithLine
    {
        public UnterminatedTomlKeyException(int lineNumber) : base(lineNumber)
        {
        }

        public override string Message => $"Found an unterminated quoted key on line {LineNumber}";
    }
}