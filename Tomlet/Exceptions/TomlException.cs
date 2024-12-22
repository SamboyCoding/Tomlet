using System;

namespace Tomlet.Exceptions;

public abstract class TomlException : Exception
{
    protected TomlException()
    {
    }

    protected TomlException(Exception cause) : base("", cause)
    {
    }
}