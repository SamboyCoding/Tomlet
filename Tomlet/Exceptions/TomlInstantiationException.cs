using System;

namespace Tomlet.Exceptions
{
    public class TomlInstantiationException : TomlException
    {
        private readonly Type _type;

        public TomlInstantiationException(Type type)
        {
            _type = type;
        }

        public override string Message => $"Could not find a no-argument constructor for type {_type.FullName}";
    }
}