using System;
using System.Linq;
using System.Reflection;
using Tomlet.Models;

namespace Tomlet;

internal static class TomlCompositeSerializer
{
    public static TomlSerializationMethods.Serialize<object> For(Type type)
    {
        TomlSerializationMethods.Serialize<object> serializer;

        if (type.IsEnum)
        {
            var stringSerializer = TomlSerializationMethods.GetSerializer<string>();
            serializer = o => stringSerializer.Invoke(Enum.GetName(type, o!) ?? throw new ArgumentException($"Tomlet: Cannot serialize {o} as an enum of type {type} because the enum type does not declare a name for that value"));
        }
        else
        {

            //Get all instance fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //Ignore NonSerialized fields.
            fields = fields.Where(f => !f.IsNotSerialized).ToArray();

            if (fields.Length == 0)
                return _ => new TomlTable();

            if (type.Namespace + "." + type.Name == "System.Collections.Generic.List`1")
            {
                //List deserializer.

                //Process base type
                For(type.GetGenericArguments()[0]);

                //And now return default list serializer
                return TomlSerializationMethods.GetSerializer(type);
            }

            if (type.IsArray)
            {
                //Process base type
                For(type.GetElementType()!);
                    
                //And return default array serializer
                return TomlSerializationMethods.GetSerializer(type);
            }

            serializer = instance =>
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance), "Object being serialized is null. TOML does not support null values.");
                    
                var resultTable = new TomlTable();

                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(instance);

                    if (fieldValue == null)
                        continue; //Skip nulls - TOML doesn't support them.

                    var fieldSerializer = TomlSerializationMethods.GetSerializer(field.FieldType);
                    var tomlValue = fieldSerializer.Invoke(fieldValue);

                    var name = field.Name;
                    var m = System.Text.RegularExpressions.Regex.Match(field.Name, "<(.*)>k__BackingField");
                    if (m.Success)
                        name = m.Groups[1].Value;

                    var name1 = name;
                    name = type.GetProperties().FirstOrDefault(p => p.Name == name1)?.GetCustomAttribute<TomlPropertyAttribute>()?.GetMappedString() ?? name;
                        
                    if (resultTable.ContainsKey(name))
                        //Do not overwrite fields if they have the same name as something already in the table
                        //This fixes serializing types which re-declare a field using the `new` keyword, overwriting a field of the same name
                        //in its supertype. 
                        continue;

                    resultTable.PutValue(name, tomlValue);
                }

                return resultTable;
            };
        }

        //Cache composite deserializer.
        TomlSerializationMethods.Register(type, serializer, null);

        return serializer;
    }
}