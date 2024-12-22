using System;

namespace Tomlet.Exceptions;

public class TomlInternalException : TomlExceptionWithLine
{
    public TomlInternalException(int lineNumber, Exception cause) : base(lineNumber, cause)
    {
    }

    public override string Message => $"An internal exception occured while parsing line {LineNumber} of the TOML document";
}