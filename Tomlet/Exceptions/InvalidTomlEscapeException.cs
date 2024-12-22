namespace Tomlet.Exceptions;

public class InvalidTomlEscapeException : TomlExceptionWithLine
{
    private readonly string _escapeSequence;

    public InvalidTomlEscapeException(int lineNumber, string escapeSequence) : base(lineNumber)
    {
        _escapeSequence = escapeSequence;
    }

    public override string Message => $"Found an invalid escape sequence '\\{_escapeSequence}' on line {LineNumber}";
}