using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Tomlet.Exceptions;
using Tomlet.Extensions;

namespace Tomlet.Models
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TomlTable : TomlValue, IEnumerable<KeyValuePair<string, TomlValue>>
    {
        public readonly Dictionary<string, TomlValue> Entries = new();

        internal bool Locked;
        internal bool Defined;

        public bool ForceNoInline { get; set; }

        public override string StringValue => $"Table ({Entries.Count} entries)";

        public HashSet<string> Keys => new(Entries.Keys);

        public bool ShouldBeSerializedInline => !ForceNoInline && Entries.Count < 4
                                                               && Entries.All(e => !e.Key.Contains(" ")
                                                                                   && e.Value.Comments.ThereAreNoComments
                                                                                   && (e.Value is TomlArray arr ? arr.IsSimpleArray : e.Value is not TomlTable));

        public override string SerializedValue
        {
            get
            {
                if (!ShouldBeSerializedInline)
                    throw new Exception("Cannot use SerializeValue to serialize non-inline tables. Use SerializeNonInlineTable(keyName).");

                var builder = new StringBuilder("{ ");

                builder.Append(string.Join(", ", Entries.Select(o => TomlKeyUtils.FullStringToProperKey(o.Key) + " = " + o.Value.SerializedValue).ToArray()));

                builder.Append(" }");

                return builder.ToString();
            }
        }

        public string SerializeNonInlineTable(string? keyName, bool includeHeader = true)
        {
            var builder = new StringBuilder();
            if (includeHeader)
            {
                builder.Append('[').Append(keyName).Append("]");

                //For non-inline tables, the inline comment goes on the header line.
                if (Comments.InlineComment != null)
                    builder.Append(" # ").Append(Comments.InlineComment);

                builder.Append('\n');
            }

            //Three passes: Simple key-value pairs including inline arrays and tables, sub-tables, then sub-table-arrays.
            foreach (var (subKey, value) in Entries)
            {
                if (value is TomlTable { ShouldBeSerializedInline: false } or TomlArray { CanBeSerializedInline: false })
                    continue;

                WriteValueToStringBuilder(keyName, subKey, builder);
            }

            foreach (var (subKey, value) in Entries)
            {
                if (value is not TomlTable { ShouldBeSerializedInline: false })
                    continue;

                WriteValueToStringBuilder(keyName, subKey, builder);
            }

            foreach (var (subKey, value) in Entries)
            {
                if (value is not TomlArray { CanBeSerializedInline: false })
                    continue;

                WriteValueToStringBuilder(keyName, subKey, builder);
            }

            return builder.ToString();
        }

        private void WriteValueToStringBuilder(string? keyName, string subKey, StringBuilder builder)
        {
            var value = GetValue(subKey);
            subKey = TomlKeyUtils.FullStringToProperKey(subKey);

            if (keyName != null)
                keyName = TomlKeyUtils.FullStringToProperKey(keyName);

            var fullSubKey = keyName == null ? subKey : $"{keyName}.{subKey}";

            var hadBlankLine = builder.Length < 2 || builder[builder.Length - 2] == '\n';

            //Handle any preceding comment - this will ALWAYS go before any sort of value
            if (value.Comments.PrecedingComment != null)
                builder.Append(value.Comments.FormatPrecedingComment())
                    .Append('\n');

            switch (value)
            {
                case TomlArray { CanBeSerializedInline: false } subArray:
                    if (!hadBlankLine)
                        builder.Append('\n');

                    builder.Append(subArray.SerializeTableArray(fullSubKey)); //No need to append newline as SerializeTableArray always ensure it ends with 2
                    return; //Return because we don't do newline or handle inline comment here.
                case TomlArray subArray:
                    builder.Append(subKey).Append(" = ").Append(subArray.SerializedValue);
                    break;
                case TomlTable { ShouldBeSerializedInline: true } subTable:
                    builder.Append(subKey).Append(" = ").Append(subTable.SerializedValue);
                    break;
                case TomlTable subTable:
                    builder.Append(subTable.SerializeNonInlineTable(fullSubKey)).Append('\n');
                    return; //Return because we don't handle inline comment here.
                default:
                    builder.Append(subKey).Append(" = ").Append(value.SerializedValue);
                    break;
            }

            //If we're here we did something resembling an inline value, even if that value is actually a multi-line array.

            //First off, handle the inline comment.
            if (value.Comments.InlineComment != null)
                builder.Append(" # ").Append(value.Comments.InlineComment);

            //Then append a newline
            builder.Append('\n');
        }
        
        internal void ParserPutValue(ref List<string> key, TomlValue value, int lineNumber)
        {
            // NB: key is ref to signal that it mutates!
            if (Locked)
                throw new TomlTableLockedException(lineNumber, string.Join(".", key.ToArray()));
            
            InternalPutValue(ref key, value, lineNumber);
        }

        public void PutValue(string key, TomlValue value, bool quote = false)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            InternalPutValue(key, value, null);
        }

        public void Put<T>(string key, T t, bool quote = false)
        {
            TomlValue? tomlValue;
            tomlValue = t is not TomlValue tv ? TomletMain.ValueFrom(t) : tv;

            if (tomlValue == null)
                throw new ArgumentException("Value to insert into TOML table serialized to null.", nameof(t));
            
            PutValue(key, tomlValue, quote);
        }
        
        private void InternalPutValue(ref List<string> key, TomlValue value, int? lineNumber)
        {
            // NB: key is ref to signal that it mutates!
            if (key.Count == 0)
            {
                // TODO: Check what should be done here
                throw new NoTomlKeyException(lineNumber ?? -1);
            }
            
            var ourKeyName = key[0];
            key.RemoveAt(0);

            // Do we have a dotted key?
            if (key.Count == 0)
            {
                // Non-dotted keys land here.
                if (Entries.ContainsKey(ourKeyName) && lineNumber.HasValue)
                    throw new TomlKeyRedefinitionException(lineNumber.Value, ourKeyName);

                Entries[ourKeyName] = value;
                return;
            }

            // Dotted keys land here
            if (!Entries.TryGetValue(ourKeyName, out var existingValue))
            {
                //We don't have a sub-table with this name defined. That's fine, make one.
                var subtable = new TomlTable();
                Entries[ourKeyName] = subtable;
                subtable.ParserPutValue(ref key, value, lineNumber!.Value);
                return;
            }

            //We have a key by this name already. Is it a table?
            if (existingValue is not TomlTable existingTable)
            {
                //No - throw an exception
                if (lineNumber.HasValue) throw new TomlDottedKeyParserException(lineNumber.Value, ourKeyName);

                throw new TomlDottedKeyException(ourKeyName);
            }

            //Yes, get the sub-table to handle the rest of the key
            existingTable.ParserPutValue(ref key, value, lineNumber!.Value);
        }
        
        private void InternalPutValue(string key, TomlValue value, int? lineNumber)
        {
            // Because we have a single key, we know it's not dotted
            if (Entries.ContainsKey(key) && lineNumber.HasValue)
                throw new TomlKeyRedefinitionException(lineNumber.Value, key);

            Entries[key] = value;
        }
        
        public bool ContainsKey(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            return Entries.ContainsKey(key);
        }

#if NET6_0
        public bool TryGetValue(string key, [NotNullWhen(true)] out TomlValue? value)
#else
        public bool TryGetValue(string key, out TomlValue? value)
#endif
        {
            if (ContainsKey(key))
                return (value = GetValue(key)) != null;

            value = null;
            return false;
        }

        /// <summary>
        /// Returns the raw instance of <see cref="TomlValue"/> associated with this key. You must cast to a sub-class and access its value
        /// yourself.
        /// Unlike all the specific getters, this Getter respects dotted keys and quotes. You must quote any keys which contain a dot if you want to access the key itself,
        /// not a sub-key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>An instance of <see cref="TomlValue"/> associated with this key.</returns>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public TomlValue GetValue(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (!Entries.ContainsKey(key))
                throw new TomlNoSuchValueException(key);

            return Entries[key];
        }

        /// <summary>
        /// Returns the string value associated with the provided key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The string value associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not a string.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public string GetString(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(key);

            if (value is not TomlString str)
                throw new TomlTypeMismatchException(typeof(TomlString), value.GetType(), typeof(string));

            return str.Value;
        }

        /// <summary>
        /// Returns the integer value associated with the provided key, downsized from a long.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The integer value associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not an integer type.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public int GetInteger(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(key);

            if (value is not TomlLong lng)
                throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(int));

            return (int)lng.Value;
        }

        /// <summary>
        /// Returns the long (64-bit int) value associated with the provided key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The long/64-bit integer value associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not an integer type.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public long GetLong(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(key);

            if (value is not TomlLong lng)
                throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(int));

            return lng.Value;
        }

        /// <summary>
        /// Returns the 32-bit floating-point value associated with the provided key, downsized from a double.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The float value associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not a floating-point type.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public float GetFloat(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(key);

            if (value is not TomlDouble dbl)
                throw new TomlTypeMismatchException(typeof(TomlDouble), value.GetType(), typeof(float));

            return (float)dbl.Value;
        }

        /// <summary>
        /// Returns the boolean value associated with the provided key
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The boolean value associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not a boolean.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public bool GetBoolean(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(key);

            if (value is not TomlBoolean b)
                throw new TomlTypeMismatchException(typeof(TomlBoolean), value.GetType(), typeof(bool));

            return b.Value;
        }

        /// <summary>
        /// Returns the TOML array associated with the provided key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The TOML array associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not an array.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public TomlArray GetArray(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(key);

            if (value is not TomlArray arr)
                throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType(), typeof(TomlArray));

            return arr;
        }

        /// <summary>
        /// Returns the TOML table associated with the provided key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The TOML table associated with the key.</returns>
        /// <exception cref="TomlTypeMismatchException">If the value associated with this key is not a table.</exception>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public TomlTable GetSubTable(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(key);

            if (value is not TomlTable tbl)
                throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType(), typeof(TomlTable));

            return tbl;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, TomlValue>> GetEnumerator()
        {
            return Entries.GetEnumerator();
        }
    }
}