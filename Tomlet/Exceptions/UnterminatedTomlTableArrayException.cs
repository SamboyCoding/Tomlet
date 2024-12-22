namespace Tomlet.Exceptions;

public class UnterminatedTomlTableArrayException : TomlExceptionWithLine
{
    public UnterminatedTomlTableArrayException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found an unterminated table-array (expecting two ]s to close it) on line {LineNumber}";
}