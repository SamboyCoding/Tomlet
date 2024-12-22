namespace Tomlet.Exceptions;

public class TomlArraySyntaxException : TomlExceptionWithLine
{
    private readonly char _charFound;

    public TomlArraySyntaxException(int lineNumber, char charFound) : base(lineNumber)
    {
        _charFound = charFound;
    }

    public override string Message => $"Expecting ',' or ']' after value in array on line {LineNumber}, found '{_charFound}'";
}