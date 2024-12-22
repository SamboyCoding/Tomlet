namespace Tomlet.Exceptions;

public class UnterminatedTomlTableNameException : TomlExceptionWithLine
{
    public UnterminatedTomlTableNameException(int lineNumber) : base(lineNumber)
    {
    }

    public override string Message => $"Found an unterminated table name on line {LineNumber}";
}