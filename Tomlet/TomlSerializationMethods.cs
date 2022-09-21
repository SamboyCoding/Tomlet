using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Tomlet.Attributes;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace Tomlet
{
    public static class TomlSerializationMethods
    {
        private static MethodInfo _stringKeyedDictionaryMethod = typeof(TomlSerializationMethods).GetMethod(nameof(StringKeyedDictionaryDeserializerFor), BindingFlags.Static | BindingFlags.NonPublic)!;
        private static MethodInfo _genericDictionarySerializerMethod = typeof(TomlSerializationMethods).GetMethod(nameof(GenericDictionarySerializer), BindingFlags.Static | BindingFlags.NonPublic)!;
        private static MethodInfo _genericNullableSerializerMethod = typeof(TomlSerializationMethods).GetMethod(nameof(GenericNullableSerializer), BindingFlags.Static | BindingFlags.NonPublic)!;

        public delegate T Deserialize<out T>(TomlValue value);

        public delegate TomlValue? Serialize<in T>(T? t);

        private static readonly Dictionary<Type, Delegate> Deserializers = new();
        private static readonly Dictionary<Type, Delegate> Serializers = new();


        [NoCoverage]
        static TomlSerializationMethods()
        {
            //Register built-in serializers

            //String
            Register(s => new TomlString(s!), value => (value as TomlString)?.Value ?? value.StringValue);

            //Bool
            Register(TomlBoolean.ValueOf, value => (value as TomlBoolean)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlBoolean), value.GetType(), typeof(bool)));

            //Byte
            Register(i => new TomlLong(i), value => (byte)((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(byte))));

            //SByte
            Register(i => new TomlLong(i), value => (sbyte)((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(sbyte))));

            //UShort
            Register(i => new TomlLong(i), value => (ushort)((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(ushort))));

            //Short
            Register(i => new TomlLong(i), value => (short)((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(short))));

            //UInt
            Register(i => new TomlLong(i), value => (uint)((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(uint))));

            //Int
            Register(i => new TomlLong(i), value => (int)((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(int))));

            //ULong
            Register(l => new TomlLong((long)l), value => (ulong)((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(ulong))));

            //Long
            Register(l => new TomlLong(l), value => (value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(long)));

            //Double
            Register(d => new TomlDouble(d), value => (value as TomlDouble)?.Value ?? (value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlDouble), value.GetType(), typeof(double)));

            //Float
            Register(f => new TomlDouble(f), value => (float)((value as TomlDouble)?.Value ?? (value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlDouble), value.GetType(), typeof(float))));

            //LocalDate(Time)
            Register(dt => dt.TimeOfDay == TimeSpan.Zero ? new TomlLocalDate(dt) : new TomlLocalDateTime(dt), value => (value as ITomlValueWithDateTime)?.Value ?? throw new TomlTypeMismatchException(typeof(ITomlValueWithDateTime), value.GetType(), typeof(DateTime)));

            //OffsetDateTime
            Register(odt => new TomlOffsetDateTime(odt), value => (value as TomlOffsetDateTime)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlOffsetDateTime), value.GetType(), typeof(DateTimeOffset)));

            //LocalTime
            Register(lt => new TomlLocalTime(lt), value => (value as TomlLocalTime)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLocalTime), value.GetType(), typeof(TimeSpan)));
        }

        internal static Serialize<object> GetSerializer(Type t)
        {
            if (Serializers.TryGetValue(t, out var value))
                return (Serialize<object>)value;

            if (t.IsArray || t.Namespace == "System.Collections.Generic" && t.Name == "List`1")
            {
                var arrSerializer = GenericEnumerableSerializer();
                Serializers[t] = arrSerializer;
                return arrSerializer;
            }

            if (t.IsGenericType &&  t.GetGenericArguments() is { } genericArgs)
            {
                if (t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var serializer = _genericDictionarySerializerMethod.MakeGenericMethod(genericArgs);

                    var del = Delegate.CreateDelegate(typeof(Serialize<>).MakeGenericType(t), serializer);
                    var ret = (Serialize<object>)(dict => (TomlValue?)del.DynamicInvoke(dict));
                    Serializers[t] = ret;

                    return ret;
                }

                if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var serializer = _genericNullableSerializerMethod.MakeGenericMethod(genericArgs);
                    
                    var del = Delegate.CreateDelegate(typeof(Serialize<>).MakeGenericType(t), serializer);
                    var ret = (Serialize<object>)(dict => (TomlValue?)del.DynamicInvoke(dict));
                    Serializers[t] = ret;
                    
                    return ret;
                }
            }

            return TomlCompositeSerializer.For(t);
        }

        internal static Deserialize<object> GetDeserializer(Type t)
        {
            if (Deserializers.TryGetValue(t, out var value))
                return (Deserialize<object>)value;

            if (t.IsArray)
            {
                var arrayDeserializer = ArrayDeserializerFor(t.GetElementType()!);
                Deserializers[t] = arrayDeserializer;
                return arrayDeserializer;
            }

            if (t.Namespace == "System.Collections.Generic" && t.Name == "List`1")
            {
                var listDeserializer = ListDeserializerFor(t.GetGenericArguments()[0]);
                Deserializers[t] = listDeserializer;
                return listDeserializer;
            }
            
            if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>) && t.GetGenericArguments() is {Length: 1} genericArguments)
            {
                var nullableDeserializer = NullableDeserializerFor(t);
                Deserializers[t] = nullableDeserializer;
                return nullableDeserializer;
            }

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>) && t.GetGenericArguments() is { Length: 2 } genericArgs)
            {
                if (genericArgs[0] == typeof(string))
                {
#if NETFRAMEWORK
                    return (Deserialize<object>)_stringKeyedDictionaryMethod.MakeGenericMethod(genericArgs[1]).Invoke(null, new object[0])!;
#else
                    return (Deserialize<object>)_stringKeyedDictionaryMethod.MakeGenericMethod(genericArgs[1]).Invoke(null, Array.Empty<object>())!;
#endif
                }
            }

            return TomlCompositeDeserializer.For(t);
        }

        private static Serialize<object?> GenericEnumerableSerializer() =>
            o =>
            {
                if (o is not IEnumerable arr)
                    throw new Exception("How did ArraySerializer end up getting a non-array?");

                var ret = new TomlArray();
                foreach (var entry in arr)
                {
                    ret.Add(entry);
                }

                return ret;
            };

        private static Deserialize<object> ArrayDeserializerFor(Type elementType) =>
            value =>
            {
                if (value is not TomlArray tomlArray)
                    throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType(), elementType.MakeArrayType());

                var ret = Array.CreateInstance(elementType, tomlArray.Count);
                var deserializer = GetDeserializer(elementType);
                for (var index = 0; index < tomlArray.ArrayValues.Count; index++)
                {
                    var arrayValue = tomlArray.ArrayValues[index];
                    ret.SetValue(deserializer(arrayValue), index);
                }

                return ret;
            };

        private static Deserialize<object> ListDeserializerFor(Type elementType)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var relevantAddMethod = listType.GetMethod("Add")!;

            return value =>
            {
                if (value is not TomlArray tomlArray)
                    throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType(), listType);

                var ret = Activator.CreateInstance(listType)!;
                var deserializer = GetDeserializer(elementType);

                foreach (var arrayValue in tomlArray.ArrayValues)
                {
                    relevantAddMethod.Invoke(ret, new[] { deserializer(arrayValue) });
                }

                return ret;
            };
        }
        
        private static Deserialize<object> NullableDeserializerFor(Type nullableType)
        {
            var elementType = nullableType.GetGenericArguments()[0];
            var elementDeserializer = GetDeserializer(elementType);
            
            return value =>
            {
                //If we're deserializing, we know the value is not null
                var element = elementDeserializer(value);
                return Activator.CreateInstance(nullableType, element)!;
            };
        }

        private static Deserialize<Dictionary<string, T>> StringKeyedDictionaryDeserializerFor<T>()
        {
            var deserializer = GetDeserializer(typeof(T));

            return value =>
            {
                if (value is not TomlTable table)
                    throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType(), typeof(Dictionary<string, T>));

                return table.Entries.ToDictionary(entry => entry.Key, entry => (T)deserializer(entry.Value));
            };
        }
        
        private static TomlValue? GenericNullableSerializer<T>(T? nullable) where T : struct
        {
            var elementSerializer = GetSerializer(typeof(T));
            
            if (nullable.HasValue)
                return elementSerializer(nullable.Value);

            return null;
        }

        private static TomlValue GenericDictionarySerializer<TKey, TValue>(Dictionary<TKey, TValue> dict) where TKey : notnull
        {
            var valueSerializer = GetSerializer(typeof(TValue));

            var ret = new TomlTable();
            foreach (var entry in dict)
            {
                var keyAsString = entry.Key?.ToString();
                
                if(keyAsString == null)
                    continue;

                var value = valueSerializer(entry.Value);
                
                if(value == null)
                    continue;
                
                ret.PutValue(keyAsString, value, true);
            }

            return ret;
        }

        internal static void Register<T>(Serialize<T>? serializer, Deserialize<T>? deserializer)
        {
            if (serializer != null)
            {
                RegisterSerializer(serializer);

                RegisterDictionarySerializer(serializer);
            }

            if (deserializer != null)
            {
                RegisterDeserializer(deserializer);
                RegisterDictionaryDeserializer(deserializer);
            }
        }

        internal static void Register(Type t, Serialize<object>? serializer, Deserialize<object>? deserializer)
        {
            if (serializer != null)
                RegisterSerializer(serializer);

            if (deserializer != null)
                RegisterDeserializer(deserializer);
        }

        private static void RegisterDeserializer<T>(Deserialize<T> deserializer)
        {
            object BoxedDeserializer(TomlValue value) => deserializer.Invoke(value) ?? throw new Exception($"TOML Deserializer returned null for type {nameof(T)}");
            Deserializers[typeof(T)] = (Deserialize<object>)BoxedDeserializer;
        }

        private static void RegisterSerializer<T>(Serialize<T> serializer)
        {
            TomlValue? ObjectAcceptingSerializer(object value) => serializer.Invoke((T)value);
            Serializers[typeof(T)] = (Serialize<object>)ObjectAcceptingSerializer!;
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