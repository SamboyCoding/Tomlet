using System;
using System.Reflection;

namespace Tomlet.Exceptions
{
    public class TomlParameterTypeMismatchException : TomlTypeMismatchException
    {
        private readonly Type _typeBeingInstantiated;
        private readonly ParameterInfo _paramBeingDeserialized;

        public TomlParameterTypeMismatchException(Type typeBeingInstantiated, ParameterInfo paramBeingDeserialized, TomlTypeMismatchException cause) : base(cause.ExpectedType, cause.ActualType, paramBeingDeserialized.ParameterType)
        {
            _typeBeingInstantiated = typeBeingInstantiated;
            _paramBeingDeserialized = paramBeingDeserialized;
        }

        public override string Message => $"While deserializing an object of type {_typeBeingInstantiated}, found parameter {_paramBeingDeserialized.Name} expecting a type of {ExpectedTypeName}, but value in TOML was of type {ActualTypeName}";
    }
}