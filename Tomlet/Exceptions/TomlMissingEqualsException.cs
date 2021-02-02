namespace Tomlet.Exceptions
{
    public class TomlMissingEqualsException : TomlExceptionWithLine
    {
        private readonly char _found;
        public TomlMissingEqualsException(int lineNumber, char found) : base(lineNumber)
        {
            _found = found;
        }
        
        public override string Message => $"Expecting an equals sign ('=') on line {LineNumber}, but found '{_found}'";
    }
}