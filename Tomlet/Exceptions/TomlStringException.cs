namespace Tomlet.Exceptions
{
    public class TomlStringException :TomlExceptionWithLine 
    {
        public TomlStringException(int lineNumber) : base(lineNumber)
        {
        }

        public override string Message => $"Found an invalid TOML string on line {LineNumber}";
    }
}