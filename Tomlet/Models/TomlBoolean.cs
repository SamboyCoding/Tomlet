namespace Tomlet.Models;

public class TomlBoolean : TomlValue
{
    public static TomlBoolean True => new(true);
    public static TomlBoolean False => new(false);
        
    private bool _value;
    private TomlBoolean(bool value)
    {
        _value = value;
    }

    public static TomlBoolean ValueOf(bool b) => b ? True : False;

    public bool Value => _value;

    public override string StringValue => Value ? bool.TrueString.ToLowerInvariant() : bool.FalseString.ToLowerInvariant();
        
    public override string SerializedValue => StringValue;
}