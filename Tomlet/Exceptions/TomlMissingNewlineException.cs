namespace Tomlet.Exceptions;

public class TomlMissingNewlineException : TomlExceptionWithLine
{
    private readonly char _found;

    public TomlMissingNewlineException(int lineNumber, char found) : base(lineNumber)
    {
        _found = found;
    }

    public override string Message => $"Expecting a newline character at the end of a statement on line {LineNumber}, but found an unexpected '{_found}'";
}