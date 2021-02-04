namespace Tomlet.Exceptions
{
    public class TomlTableArrayIntermediateNonTableException : TomlExceptionWithLine
    {
        private readonly string _arrayName;

        public TomlTableArrayIntermediateNonTableException(int lineNumber, string arrayName) : base(lineNumber)
        {
            _arrayName = arrayName;
        }

        public override string Message => $"Last element of array {_arrayName}, referenced in table-array specification on line {LineNumber}, is not a table";
    }
}