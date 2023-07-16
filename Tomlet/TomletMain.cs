using System;
using System.Diagnostics.CodeAnalysis;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace Tomlet
{
    //Api class, these are supposed to be exposed
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class TomletMain
    {
        [Attributes.ExcludeFromCodeCoverage]
        public static void RegisterMapper<T>(TomlSerializationMethods.Serialize<T>? serializer, TomlSerializationMethods.Deserialize<T>? deserializer)
            => TomlSerializationMethods.Register(serializer, deserializer);

        public static T To<T>(string tomlString, TomlSerializerOptions? options = null)
        {
            var parser = new TomlParser();
            var tomlDocument = parser.Parse(tomlString);

            return To<T>(tomlDocument, options);
        }

        public static T To<T>(TomlValue value, TomlSerializerOptions? options = null)
        {
            return (T)To(typeof(T), value, options);
        }

        public static object To(Type what, TomlValue value, TomlSerializerOptions? options = null)
        {
            var deserializer = TomlSerializationMethods.GetDeserializer(what, options);

            return deserializer.Invoke(value);
        }

#if NET6_0
        [return: NotNullIfNotNull("t")]
#endif
        public static TomlValue? ValueFrom<T>(T t, TomlSerializerOptions? options = null)
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t));

            return ValueFrom(t.GetType(), t, options);
        }

#if NET6_0
        [return: NotNullIfNotNull("t")]
#endif
        public static TomlValue? ValueFrom(Type type, object t, TomlSerializerOptions? options = null)
        {
            var serializer = TomlSerializationMethods.GetSerializer(type, options);

            var tomlValue = serializer.Invoke(t);

            return tomlValue!;
        }

        public static TomlDocument DocumentFrom<T>(T t, TomlSerializerOptions? options = null)
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t));

            return DocumentFrom(t.GetType(), t, options);
        }

        public static TomlDocument DocumentFrom(Type type, object t, TomlSerializerOptions? options = null)
        {
            var val = ValueFrom(type, t, options);

            return val switch
            {
                TomlDocument doc => doc,
                TomlTable table => new TomlDocument(table),
                _ => throw new TomlPrimitiveToDocumentException(type)
            };
        }

        public static string TomlStringFrom<T>(T t, TomlSerializerOptions? options = null) => DocumentFrom(t, options).SerializedValue;

        public static string TomlStringFrom(Type type, object t, TomlSerializerOptions? options = null) => DocumentFrom(type, t, options).SerializedValue;
    }
}