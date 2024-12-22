using System;

namespace Tomlet.Exceptions;

public class TomlEnumParseException : TomlException
{
    private string _valueName;
    private Type _enumType;

    public TomlEnumParseException(string valueName, Type enumType)
    {
        _valueName = valueName;
        _enumType = enumType;
    }

    public override string Message => $"Could not find enum value by name \"{_valueName}\" in enum class {_enumType} while deserializing.";
}