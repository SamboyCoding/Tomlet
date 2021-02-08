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
    }
}