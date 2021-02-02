namespace Tomlet.Exceptions
{
    public class TripleQuoteInTomlMultilineLiteralException : TomlExceptionWithLine
    {
        public TripleQuoteInTomlMultilineLiteralException(int lineNumber) : base(lineNumber)
        {
        }

        public override string Message => $"Found a triple-single-quote (''') inside a multiline string literal on line {LineNumber}. This is not allowed.";
    }
}