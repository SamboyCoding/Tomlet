using System;

namespace Tomlet.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class TomlPropertyAttribute : Attribute
{
    private readonly string _mapFrom;

    public TomlPropertyAttribute(string mapFrom)
    {
        _mapFrom = mapFrom;
    }

    public string GetMappedString()
    {
        return _mapFrom;
    }
}