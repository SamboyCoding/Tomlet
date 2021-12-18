using System;
using System.Linq;
using System.Reflection;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace Tomlet;

internal static class TomlCompositeDeserializer
{
    public static TomlSerializationMethods.Deserialize<object> For(Type type)
    {
        TomlSerializationMethods.Deserialize<object> deserializer;
        if (type.IsEnum)
        {
            var stringDeserializer = TomlSerializationMethods.GetDeserializer<string>();
            deserializer = value =>
            {
                var enumName = stringDeserializer.Invoke(value);

                try
                {
                    return Enum.Parse(type, enumName, true);
                }
                catch (Exception)
                {
                    throw new TomlEnumParseException(enumName, type);
                }
            };
        }
        else
        {
            //Get all instance fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //Ignore NonSerialized fields.
            fields = fields.Where(f => !f.IsNotSerialized).ToArray();

            if (fields.Length == 0)
                return _ => Activator.CreateInstance(type)!;

            deserializer = value =>
            {
                if (value is not TomlTable table)
                    throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType(), type);

                object instance;
                try
                {
                    instance = Activator.CreateInstance(type)!;
                }
                catch (MissingMethodException)
                {
                    throw new TomlInstantiationException(type);
                }

                foreach (var field in fields)
                {
                    try
                    {
                        var name = field.Name;
                        var m = System.Text.RegularExpressions.Regex.Match(field.Name, "<(.*)>k__BackingField");
                        if (m.Success)
                            name = m.Groups[1].Value;

                        var name1 = name;
                        name = type.GetProperties().FirstOrDefault(p => p.Name == name1)?.GetCustomAttribute<TomlPropertyAttribute>()?.GetMappedString() ?? name;
                        if (!table.ContainsKey(name))
                            continue; //TODO: Do we want to make this configurable? As in, throw exception if data is missing?

                        var entry = table.GetValue(name);
                        var fieldDeserializer = TomlSerializationMethods.GetDeserializer(field.FieldType);
                        var fieldValue = fieldDeserializer.Invoke(entry);

                        field.SetValue(instance, fieldValue);
                    }
                    catch (TomlTypeMismatchException e)
                    {
                        throw new TomlFieldTypeMismatchException(type, field, e);
                    }
                }

                return instance;
            };
        }

        //Cache composite deserializer.
        TomlSerializationMethods.Register(type, null, deserializer);

        return deserializer;
    }
}