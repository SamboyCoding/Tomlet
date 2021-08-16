namespace Tomlet.Exceptions
{
    public class TomlEndOfFileException : TomlExceptionWithLine
    {
        public TomlEndOfFileException(int lineNumber) : base(lineNumber)
        {
        }

        public override string Message => $"Found unexpected EOF on line {LineNumber} when parsing TOML file";
    }
}