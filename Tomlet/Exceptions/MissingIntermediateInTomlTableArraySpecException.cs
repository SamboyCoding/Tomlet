using System;

namespace Tomlet.Exceptions
{
    public class MissingIntermediateInTomlTableArraySpecException : TomlExceptionWithLine
    {
        private readonly string _missing;

        public MissingIntermediateInTomlTableArraySpecException(int lineNumber, string missing) : base(lineNumber)
        {
            _missing = missing;
        }

        public override string Message => $"Missing intermediate definition for {_missing} in table-array specification on line {LineNumber}. This is undefined behavior, and I chose to define it as an error.";
    }
}