namespace Tomlet.Exceptions;

public class TomlUnescapedUnicodeControlCharException : TomlExceptionWithLine
{
    private readonly int _theChar;

    public TomlUnescapedUnicodeControlCharException(int lineNumber, int theChar) : base(lineNumber)
    {
        _theChar = theChar;
    }

    public override string Message => $"Found an unescaped unicode control character U+{_theChar:0000} on line {LineNumber}. Control character other than tab (U+0009) are not allowed in TOML unless they are escaped.";
}