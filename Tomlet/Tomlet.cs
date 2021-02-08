namespace Tomlet
{
    public static class Tomlet
    {
        public static T To<T>(string tomlString)
        {
            var parser = new TomlParser();
            var tomlDocument = parser.Parse(tomlString);

            var deserializer = TomlSerializationMethods.GetDeserializer<T>();
            if (deserializer == null)
                deserializer = TomlSerializationMethods.GetCompositeDeserializer<T>();

            //Will only work for TomlTables.
            return deserializer.Invoke(tomlDocument);
        }
    }
}