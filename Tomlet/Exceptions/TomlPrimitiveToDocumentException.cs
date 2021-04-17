using System;

namespace Tomlet.Exceptions
{
    public class TomlPrimitiveToDocumentException : TomlException
    {
        private Type primitiveType;
        
        public TomlPrimitiveToDocumentException(Type primitiveType)
        {
            this.primitiveType = primitiveType;
        }

        public override string Message => $"Tried to create a TOML document from a primitive value of type {primitiveType.Name}. Documents can only be created from objects.";
    }
}