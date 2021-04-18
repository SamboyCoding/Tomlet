using System;
using Tomlet.Models;

namespace Tomlet.Exceptions
{
    public class TomlTypeMismatchException : TomlException
    {
        protected readonly string expectedTypeName;
        protected readonly string actualTypeName;
        protected internal readonly Type expectedType;
        protected internal readonly Type actualType;
        private readonly Type _context;

        public TomlTypeMismatchException(Type expected, Type actual, Type context)
        {
            expectedTypeName = typeof(TomlValue).IsAssignableFrom(expected) ? expected.Name.Replace("Toml", "") : expected.Name;
            actualTypeName = typeof(TomlValue).IsAssignableFrom(actual) ? actual.Name.Replace("Toml", "") : actual.Name;
            expectedType = expected;
            actualType = actual;
            _context = context;
        }

        public override string Message => $"While trying to convert to type {_context}, a TOML value of type {expectedTypeName} was required but a value of type {actualTypeName} was found";
    }
}