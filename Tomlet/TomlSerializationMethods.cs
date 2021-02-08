using System;
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

        public delegate TomlValue Serialize<in T>(T t);

        private static Dictionary<Type, Delegate> _deserializers = new ();
        private static Dictionary<Type, Delegate> _serializers = new();

        static TomlSerializationMethods()
        {
            //Register built-in serializers
            
            //String
            Register(s => new TomlString(s!), value => (value as TomlString)?.Value ?? value.StringValue);
            
            //Long
            Register(l => new TomlLong(l), value => (value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType()));
            
            //Int
            Register(i => new TomlLong(i), value => (int) ((value as TomlLong)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType())));
            
            //Bool
            Register(TomlBoolean.ValueOf, value => (value as TomlBoolean)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlBoolean), value.GetType()));
            
            //Double
            Register(d => new TomlDouble(d), value => (value as TomlDouble)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlDouble), value.GetType()));

            //Float
            Register(f => new TomlDouble(f), value => (float) ((value as TomlDouble)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlDouble), value.GetType())));
            
            //LocalDate(Time)
            Register(dt => dt.TimeOfDay == TimeSpan.Zero ? new TomlLocalDate(dt) : new TomlLocalDateTime(dt), value => (value as TomlValueWithDateTime)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlValueWithDateTime), value.GetType()));
            
            //OffsetDateTime
            Register(odt => new TomlOffsetDateTime(odt), value => (value as TomlOffsetDateTime)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlOffsetDateTime), value.GetType()));
            
            //LocalTime
            Register(lt => new TomlLocalTime(lt), value => (value as TomlLocalTime)?.Value ?? throw new TomlTypeMismatchException(typeof(TomlLocalTime), value.GetType()));
        }

        internal static Serialize<T>? GetSerializer<T>()
        {
            if (_serializers.TryGetValue(typeof(T), out var value))
                return (Serialize<T>) value;
            
            return null;
        }
        
        internal static Deserialize<T>? GetDeserializer<T>()
        {
            if (_deserializers.TryGetValue(typeof(T), out var deserializer))
                return value => (T) deserializer.DynamicInvoke(value);
            
            return null;
        }
        
        internal static Deserialize<object>? GetDeserializer(Type t)
        {
            if (_deserializers.TryGetValue(t, out var value))
                return (Deserialize<object>) value;
            
            return null;
        }

        internal static Deserialize<T> GetCompositeDeserializer<T>()
        {
            var deserializer = GetCompositeDeserializer(typeof(T));

            return value => (T) deserializer.Invoke(value);
        }
        
        internal static Deserialize<object> GetCompositeDeserializer(Type type)
        {
            //Get all instance fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //Ignore NonSerialized fields.
            fields = fields.Where(f => !f.IsNotSerialized).ToArray();

            if(fields.Length == 0)
                return _ => Activator.CreateInstance(type);

            var deserializer = (Deserialize<object>) (value =>
            {
                if (!(value is TomlTable table))
                    throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType());

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
                        var entry = table.GetValue(field.Name);
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
            });

            //Cache composite deserializer.
            _deserializers[type] = deserializer;

            return deserializer;
        }

        public static void Register<T>(Serialize<T>? serializer, Deserialize<T>? deserializer)
        {
            if (serializer != null)
            {
                RegisterSerializer(serializer);
                
                RegisterSerializer<T[]>(arr => new TomlArray(arr.Select(serializer.Invoke).ToList()));
                RegisterSerializer<List<T>>(arr => new TomlArray(arr.Select(serializer.Invoke).ToList()));
                RegisterDictionarySerializer(serializer);
            }

            if (deserializer != null)
            {
                RegisterDeserializer<T>(deserializer);
                RegisterDeserializer<T[]>(value => value is TomlArray arr ? arr.ArrayValues.Select(deserializer.Invoke).ToArray() : throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType()));
                RegisterDeserializer<List<T>>(value => value is TomlArray arr ? arr.ArrayValues.Select(deserializer.Invoke).ToList() : throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType()));
                RegisterDictionaryDeserializer(deserializer);
            }
        }
        
        private static void RegisterDeserializer<T>(Deserialize<T> deserializer)
        {
            Deserialize<object> boxed = value => (object) deserializer.Invoke(value);
            _deserializers[typeof(T)] = boxed;
        }

        private static void RegisterSerializer<T>(Serialize<T> serializer)
        {
            Serialize<object> boxed = value => serializer.Invoke((T) value);
            _serializers[typeof(T)] = boxed;
        }

        private static void RegisterDictionarySerializer<T>(Serialize<T> serializer)
        {
            RegisterSerializer<Dictionary<string, T>>(dict =>
            {
                var keys = dict.Keys.ToList();
                var values = dict.Values.Select(serializer.Invoke).ToList();

                var table = new TomlTable();

                for (var i = 0; i < keys.Count; i++)
                {
                    table.PutValue(keys[i], values[i], true);
                }

                return table;
            });
        }

        private static void RegisterDictionaryDeserializer<T>(Deserialize<T> deserializer)
        {
            RegisterDeserializer<Dictionary<string, T>>(value =>
            {
                if(!(value is TomlTable table))
                    throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType());

                return table.Entries
                    .Select(kvp => new KeyValuePair<string, T>(kvp.Key, deserializer.Invoke(kvp.Value)))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            });
        }
    }
}