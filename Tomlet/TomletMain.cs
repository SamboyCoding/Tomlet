using System;
using System.Diagnostics.CodeAnalysis;
using Tomlet.Attributes;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace Tomlet
{
    //Api class, these are supposed to be exposed
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class TomletMain
    {
        [NoCoverage]
        public static void RegisterMapper<T>(TomlSerializationMethods.Serialize<T>? serializer, TomlSerializationMethods.Deserialize<T>? deserializer)
            => TomlSerializationMethods.Register(serializer, deserializer);

        public static T To<T>(string tomlString)
        {
            var parser = new TomlParser();
            var tomlDocument = parser.Parse(tomlString);

            return To<T>(tomlDocument);
        }

        public static T To<T>(TomlValue value)
        {
            return (T) To(typeof(T), value);
        }

        public static object To(Type what, TomlValue value)
        {
            var deserializer = TomlSerializationMethods.GetDeserializer(what);

            return deserializer.Invoke(value);
        }

        public static TomlValue ValueFrom<T>(T t)
        {
            if (t == null) 
                throw new ArgumentNullException(nameof(t));
            
            return ValueFrom(t.GetType(), t);
        }

        public static TomlValue ValueFrom(Type type, object t)
        {
            var serializer = TomlSerializationMethods.GetSerializer(type);

            var tomlValue = serializer.Invoke(t);

            return tomlValue;
        }

        public static TomlDocument DocumentFrom<T>(T t)
        {
            if (t == null) 
                throw new ArgumentNullException(nameof(t));
            
            return DocumentFrom(t.GetType(), t);
        }

        public static TomlDocument DocumentFrom(Type type, object t)
        {
            var val = ValueFrom(type, t);

            return val switch
            {
                TomlDocument doc => doc,
                TomlTable table => new TomlDocument(table),
                _ => throw new TomlPrimitiveToDocumentException(type)
            };
        }

        public static string TomlStringFrom<T>(T t) => DocumentFrom(t).SerializedValue;
        
        public static string TomlStringFrom(Type type, object t) => DocumentFrom(type, t).SerializedValue;
    }
}