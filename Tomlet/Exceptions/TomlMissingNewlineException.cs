namespace Tomlet.Exceptions
{
    public class TomlMissingNewlineException : TomlExceptionWithLine
    {
        private readonly char _found;

        public TomlMissingNewlineException(int lineNumber, char found) : base(lineNumber)
        {
            _found = found;
        }

        public override string Message => $"Finished reading a key-value pair on line {LineNumber} and was expecting a newline character, but found an unexpected '{_found}'";
    }
}