using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Tomlet.Attributes;
using Tomlet.Extensions;
using Tomlet.Models;

namespace Tomlet;

internal static class TomlCompositeSerializer
{
#if MODERN_DOTNET
    [UnconditionalSuppressMessage("AOT", "IL2072", Justification = "Any field that is being serialized must have been used as a field in the consuming code in order for the code path that queries it to run, so the dynamic code requirement is already satisfied.")]
    public static TomlSerializationMethods.Serialize<object> For([DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] Type type, TomlSerializerOptions options)
#else
    public static TomlSerializationMethods.Serialize<object> For(Type type, TomlSerializerOptions options)
#endif
    {
        TomlSerializationMethods.Serialize<object> serializer;

        if (type.IsEnum)
        {
            var stringSerializer = TomlSerializationMethods.GetSerializer(typeof(string), options);
            serializer = o => stringSerializer.Invoke(Enum.GetName(type, o!) ?? throw new ArgumentException($"Tomlet: Cannot serialize {o} as an enum of type {type} because the enum type does not declare a name for that value"));
        }
        else
        {
            //Get all instance fields
            var memberFlags = BindingFlags.Public | BindingFlags.Instance;
            if (!options.IgnoreNonPublicMembers) {
                memberFlags |= BindingFlags.NonPublic;
            }

            var fields = type.GetFields(memberFlags);
            var fieldAttribs = fields
                .ToDictionary(f => f, f => new
                {
                    inline = GenericExtensions.GetCustomAttribute<TomlInlineCommentProviderAttribute>(f), 
                    preceding = GenericExtensions.GetCustomAttribute<TomlPrecedingCommentProviderAttribute>(f), 
                    noInline = GenericExtensions.GetCustomAttribute<TomlDoNotInlineObjectAttribute>(f)
                });
            var props = type.GetProperties(memberFlags)
                .ToArray();
            var propAttribs = props
                .ToDictionary(p => p, p => new
                {
                    inline = GenericExtensions.GetCustomAttribute<TomlInlineCommentProviderAttribute>(p), 
                    preceding = GenericExtensions.GetCustomAttribute<TomlPrecedingCommentProviderAttribute>(p), 
                    prop = GenericExtensions.GetCustomAttribute<TomlPropertyAttribute>(p), 
                    noInline = GenericExtensions.GetCustomAttribute<TomlDoNotInlineObjectAttribute>(p)
                });

            var isForcedNoInline = GenericExtensions.GetCustomAttribute<TomlDoNotInlineObjectAttribute>(type) != null;

            //Ignore NonSerialized and CompilerGenerated fields.
            fields = fields.Where(f => !(f.IsNotSerialized || GenericExtensions.GetCustomAttribute<TomlNonSerializedAttribute>(f) != null)
                && GenericExtensions.GetCustomAttribute<CompilerGeneratedAttribute>(f) == null 
                && !f.Name.Contains('<')).ToArray();

            //Ignore TomlNonSerializedAttribute Decorated Properties
            props = props.Where(p => GenericExtensions.GetCustomAttribute<TomlNonSerializedAttribute>(p) == null).ToArray();

            if (fields.Length + props.Length == 0)
                return _ => new TomlTable();

            serializer = instance =>
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance), "Object being serialized is null. TOML does not support null values.");

                var resultTable = new TomlTable {ForceNoInline = isForcedNoInline};

                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(instance);

                    if (fieldValue == null)
                        continue; //Skip nulls - TOML doesn't support them.

                    var tomlValue = TomlSerializationMethods.GetSerializer(field.FieldType, options).Invoke(fieldValue);
                    
                    if(tomlValue == null)
                        continue;
                    
                    var commentAttribs = fieldAttribs[field];

                    if (resultTable.ContainsKey(field.Name))
                        //Do not overwrite fields if they have the same name as something already in the table
                        //This fixes serializing types which re-declare a field using the `new` keyword, overwriting a field of the same name
                        //in its supertype. 
                        continue;

                    tomlValue.Comments.InlineComment = commentAttribs.inline?.GetComment();
                    tomlValue.Comments.PrecedingComment = commentAttribs.preceding?.GetComment();
                    
                    if(commentAttribs.noInline != null && tomlValue is TomlTable table)
                        table.ForceNoInline = true;

                    resultTable.PutValue(field.Name, tomlValue);
                }

                foreach (var prop in props)
                {
                    if(prop.GetGetMethod(true) == null)
                        continue; //Skip properties without a getter
                    
                    if(prop.Name == "EqualityContract")
                        continue; //Skip record equality contract property. Wish there was a better way to do this.
                    
                    var propValue = prop.GetValue(instance, null);
                    
                    if(propValue == null)
                        continue;
                    
                    var tomlValue = TomlSerializationMethods.GetSerializer(prop.PropertyType, options).Invoke(propValue);

                    if (tomlValue == null) 
                        continue;
                    
                    var thisPropAttribs = propAttribs[prop];
                    
                    tomlValue.Comments.InlineComment = thisPropAttribs.inline?.GetComment();
                    tomlValue.Comments.PrecedingComment = thisPropAttribs.preceding?.GetComment();

                    if (thisPropAttribs.noInline != null && tomlValue is TomlTable table)
                        table.ForceNoInline = true;

                    resultTable.PutValue(thisPropAttribs.prop?.GetMappedString() ?? prop.Name, tomlValue);
                }

                return resultTable;
            };
        }

        //Cache composite deserializer.
        TomlSerializationMethods.Register(type, serializer, null);

        return serializer;
    }
}