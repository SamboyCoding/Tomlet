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

        public void PutValue(string key, TomlValue value)
        {
            InternalPutValue(key, value, null, false);
        }

        private string DequoteKey(string key)
        {
            var wholeKeyIsQuoted = key.StartsWith("\"") && key.EndsWith("\"") || key.StartsWith("'") && key.EndsWith("'");
            return !wholeKeyIsQuoted ? key : key.Substring(1, key.Length - 2);
        }

        private void InternalPutValue(string key, TomlValue value, int? lineNumber, bool callParserForm)
        {
            key = key.Trim();

            var wholeKeyIsQuoted = key.StartsWith("\"") && key.EndsWith("\"") || key.StartsWith("'") && key.EndsWith("'");
            var firstPartOfKeyIsQuoted = !wholeKeyIsQuoted && (key.StartsWith("\"") || key.StartsWith("'"));
            if (key.Contains(".") && !wholeKeyIsQuoted)
            {
                //Unquoted dotted key means we put this in a sub-table.

                //First get the name of the key in *this* table.
                string ourKeyName;
                if (!firstPartOfKeyIsQuoted)
                {
                    var split = key.Split('.');
                    ourKeyName = split[0];
                }
                else
                {
                    ourKeyName = key;
                    var keyNameWithoutOpeningQuote = ourKeyName.Substring(1);
                    if (ourKeyName.Contains("\""))
                        ourKeyName = ourKeyName.Substring(0, 2 + keyNameWithoutOpeningQuote.IndexOf("\"", StringComparison.Ordinal));
                    else
                        ourKeyName = ourKeyName.Substring(0, 2 + keyNameWithoutOpeningQuote.IndexOf("'", StringComparison.Ordinal));
                }

                //And get the remainder of the key, relative to the sub-table.
                var restOfKey = key.Substring(ourKeyName.Length + 1);

                ourKeyName = ourKeyName.Trim();

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

            key = DequoteKey(key);

            //Non-dotted keys land here.
            Entries[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return Entries.ContainsKey(key);
        }

        /// <summary>
        /// Returns the raw instance of <see cref="TomlValue"/> associated with this key. You must cast to a sub-class and access its value
        /// yourself.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>An instance of <see cref="TomlValue"/> associated with this key.</returns>
        /// <exception cref="TomlNoSuchValueException">If the key is not present in the table.</exception>
        public TomlValue GetValue(string key)
        {
            //todo dotted keys
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
            var value = GetValue(key);

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
            var value = GetValue(key);

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
            var value = GetValue(key);

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
            var value = GetValue(key);

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
            var value = GetValue(key);

            if (!(value is TomlTable tbl))
                throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType());

            return tbl;
        }
    }
}