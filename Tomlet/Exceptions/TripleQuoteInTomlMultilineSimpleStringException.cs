namespace Tomlet.Exceptions
{
    public class TripleQuoteInTomlMultilineSimpleStringException : TomlExceptionWithLine
    {
        public TripleQuoteInTomlMultilineSimpleStringException(int lineNumber) : base(lineNumber)
        {
        }
        
        public override string Message => $"Found a triple-double-quote (\"\"\") inside a multiline simple string on line {LineNumber}. This is not allowed.";
    }
}