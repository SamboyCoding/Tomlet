using Tomlet.Exceptions;
using Tomlet.Models;

namespace Tomlet
{
    public static class Tomlet
    {
        public static void RegisterMapper<T>(TomlSerializationMethods.Serialize<T>? serializer, TomlSerializationMethods.Deserialize<T>? deserializer)
            => TomlSerializationMethods.Register(serializer, deserializer);

        public static T To<T>(string tomlString)
        {
            var parser = new TomlParser();
            var tomlDocument = parser.Parse(tomlString);

            var deserializer = TomlSerializationMethods.GetDeserializer<T>() ?? TomlSerializationMethods.GetCompositeDeserializer<T>();

            return deserializer.Invoke(tomlDocument);
        }

        public static TomlValue ValueFrom<T>(T t)
        {
            var serializer = TomlSerializationMethods.GetSerializer<T>() ?? TomlSerializationMethods.GetCompositeSerializer<T>();

            var tomlValue = serializer.Invoke(t);

            return tomlValue;
        }

        public static TomlDocument DocumentFrom<T>(T t)
        {
            var val = ValueFrom(t);

            if (val is TomlDocument doc)
                return doc;

            if (val is TomlTable table)
                return new TomlDocument(table);

            throw new TomlPrimitiveToDocumentException(typeof(T));
        }

        public static string TomlStringFrom<T>(T t) => DocumentFrom(t).SerializedValue;
    }
}