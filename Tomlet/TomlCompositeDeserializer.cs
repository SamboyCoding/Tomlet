using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
#if MODERN_DOTNET
    [UnconditionalSuppressMessage("AOT", "IL2072", Justification = "Any field that is being deserialized to must have been used as a field in the consuming code in order for the code path that queries it to run, so the dynamic code requirement is already satisfied.")]
    public static TomlSerializationMethods.Deserialize<object> For([DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, TomlSerializerOptions options)
#else
    public static TomlSerializationMethods.Deserialize<object> For(Type type, TomlSerializerOptions options)
#endif
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
                    if(options.IgnoreInvalidEnumValues)
                        return Enum.GetValues(type).GetValue(0)!;
                    
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
            var fieldsDict = fields
                .Where(f => !f.IsNotSerialized && GenericExtensions.GetCustomAttribute<CompilerGeneratedAttribute>(f) == null)
                .Select(f => new KeyValuePair<FieldInfo, TomlFieldAttribute?>(f, GenericExtensions.GetCustomAttribute<TomlFieldAttribute>(f)))
                .ToDictionary(tuple => tuple.Key, tuple => tuple.Value);

            var props = type.GetProperties(memberFlags);

            //Ignore TomlNonSerializedAttribute Decorated Properties
            var propsDict = props
                .Where(p => p.GetSetMethod(true) != null && GenericExtensions.GetCustomAttribute<TomlNonSerializedAttribute>(p) == null)
                .Select(p => new KeyValuePair<PropertyInfo, TomlPropertyAttribute?>(p, GenericExtensions.GetCustomAttribute<TomlPropertyAttribute>(p)))
                .ToDictionary(tuple => tuple.Key, tuple => tuple.Value);

            if (fieldsDict.Count + propsDict.Count == 0)
                return value => CreateInstance(type, value, options, out _);

            deserializer = value =>
            {
                if (value is not TomlTable table)
                    throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType(), type);

                var instance = CreateInstance(type, value, options, out var assignedMembers);

                foreach (var (field, attribute) in fieldsDict)
                {
                    var name = attribute?.GetMappedString() ?? field.Name;
                    if (!options.OverrideConstructorValues && assignedMembers.Contains(name))
                        continue;
                        
                    if (!table.TryGetValue(name, out var entry))
                        continue; //TODO: Do we want to make this configurable? As in, throw exception if data is missing?

                    object fieldValue;
                    try
                    {
                        fieldValue = TomlSerializationMethods.GetDeserializer(field.FieldType, options).Invoke(entry);
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
                        propValue = TomlSerializationMethods.GetDeserializer(prop.PropertyType, options).Invoke(entry);
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

#if MODERN_DOTNET
    [UnconditionalSuppressMessage("AOT", "IL2072", Justification = "Any constructor parameter must have been used somewhere in the consuming code in order for the code path that queries it to run, so the dynamic code requirement is already satisfied.")]
    private static object CreateInstance([DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, TomlValue tomlValue, TomlSerializerOptions options, out HashSet<string> assignedMembers)
#else
    private static object CreateInstance(Type type, TomlValue tomlValue, TomlSerializerOptions options, out HashSet<string> assignedMembers)
#endif
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
                argument = TomlSerializationMethods.GetDeserializer(parameter.ParameterType, options).Invoke(entry);
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