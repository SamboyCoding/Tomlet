using System;
using System.Collections.Generic;
using System.Linq;
using Tomlet.Exceptions;

namespace Tomlet.Models
{
    public class TomlTable : TomlValue
    {
        public Dictionary<string, TomlValue> Entries = new();

        internal bool Locked;

        public override string StringValue => $"Table ({Entries.Count} entries)";

        internal void ParserPutValue(string key, TomlValue value, int lineNumber)
        {
            if (Locked)
                throw new TomlTableLockedException(lineNumber, key);

            InternalPutValue(key, value, lineNumber, true);
        }

        public void PutValue(string key, TomlValue value, bool quote = false)
        {
            if (quote)
                key = QuoteKey(key);
            InternalPutValue(key, value, null, false);
        }

        private string DequoteKey(string key)
        {
            var wholeKeyIsQuoted = key.StartsWith("\"") && key.EndsWith("\"") || key.StartsWith("'") && key.EndsWith("'");
            return !wholeKeyIsQuoted ? key : key.Substring(1, key.Length - 2);
        }

        private static string QuoteKey(string key)
        {
            if (key.Contains("'") && key.Contains("\""))
                throw new InvalidTomlKeyException(key);

            if (key.Contains("\""))
                return $"'{key}'";

            return $"\"{key}\"";
        }

        private void InternalPutValue(string key, TomlValue value, int? lineNumber, bool callParserForm)
        {
            key = key.Trim();
            var (ourKeyName, restOfKey) = TomlKeyUtils.GetTopLevelAndSubKeys(key);

            if (!string.IsNullOrEmpty(restOfKey))
            {
                if (!Entries.TryGetValue(DequoteKey(ourKeyName), out var existingValue))
                {
                    //We don't have a sub-table with this name defined. That's fine, make one.
                    var subtable = new TomlTable();
                    if (callParserForm)
                        ParserPutValue(ourKeyName, subtable, lineNumber!.Value);
                    else
                        PutValue(ourKeyName, subtable);

                    //And tell it to handle the rest of the key.
                    if (callParserForm)
                        subtable.ParserPutValue(restOfKey, value, lineNumber!.Value);
                    else
                        subtable.PutValue(restOfKey, value);
                    return;
                }

                //We have a key by this name already. Is it a table?
                if (!(existingValue is TomlTable existingTable))
                {
                    //No - throw an exception
                    if (lineNumber.HasValue)
                        throw new TomlDottedKeyParserException(lineNumber.Value, ourKeyName);

                    throw new TomlDottedKeyException(ourKeyName);
                }

                //Yes, get the sub-table to handle the rest of the key
                if (callParserForm)
                    existingTable.ParserPutValue(restOfKey, value, lineNumber!.Value);
                else
                    existingTable.PutValue(restOfKey, value);
                return;
            }

            //Non-dotted keys land here.
            key = DequoteKey(key);

            if (Entries.ContainsKey(key) && lineNumber.HasValue)
                throw new TomlKeyRedefinitionException(lineNumber.Value, key);

            Entries[key] = value;
        }

        public bool ContainsKey(string key)
        {
            var (ourKeyName, restOfKey) = TomlKeyUtils.GetTopLevelAndSubKeys(key);

            if (string.IsNullOrEmpty(restOfKey))
                //Non-dotted key
                return Entries.ContainsKey(DequoteKey(key));

            if (!Entries.TryGetValue(ourKeyName, out var existingKey))
                return false;

            if (existingKey is TomlTable table)
                return table.ContainsKey(restOfKey);

            throw new TomlContainsDottedKeyNonTableException(key);
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
            if (!ContainsKey(key))
                throw new TomlNoSuchValueException(key);

            var (ourKeyName, restOfKey) = TomlKeyUtils.GetTopLevelAndSubKeys(key);

            if (string.IsNullOrEmpty(restOfKey))
                //Non-dotted key
                return Entries[DequoteKey(key)];

            if (!Entries.TryGetValue(ourKeyName, out var existingKey))
                throw new TomlNoSuchValueException(key); //Should already be handled by ContainsKey test

            if (existingKey is TomlTable table)
                return table.GetValue(restOfKey);

            throw new Exception("Tomlet Internal bug - existing key is not a table in TomlTable GetValue, but we didn't throw in ContainsKey?");
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
            var value = GetValue(QuoteKey(key));

            if (!(value is TomlString str))
                throw new TomlTypeMismatchException(typeof(TomlString), value.GetType());

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
            var value = GetValue(QuoteKey(key));

            if (!(value is TomlLong lng))
                throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType());

            return (int) lng.Value;
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
            var value = GetValue(QuoteKey(key));

            if (!(value is TomlBoolean b))
                throw new TomlTypeMismatchException(typeof(TomlBoolean), value.GetType());

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
            var value = GetValue(QuoteKey(key));

            if (!(value is TomlArray arr))
                throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType());

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
            var value = GetValue(QuoteKey(key));

            if (!(value is TomlTable tbl))
                throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType());

            return tbl;
        }
    }
}