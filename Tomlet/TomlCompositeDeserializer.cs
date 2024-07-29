using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Tomlet.Attributes;
using Tomlet.Exceptions;
using Tomlet.Extensions;
using Tomlet.Models;

namespace Tomlet;

internal static class TomlCompositeDeserializer
{
    public static TomlSerializationMethods.Deserialize<object> For(Type type, TomlSerializerOptions options)
    {
        TomlSerializationMethods.Deserialize<object> deserializer;
        if (type.IsEnum)
        {
            var stringDeserializer = TomlSerializationMethods.GetDeserializer(typeof(string), options);
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
            var memberFlags = BindingFlags.Public | BindingFlags.Instance;
            if (!options.IgnoreNonPublicMembers) {
                memberFlags |= BindingFlags.NonPublic;
            }

            var fields = type.GetFields(memberFlags);

            //Ignore NonSerialized and CompilerGenerated fields.
            fields = fields.Where(f => !f.IsNotSerialized && GenericExtensions.GetCustomAttribute<CompilerGeneratedAttribute>(f) == null).ToArray();

            var props = type.GetProperties(memberFlags);

            //Ignore TomlNonSerializedAttribute Decorated Properties
            var propsDict = props
                .Where(p => p.GetSetMethod(true) != null && GenericExtensions.GetCustomAttribute<TomlNonSerializedAttribute>(p) == null)
                .Select(p => new KeyValuePair<PropertyInfo, TomlPropertyAttribute?>(p, GenericExtensions.GetCustomAttribute<TomlPropertyAttribute>(p)))
                .ToDictionary(tuple => tuple.Key, tuple => tuple.Value);

            if (fields.Length + propsDict.Count == 0)
                return value => CreateInstance(type, value, options, out _);

            deserializer = value =>
            {
                if (value is not TomlTable table)
                    throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType(), type);

                var instance = CreateInstance(type, value, options, out var assignedMembers);

                foreach (var field in fields)
                {
                    if (!options.OverrideConstructorValues && assignedMembers.Contains(field.Name))
                        continue;
                        
                    if (!table.TryGetValue(field.Name, out var entry))
                        continue; //TODO: Do we want to make this configurable? As in, throw exception if data is missing?

                    object fieldValue;
                    try
                    {
                        fieldValue = TomlSerializationMethods.GetDeserializer(field.FieldType, options).Invoke(entry!);
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
                    if (!options.OverrideConstructorValues && assignedMembers.Contains(name))
                        continue;
                        
                    if (!table.TryGetValue(name, out var entry))
                        continue; //TODO: As above, configurable?

                    object propValue;

                    try
                    {
                        propValue = TomlSerializationMethods.GetDeserializer(prop.PropertyType, options).Invoke(entry!);
                    }
                    catch (TomlTypeMismatchException e)
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

    private static object CreateInstance(Type type, TomlValue tomlValue, TomlSerializerOptions options, out HashSet<string> assignedMembers)
    {
        if (tomlValue is not TomlTable table)
            throw new TomlTypeMismatchException(typeof(TomlTable), tomlValue.GetType(), type);
        
        if (!type.TryGetBestMatchConstructor(out var constructor))
        {
            throw new TomlInstantiationException();
        }

        var parameters = constructor!.GetParameters();
        if (parameters.Length == 0)
        {
            assignedMembers = new HashSet<string>();
            return constructor.Invoke(null);
        }

        assignedMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var arguments = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            object argument;
            
            if (!table.TryGetValue(parameter.Name!.ToPascalCase(), out var entry))
                continue;

            try
            {
                argument = TomlSerializationMethods.GetDeserializer(parameter.ParameterType, options).Invoke(entry!);
            }
            catch (TomlTypeMismatchException e)
            {
                throw new TomlParameterTypeMismatchException(parameter.ParameterType, parameter, e);
            }

            arguments[i] = argument;
            assignedMembers.Add(parameter.Name!);
        }

        return constructor.Invoke(arguments);
    }
}