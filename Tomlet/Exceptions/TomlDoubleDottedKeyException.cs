namespace Tomlet.Exceptions;

public class TomlDoubleDottedKeyException : TomlExceptionWithLine
{
    public TomlDoubleDottedKeyException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => "Found two consecutive dots, or a leading dot, in a key on line " + LineNumber;
}