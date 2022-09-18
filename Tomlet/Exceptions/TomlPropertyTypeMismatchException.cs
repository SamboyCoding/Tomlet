using System;
using System.Reflection;

namespace Tomlet.Exceptions;

public class TomlPropertyTypeMismatchException : TomlTypeMismatchException
{
    private readonly Type _typeBeingInstantiated;
    private readonly PropertyInfo _propBeingDeserialized;

    public TomlPropertyTypeMismatchException(Type typeBeingInstantiated, PropertyInfo propBeingDeserialized, TomlTypeMismatchException cause) : base(cause.ExpectedType, cause.ActualType, propBeingDeserialized.PropertyType)
    {
        _typeBeingInstantiated = typeBeingInstantiated;
        _propBeingDeserialized = propBeingDeserialized;
    }

    public override string Message => $"While deserializing an object of type {_typeBeingInstantiated}, found property {_propBeingDeserialized.Name} expecting a type of {ExpectedTypeName}, but value in TOML was of type {ActualTypeName}";
}