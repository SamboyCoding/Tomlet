using System;
using Tomlet.Models;

namespace Tomlet.Exceptions
{
    public class TomlTypeMismatchException : TomlException
    {
        protected readonly string ExpectedTypeName;
        protected readonly string ActualTypeName;
        protected internal readonly Type ExpectedType;
        protected internal readonly Type ActualType;
        private readonly Type _context;

        public TomlTypeMismatchException(Type expected, Type actual, Type context)
        {
            ExpectedTypeName = typeof(TomlValue).IsAssignableFrom(expected) ? expected.Name.Replace("Toml", "") : expected.Name;
            ActualTypeName = typeof(TomlValue).IsAssignableFrom(actual) ? actual.Name.Replace("Toml", "") : actual.Name;
            ExpectedType = expected;
            ActualType = actual;
            _context = context;
        }

        public override string Message => $"While trying to convert to type {_context}, a TOML value of type {ExpectedTypeName} was required but a value of type {ActualTypeName} was found";
    }
}