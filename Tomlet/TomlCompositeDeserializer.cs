using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Tomlet.Attributes;
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
            var stringDeserializer = TomlSerializationMethods.GetDeserializer(typeof(string));
            deserializer = value =>
            {
                var enumName = (string)stringDeserializer.Invoke(value);

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

            //Ignore NonSerialized and CompilerGenerated fields.
            fields = fields.Where(f => !f.IsNotSerialized && f.GetCustomAttribute<CompilerGeneratedAttribute>() == null).ToArray();

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //Ignore TomlNonSerializedAttribute Decorated Properties
            var propsDict = props
                .Where(p => p.GetSetMethod(true) != null && p.GetCustomAttribute<TomlNonSerializedAttribute>() == null)
                .Select(p => new KeyValuePair<PropertyInfo, TomlPropertyAttribute?>(p, p.GetCustomAttribute<TomlPropertyAttribute>()))
                .ToDictionary(tuple => tuple.Key, tuple => tuple.Value);

            if (fields.Length + propsDict.Count == 0)
                return _ =>
                {
                    try
                    {
                        return Activator.CreateInstance(type)!;
                    }
                    catch (MissingMethodException)
                    {
                        throw new TomlInstantiationException(type);
                    }
                };

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
                    if (!table.TryGetValue(field.Name, out var entry))
                        continue; //TODO: Do we want to make this configurable? As in, throw exception if data is missing?

                    object fieldValue;
                    try
                    {
                        fieldValue = TomlSerializationMethods.GetDeserializer(field.FieldType).Invoke(entry!);
                    }
                    catch (TomlTypeMismatchException e)
                    {
                        throw new TomlFieldTypeMismatchException(type, field, e);
                    }

                    field.SetValue(instance, fieldValue);
                }

                foreach (var (prop, attribute) in propsDict)
                {
                    var name = attribute?.GetMappedString() ?? prop.Name;
                    if (!table.TryGetValue(name, out var entry))
                        continue; //TODO: As above, configurable?

                    object propValue;

                    try
                    {
                        propValue = TomlSerializationMethods.GetDeserializer(prop.PropertyType).Invoke(entry!);
                    } catch (TomlTypeMismatchException e)
                    {
                        throw new TomlPropertyTypeMismatchException(type, prop, e);
                    }

                    prop.SetValue(instance, propValue, null);
                }

                return instance;
            };
        }

        //Cache composite deserializer.
        TomlSerializationMethods.Register(type, null, deserializer);

        return deserializer;
    }
}