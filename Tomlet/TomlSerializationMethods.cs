using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace Tomlet
{
    public static class TomlSerializationMethods
    {
        public delegate T Deserialize<out T>(TomlValue value);

        public delegate TomlValue Serialize<in T>(T? t);

        private static readonly Dictionary<Type, Delegate> Deserializers = new();
        private static readonly Dictionary<Type, Delegate> Serializers = new();

        static TomlSerializationMethods()
        {
            //Register built-in serializers

            //String
            Register(s => new TomlString(s!), value => (value as TomlString)?.Value ?? value.StringValue);
            
            //Bool
            Register(TomlBoolean.ValueOf, value => (value as TomlBoolean)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlBoolean), value.GetType(), typeof(bool)));
            
            //Byte
            Register(i => new TomlLong(i), value => (byte) ((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(byte))));
            
            //SByte
            Register(i => new TomlLong(i), value => (sbyte) ((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(sbyte))));
            
            //UShort
            Register(i => new TomlLong(i), value => (ushort) ((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(ushort))));
            
            //Short
            Register(i => new TomlLong(i), value => (short) ((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(short))));

            //UInt
            Register(i => new TomlLong(i), value => (uint) ((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(uint))));
            
            //Int
            Register(i => new TomlLong(i), value => (int) ((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(int))));
            
            //ULong
            Register(l => new TomlLong((long) l), value => (ulong) ((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(ulong))));
            
            //Long
            Register(l => new TomlLong(l), value => (value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(long)));

            //Double
            Register(d => new TomlDouble(d), value => (value as TomlDouble)?.Value ?? (value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlDouble), value.GetType(), typeof(double)));

            //Float
            Register(f => new TomlDouble(f), value => (float) ((value as TomlDouble)?.Value ?? (value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlDouble), value.GetType(), typeof(float))));

            //LocalDate(Time)
            Register(dt => dt.TimeOfDay == TimeSpan.Zero ? new TomlLocalDate(dt) : new TomlLocalDateTime(dt), value => (value as ITomlValueWithDateTime)?.Value ?? throw new TomlTypeMismatchException(typeof(ITomlValueWithDateTime), value.GetType(), typeof(DateTime)));

            //OffsetDateTime
            Register(odt => new TomlOffsetDateTime(odt), value => (value as TomlOffsetDateTime)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlOffsetDateTime), value.GetType(), typeof(DateTimeOffset)));

            //LocalTime
            Register(lt => new TomlLocalTime(lt), value => (value as TomlLocalTime)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLocalTime), value.GetType(), typeof(TimeSpan)));
        }

        internal static Serialize<T>? GetSerializer<T>()
        {
            if (Serializers.TryGetValue(typeof(T), out var value))
                return (Serialize<T>) value;

            return null;
        }

        internal static Deserialize<T>? GetDeserializer<T>()
        {
            if (Deserializers.TryGetValue(typeof(T), out var deserializer))
                return value => (T) deserializer.DynamicInvoke(value)!;

            return null;
        }

        internal static Serialize<object>? GetSerializer(Type t)
        {
            if (Serializers.TryGetValue(t, out var value))
                return (Serialize<object>) value;

            return null;
        }

        internal static Deserialize<object>? GetDeserializer(Type t)
        {
            if (Deserializers.TryGetValue(t, out var value))
                return (Deserialize<object>) value;

            return null;
        }

        internal static Deserialize<T> GetCompositeDeserializer<T>()
        {
            var deserializer = GetCompositeDeserializer(typeof(T));

            return value => (T) deserializer.Invoke(value);
        }

        internal static Serialize<T> GetCompositeSerializer<T>()
        {
            var serializer = GetCompositeSerializer(typeof(T));

            return value => serializer.Invoke(value);
        }

        internal static Deserialize<object> GetCompositeDeserializer(Type type)
        {
            Deserialize<object> deserializer;
            if (type.IsEnum)
            {
                var stringDeserializer = GetDeserializer<string>()!;
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

                if (type.Namespace + "." + type.Name == "System.Collections.Generic.List`1")
                {
                    //List deserializer.

                    //Process base type
                    GetCompositeDeserializer(type.GetGenericArguments()[0]);

                    //And now return default list deserializer
                    return GetDeserializer(type)!;
                }
                
                if (type.IsArray)
                {
                    //Process base type
                    GetCompositeDeserializer(type.GetElementType()!);
                    
                    //And return default array deserializer
                    return GetDeserializer(type)!;
                }


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
                            var fieldDeserializer = GetDeserializer(field.FieldType) ?? GetCompositeDeserializer(field.FieldType);
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
            Register(type, null, deserializer);

            return deserializer;
        }

        internal static Serialize<object> GetCompositeSerializer(Type type)
        {
            Serialize<object> serializer;

            if (type.IsEnum)
            {
                var stringSerializer = GetSerializer<string>()!;
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
                    GetCompositeSerializer(type.GetGenericArguments()[0]);

                    //And now return default list serializer
                    return GetSerializer(type)!;
                }

                if (type.IsArray)
                {
                    //Process base type
                    GetCompositeSerializer(type.GetElementType()!);
                    
                    //And return default array serializer
                    return GetSerializer(type)!;
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

                        var fieldSerializer = GetSerializer(field.FieldType) ?? GetCompositeSerializer(field.FieldType);
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
            Register(type, serializer, null);

            return serializer;
        }

        internal static void Register<T>(Serialize<T>? serializer, Deserialize<T>? deserializer)
        {
            if (serializer != null)
            {
                RegisterSerializer(serializer);

                RegisterSerializer<T[]>(arr => new TomlArray(arr?.Select(serializer.Invoke).ToList() ?? new List<TomlValue>()));
                RegisterSerializer<List<T>>(arr => new TomlArray(arr?.Select(serializer.Invoke).ToList() ?? new List<TomlValue>()));
                RegisterDictionarySerializer(serializer);
            }

            if (deserializer != null)
            {
                RegisterDeserializer(deserializer);
                RegisterDeserializer(value => value is TomlArray arr ? arr.ArrayValues.Select(deserializer.Invoke).ToArray() : throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType(), typeof(T[])));
                RegisterDeserializer(value => value is TomlArray arr ? arr.ArrayValues.Select(deserializer.Invoke).ToList() : throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType(), typeof(List<T>)));
                RegisterDictionaryDeserializer(deserializer);
            }
        }

        internal static void Register(Type t, Serialize<object>? serializer, Deserialize<object>? deserializer)
        {
            if (serializer != null)
            {
                var listType = typeof(List<>).MakeGenericType(t);
                RegisterSerializer(serializer);

                RegisterSerializer(t.MakeArrayType(1), arr =>
                {
                    var ret = new TomlArray();

                    if (arr == null)
                        return ret;
                    
                    foreach (var o in (IEnumerable) arr)
                    {
                        ret.ArrayValues.Add(serializer.Invoke(o));
                    }

                    if (ret.ArrayValues.All(o => o is TomlTable))
                        ret.IsLockedToBeTableArray = true;

                    return ret;
                });
                RegisterSerializer(listType, arr =>
                {
                    var ret = new TomlArray();
                    
                    if (arr == null)
                        return ret;
                    
                    foreach (var o in (IEnumerable) arr)
                    {
                        ret.ArrayValues.Add(serializer.Invoke(o));
                    }

                    if (ret.ArrayValues.All(o => o is TomlTable))
                        ret.IsLockedToBeTableArray = true;

                    return ret;
                });
            }

            if (deserializer != null)
            {
                var listType = typeof(List<>).MakeGenericType(t);
                RegisterDeserializer(deserializer);
                RegisterDeserializer(t.MakeArrayType(1), value => value is TomlArray arr ? arr.ArrayValues.Select(deserializer.Invoke).ToArray() : throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType(), t.MakeArrayType(1)));
                RegisterDeserializer(listType, delegate(TomlValue value)
                {
                    if (value is not TomlArray arr)
                        throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType(), listType);

                    object o = Activator.CreateInstance(listType)!;

                    foreach (var o1 in arr.ArrayValues.Select(deserializer.Invoke))
                    {
                        o.GetType().GetMethod(nameof(List<object>.Add))!.Invoke(o, new[] {o1});
                    }

                    return o;
                });
            }
        }

        private static void RegisterDeserializer<T>(Deserialize<T> deserializer)
        {
            object BoxedDeserializer(TomlValue value) => deserializer.Invoke(value) ?? throw new Exception($"TOML Deserializer returned null for type {nameof(T)}");
            Deserializers[typeof(T)] = (Deserialize<object>) BoxedDeserializer;
        }

        private static void RegisterDeserializer(Type type, Deserialize<object> deserializer)
        {
            Deserializers[type] = deserializer;
        }

        private static void RegisterSerializer<T>(Serialize<T> serializer)
        {
            TomlValue ObjectAcceptingSerializer(object value) => serializer.Invoke((T) value);
            Serializers[typeof(T)] = (Serialize<object>) ObjectAcceptingSerializer!;
        }

        private static void RegisterSerializer(Type type, Serialize<object> serializer)
        {
            Serializers[type] = serializer;
        }

        private static void RegisterDictionarySerializer<T>(Serialize<T> serializer)
        {
            RegisterSerializer<Dictionary<string, T>>(dict =>
            {
                var table = new TomlTable();

                if (dict == null)
                    return table;
                
                var keys = dict.Keys.ToList();
                var values = dict.Values.Select(serializer.Invoke).ToList();

                for (var i = 0; i < keys.Count; i++)
                {
                    table.PutValue(keys[i], values[i], true);
                }

                return table;
            });
        }

        private static void RegisterDictionaryDeserializer<T>(Deserialize<T> deserializer)
        {
            RegisterDeserializer(value =>
            {
                if (value is not TomlTable table)
                    throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType(), typeof(Dictionary<string, T>));

                return table.Entries
                    .Select(kvp => new KeyValuePair<string, T>(kvp.Key, deserializer.Invoke(kvp.Value)))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            });
        }
    }
}