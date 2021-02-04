using System;
using Tomlet.Models;

namespace Tomlet.Exceptions
{
    public class TomlTypeMismatchException : TomlException
    {
        private string expectedType;
        private string actualType;

        public TomlTypeMismatchException(Type expected, Type actual)
        {
            expectedType = typeof(TomlValue).IsAssignableFrom(expected) ? expected.Name.Replace("Toml", "") : expected.Name;
            actualType = typeof(TomlValue).IsAssignableFrom(actual) ? actual.Name.Replace("Toml", "") : actual.Name;
        }

        public override string Message => $"A TOML value of type {expectedType} was requested but a value of type {actualType} was found";
    }
}