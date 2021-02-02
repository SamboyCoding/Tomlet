using System;

namespace Tomlet.Exceptions
{
    public abstract class TomlExceptionWithLine : TomlException
    {
        protected int LineNumber;

        protected TomlExceptionWithLine(int lineNumber)
        {
            LineNumber = lineNumber;
        }
    }
}