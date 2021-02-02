namespace Tomlet.Exceptions
{
    public class InvalidTomlNumberException : TomlExceptionWithLine
    {
        private readonly string _input;

        public InvalidTomlNumberException(int lineNumber, string input) : base(lineNumber)
        {
            _input = input;
        }

        public override string Message => $"While reading input line {LineNumber}, found an invalid number literal '{_input}'";
    }
}