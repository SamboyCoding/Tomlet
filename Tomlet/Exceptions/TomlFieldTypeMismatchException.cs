using System;
using System.Reflection;

namespace Tomlet.Exceptions
{
    public class TomlFieldTypeMismatchException : TomlTypeMismatchException
    {
        private readonly Type _typeBeingInstantiated;
        private readonly FieldInfo _fieldBeingDeserialized;

        public TomlFieldTypeMismatchException(Type typeBeingInstantiated, FieldInfo fieldBeingDeserialized, Type foundType) : base(fieldBeingDeserialized.FieldType, foundType)
        {
            _typeBeingInstantiated = typeBeingInstantiated;
            _fieldBeingDeserialized = fieldBeingDeserialized;
        }

        public TomlFieldTypeMismatchException(Type typeBeingInstantiated, FieldInfo fieldBeingDeserialized, TomlTypeMismatchException cause) : base(fieldBeingDeserialized.FieldType, cause.actualType)
        {
            _typeBeingInstantiated = typeBeingInstantiated;
            _fieldBeingDeserialized = fieldBeingDeserialized;
        }

        public override string Message => $"While deserializing an object of type {_typeBeingInstantiated}, found field {_fieldBeingDeserialized.Name} expecting a type of {expectedTypeName}, but value in TOML was of type {actualTypeName}";
    }
}