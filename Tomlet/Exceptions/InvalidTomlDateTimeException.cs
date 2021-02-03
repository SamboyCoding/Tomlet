namespace Tomlet.Exceptions
{
    public class InvalidTomlDateTimeException : TomlExceptionWithLine
    {
        private readonly string _inputString;

        public InvalidTomlDateTimeException(int lineNumber, string inputString) : base(lineNumber)
        {
            _inputString = inputString;
        }

        public override string Message => $"Found an invalid TOML date/time string '{_inputString}' on line {LineNumber}";
    }
}