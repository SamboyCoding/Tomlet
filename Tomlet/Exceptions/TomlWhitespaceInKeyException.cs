namespace Tomlet.Exceptions;

public class TomlWhitespaceInKeyException : TomlExceptionWithLine
{
    public TomlWhitespaceInKeyException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => "Found whitespace in an unquoted TOML key at line " + LineNumber;
}