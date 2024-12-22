namespace Tomlet.Exceptions;

public class TomlNonTableArrayUsedAsTableArrayException : TomlExceptionWithLine
{
    private readonly string _arrayName;

    public TomlNonTableArrayUsedAsTableArrayException(int lineNumber, string arrayName) : base(lineNumber)
    {
        _arrayName = arrayName;
    }

    public override string Message => $"{_arrayName} is used as a table-array on line {LineNumber} when it has previously been defined as a static array. This is not allowed.";
}