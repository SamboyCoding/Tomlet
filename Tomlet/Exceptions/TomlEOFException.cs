namespace Tomlet.Exceptions
{
    public class TomlEOFException : TomlExceptionWithLine
    {
        public TomlEOFException(int lineNumber) : base(lineNumber)
        {
        }

        public override string Message => $"Found unexpected EOF on line {LineNumber} when parsing TOML file";
    }
}