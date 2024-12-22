using System;

namespace Tomlet.Models;

public class TomlOffsetDateTime : TomlValue
{
    private readonly DateTimeOffset _value;

    public TomlOffsetDateTime(DateTimeOffset value)
    {
        _value = value;
    }
        
    public DateTimeOffset Value => _value;
        
    public override string StringValue => Value.ToString("O");

    public static TomlOffsetDateTime? Parse(string input)
    {
        if (!DateTimeOffset.TryParse(input, out var dt))
            return null;

        return new TomlOffsetDateTime(dt);
    }
        
    public override string SerializedValue => StringValue;
}