namespace Tomlet.Exceptions
{
    public class TomlTripleQuotedKeyException : TomlExceptionWithLine
    {
        public TomlTripleQuotedKeyException(int lineNumber) : base(lineNumber)
        {
        }
        
        public override string Message => $"Found a triple-quoted key on line {LineNumber}. This is not allowed.";
    }
}