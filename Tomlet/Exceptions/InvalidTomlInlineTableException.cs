using System;

namespace Tomlet.Exceptions
{
    public class InvalidTomlInlineTableException : TomlExceptionWithLine
    {
        public InvalidTomlInlineTableException(int lineNumber, TomlException cause) : base(lineNumber, cause)
        {
        }

        public override string Message => $"Found an invalid inline TOML table on line {LineNumber}. See further down for cause.";
    }
}