namespace Tomlet.Exceptions;

public class TomlNoSuchValueException : TomlException
{
    private readonly string _key;

    public TomlNoSuchValueException(string key)
    {
        _key = key;
    }

    public override string Message => $"Attempted to get the value for key {_key} but no value is associated with that key";
}