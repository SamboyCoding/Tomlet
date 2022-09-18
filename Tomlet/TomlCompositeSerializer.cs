using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Tomlet.Attributes;
using Tomlet.Models;

namespace Tomlet;

internal static class TomlCompositeSerializer
{
    public static TomlSerializationMethods.Serialize<object> For(Type type)
    {
        TomlSerializationMethods.Serialize<object> serializer;

        if (type.IsEnum)
        {
            var stringSerializer = TomlSerializationMethods.GetSerializer(typeof(string));
            serializer = o => stringSerializer.Invoke(Enum.GetName(type, o!) ?? throw new ArgumentException($"Tomlet: Cannot serialize {o} as an enum of type {type} because the enum type does not declare a name for that value"));
        }
        else
        {
            //Get all instance fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldAttribs = fields
                .ToDictionary(f => f, f => new {inline = f.GetCustomAttribute<TomlInlineCommentAttribute>(), preceding = f.GetCustomAttribute<TomlPrecedingCommentAttribute>()});
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ToArray();
            var propAttribs = props
                .ToDictionary(p => p, p => new {inline = p.GetCustomAttribute<TomlInlineCommentAttribute>(), preceding = p.GetCustomAttribute<TomlPrecedingCommentAttribute>(), prop = p.GetCustomAttribute<TomlPropertyAttribute>()});

            var isForcedNoInline = type.GetCustomAttribute<TomlDoNotInlineObjectAttribute>() != null;

            //Ignore NonSerialized and CompilerGenerated fields.
            fields = fields.Where(f => !f.IsNotSerialized && f.GetCustomAttribute<CompilerGeneratedAttribute>() == null && !f.Name.Contains('<')).ToArray();

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

                    var tomlValue = TomlSerializationMethods.GetSerializer(field.FieldType).Invoke(fieldValue);
                    var commentAttribs = fieldAttribs[field];

                    if (resultTable.ContainsKey(field.Name))
                        //Do not overwrite fields if they have the same name as something already in the table
                        //This fixes serializing types which re-declare a field using the `new` keyword, overwriting a field of the same name
                        //in its supertype. 
                        continue;

                    tomlValue.Comments.InlineComment = commentAttribs.inline?.Comment;
                    tomlValue.Comments.PrecedingComment = commentAttribs.preceding?.Comment;

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
                    
                    var tomlValue = TomlSerializationMethods.GetSerializer(prop.PropertyType).Invoke(propValue);
                    var thisPropAttribs = propAttribs[prop];
                    
                    tomlValue.Comments.InlineComment = thisPropAttribs.inline?.Comment;
                    tomlValue.Comments.PrecedingComment = thisPropAttribs.preceding?.Comment;

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