using System;

namespace Tomlet.Exceptions;

public class TomlInstantiationException : TomlException
{
    private readonly Type _type;

    public TomlInstantiationException(Type type)
    {
        _type = type;
    }
    
    public override string Message => $"Deserialization of types without a parameterless constructor or a singular parameterized constructor is not supported. Could not find a suitable constructor for type {_type.FullName}";
}