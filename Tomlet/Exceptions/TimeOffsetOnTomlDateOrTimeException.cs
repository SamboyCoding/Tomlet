namespace Tomlet.Exceptions;

public class TimeOffsetOnTomlDateOrTimeException : TomlExceptionWithLine
{
    private readonly string _tzString;

    public TimeOffsetOnTomlDateOrTimeException(int lineNumber, string tzString) : base(lineNumber)
    {
        _tzString = tzString;
    }

    public override string Message => $"Found a time offset string {_tzString} in a partial datetime on line {LineNumber}. This is not allowed - either specify both the date and the time, or remove the offset specifier.";
}