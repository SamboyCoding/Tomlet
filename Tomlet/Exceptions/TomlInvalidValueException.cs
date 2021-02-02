namespace Tomlet.Exceptions
{
    public class TomlInvalidValueException : TomlExceptionWithLine
    {
        private readonly char _found;

        public TomlInvalidValueException(int lineNumber, char found) : base(lineNumber)
        {
            _found = found;
        }

        public override string Message => $"Expected the start of a number, string literal, boolean, array, or table on line {LineNumber}, found '{_found}'";
    }
}