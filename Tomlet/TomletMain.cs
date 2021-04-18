using System;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace Tomlet
{
    public static class TomletMain
    {
        public static void RegisterMapper<T>(TomlSerializationMethods.Serialize<T>? serializer, TomlSerializationMethods.Deserialize<T>? deserializer)
            => TomlSerializationMethods.Register(serializer, deserializer);

        public static T To<T>(string tomlString) where T : new()
        {
            var parser = new TomlParser();
            var tomlDocument = parser.Parse(tomlString);

            return To<T>(tomlDocument);
        }

        public static T To<T>(TomlValue value) where T : new()
        {
            var deserializer = TomlSerializationMethods.GetDeserializer<T>() ?? TomlSerializationMethods.GetCompositeDeserializer<T>();

            return deserializer.Invoke(value);
        }

        public static object To(Type what, TomlValue value)
        {
            var deserializer = TomlSerializationMethods.GetDeserializer(what) ?? TomlSerializationMethods.GetCompositeDeserializer(what);

            return deserializer.Invoke(value);
        }

        public static TomlValue ValueFrom<T>(T t) => ValueFrom(typeof(T), t);

        public static TomlValue ValueFrom(Type type, object t)
        {
            var serializer = TomlSerializationMethods.GetSerializer(type) ?? TomlSerializationMethods.GetCompositeSerializer(type);

            var tomlValue = serializer.Invoke(t);

            return tomlValue;
        }

        public static TomlDocument DocumentFrom<T>(T t) => DocumentFrom(typeof(T), t);

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