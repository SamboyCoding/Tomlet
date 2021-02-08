using System;
using Tomlet.Models;

namespace Tomlet.Exceptions
{
    public class TomlTypeMismatchException : TomlException
    {
        protected readonly string expectedTypeName;
        protected readonly string actualTypeName;
        protected internal readonly Type actualType;

        public TomlTypeMismatchException(Type expected, Type actual)
        {
            expectedTypeName = typeof(TomlValue).IsAssignableFrom(expected) ? expected.Name.Replace("Toml", "") : expected.Name;
            actualTypeName = typeof(TomlValue).IsAssignableFrom(actual) ? actual.Name.Replace("Toml", "") : actual.Name;
            actualType = actual;
        }

        public override string Message => $"A TOML value of type {expectedTypeName} was requested but a value of type {actualTypeName} was found";
    }
}