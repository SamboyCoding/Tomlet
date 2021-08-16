using System;
using System.Reflection;

namespace Tomlet.Exceptions
{
    public class TomlFieldTypeMismatchException : TomlTypeMismatchException
    {
        private readonly Type _typeBeingInstantiated;
        private readonly FieldInfo _fieldBeingDeserialized;

        public TomlFieldTypeMismatchException(Type typeBeingInstantiated, FieldInfo fieldBeingDeserialized, TomlTypeMismatchException cause) : base(cause.ExpectedType, cause.ActualType, fieldBeingDeserialized.FieldType)
        {
            _typeBeingInstantiated = typeBeingInstantiated;
            _fieldBeingDeserialized = fieldBeingDeserialized;
        }

        public override string Message => $"While deserializing an object of type {_typeBeingInstantiated}, found field {_fieldBeingDeserialized.Name} expecting a type of {ExpectedTypeName}, but value in TOML was of type {ActualTypeName}";
    }
}