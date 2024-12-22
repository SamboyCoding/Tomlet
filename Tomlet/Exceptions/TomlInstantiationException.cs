namespace Tomlet.Exceptions;

public class TomlInstantiationException : TomlException
{
    public override string Message =>
        "Deserialization of types without a parameterless constructor or a singular parameterized constructor is not supported.";
}