using System;
using System.Diagnostics.CodeAnalysis;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace Tomlet;

//Api class, these are supposed to be exposed
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class TomletMain
{
    [Attributes.ExcludeFromCodeCoverage]
    public static void RegisterMapper<T>(TomlSerializationMethods.Serialize<T>? serializer, TomlSerializationMethods.Deserialize<T>? deserializer)
        => TomlSerializationMethods.Register(serializer, deserializer);

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static T To<[DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(string tomlString, TomlSerializerOptions? options = null)
#else
        public static T To<T>(string tomlString, TomlSerializerOptions? options = null)
#endif
    {
        return (T)To(typeof(T), tomlString, options);
    }

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static object To([DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type what, string tomlString, TomlSerializerOptions? options = null)
#else
        public static object To(Type what, string tomlString, TomlSerializerOptions? options = null)
#endif
    {
        var parser = new TomlParser();
        var tomlDocument = parser.Parse(tomlString);

        return To(what, tomlDocument, options);
    }

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static T To<[DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(TomlValue value, TomlSerializerOptions? options = null)
#else
        public static T To<T>(TomlValue value, TomlSerializerOptions? options = null)
#endif
    {
        return (T)To(typeof(T), value, options);
    }

#if MODERN_DOTNET
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of deserialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static object To([DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type what, TomlValue value, TomlSerializerOptions? options = null)
#else
        public static object To(Type what, TomlValue value, TomlSerializerOptions? options = null)
#endif
    {
        var deserializer = TomlSerializationMethods.GetDeserializer(what, options);

        return deserializer.Invoke(value);
    }

#if MODERN_DOTNET
    [return: NotNullIfNotNull("t")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static TomlValue? ValueFrom<[DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(T? t, TomlSerializerOptions? options = null)
#else
        public static TomlValue? ValueFrom<T>(T? t, TomlSerializerOptions? options = null)
#endif
    {
        if (t == null)
            return null;

        return ValueFrom(typeof(T), t, options);
    }

#if MODERN_DOTNET
    [return: NotNullIfNotNull("t")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static TomlValue? ValueFrom([DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, object? t, TomlSerializerOptions? options = null)
#else
        public static TomlValue? ValueFrom(Type type, object? t, TomlSerializerOptions? options = null)
#endif
    {
        if (t == null)
            return null;
        
        var serializer = TomlSerializationMethods.GetSerializer(type, options);

        var tomlValue = serializer.Invoke(t);

        return tomlValue!;
    }

#if MODERN_DOTNET
    [return: NotNullIfNotNull("t")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static TomlDocument? DocumentFrom<[DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(T? t, TomlSerializerOptions? options = null)
#else
        public static TomlDocument? DocumentFrom<T>(T? t, TomlSerializerOptions? options = null)
#endif
    {
        if (t == null)
            return null;

        return DocumentFrom(typeof(T), t, options);
    }

#if MODERN_DOTNET
    [return: NotNullIfNotNull("t")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static TomlDocument? DocumentFrom([DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, object? t, TomlSerializerOptions? options = null)
#else
        public static TomlDocument? DocumentFrom(Type type, object? t, TomlSerializerOptions? options = null)
#endif
    {
        if (t == null)
            return null;
        
        var val = ValueFrom(type, t, options);

        return val switch
        {
            TomlDocument doc => doc,
            TomlTable table => new TomlDocument(table),
            _ => throw new TomlPrimitiveToDocumentException(type)
        };
    }

#if MODERN_DOTNET
    [return: NotNullIfNotNull("t")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static string? TomlStringFrom<[DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(T? t, TomlSerializerOptions? options = null) => DocumentFrom(t, options)?.SerializedValue;
    
    [return: NotNullIfNotNull("t")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The native code for underlying implementations of serialize helper methods may not be available for a given type.")]
#endif // NET7_0_OR_GREATER
    public static string? TomlStringFrom([DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, object? t, TomlSerializerOptions? options = null) => DocumentFrom(type, t, options)?.SerializedValue;

#else
        public static string? TomlStringFrom<T>(T? t, TomlSerializerOptions? options = null) => DocumentFrom(t, options)?.SerializedValue;

        public static string? TomlStringFrom(Type type, object? t, TomlSerializerOptions? options = null) => DocumentFrom(type, t, options)?.SerializedValue;
#endif
}