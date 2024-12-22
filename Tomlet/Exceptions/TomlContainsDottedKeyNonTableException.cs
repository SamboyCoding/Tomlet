namespace Tomlet.Exceptions;

public class TomlContainsDottedKeyNonTableException : TomlException
{
    internal readonly string Key;

    public TomlContainsDottedKeyNonTableException(string key)
    {
        Key = key;
    }

    public override string Message => $"A call was made on a TOML table which attempted to access a sub-key of {Key}, but the value it refers to is not a table";
}