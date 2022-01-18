using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Tomlet.Exceptions;

namespace Tomlet.Models
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TomlTable : TomlValue
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

                builder.Append(string.Join(", ", Entries.Select(o => o.Key + " = " + o.Value.SerializedValue).ToArray()));

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
                if(Comments.InlineComment != null)
                    builder.Append(" # ").Append(Comments.InlineComment);

                builder.Append('\n');
            }

            //Three passes: Simple key-value pairs including inline arrays and tables, sub-tables, then sub-table-arrays.
            foreach (var (subKey, value) in Entries)
            {
                if (value is TomlTable {ShouldBeSerializedInline: false} or TomlArray {CanBeSerializedInline: false})
                    continue;

                WriteValueToStringBuilder(keyName, subKey, builder);
            }

            foreach (var (subKey, value) in Entries)
            {
                if (value is not TomlTable {ShouldBeSerializedInline: false})
                    continue;

                WriteValueToStringBuilder(keyName, subKey, builder);
            }

            foreach (var (subKey, value) in Entries)
            {
                if (value is not TomlArray {CanBeSerializedInline: false})
                    continue;

                WriteValueToStringBuilder(keyName, subKey, builder);
            }

            return builder.ToString();
        }

        private void WriteValueToStringBuilder(string? keyName, string subKey, StringBuilder builder)
        {
            var value = GetValue(subKey);

            subKey = EscapeKeyIfNeeded(subKey);

            if(keyName != null)
                keyName = EscapeKeyIfNeeded(keyName);

            var fullSubKey = keyName == null ? subKey : $"{keyName}.{subKey}";  
            
            //Handle any preceding comment - this will ALWAYS go before any sort of value
            if (value.Comments.PrecedingComment != null)
                builder.Append(value.Comments.FormatPrecedingComment())
                    .Append('\n');

            switch (value)
            {
                case TomlArray {CanBeSerializedInline: false} subArray:
                    builder.Append(subArray.SerializeTableArray(fullSubKey)); //No need to append newline as SerializeTableArray always ensure it ends with 2
                    return; //Return because we don't do newline or handle inline comment here.
                case TomlArray subArray:
                    builder.Append(subKey).Append(" = ").Append(subArray.SerializedValue);
                    break;
                case TomlTable {ShouldBeSerializedInline: true} subTable:
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

        private string EscapeKeyIfNeeded(string key)
        {
            var didQuote = false;

            if (key.StartsWith("\"") && key.EndsWith("\"") && key.Count(c => c == '"') == 2)
                //Already double quoted
                return key;
            
            if (key.StartsWith("'") && key.EndsWith("'") && key.Count(c => c == '\'') == 2)
                //Already single quoted
                return key;
            
            if (key.Contains("\"") || key.Contains("'"))
            {
                key = TomlUtils.AddCorrectQuotes(key);
                didQuote = true;
            }

            var escaped = TomlUtils.EscapeStringValue(key);

            if (escaped.Contains(" ") || escaped.Contains("\\") && !didQuote)
                escaped = TomlUtils.AddCorrectQuotes(escaped);

            return escaped;
        }

        internal void ParserPutValue(string key, TomlValue value, int lineNumber)
        {
            if (Locked)
                throw new TomlTableLockedException(lineNumber, key);

            InternalPutValue(key, value, lineNumber, true);
        }

        public void PutValue(string key, TomlValue value, bool quote = false)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            if(value == null)
                throw new ArgumentNullException("value");

            if (quote)
                key = TomlUtils.AddCorrectQuotes(key);
            InternalPutValue(key, value, null, false);
        }

        public void Put<T>(string key, T t, bool quote = false)
        {
            if(t is TomlValue tv)
                PutValue(key, tv, quote);
            else
                PutValue(key, TomletMain.ValueFrom(t), quote);
        }

        public string DeQuoteKey(string key)
        {
            var wholeKeyIsQuoted = key.StartsWith("\"") && key.EndsWith("\"") || key.StartsWith("'") && key.EndsWith("'");
            return !wholeKeyIsQuoted ? key : key.Substring(1, key.Length - 2);
        }

        private void InternalPutValue(string key, TomlValue value, int? lineNumber, bool callParserForm)
        {
            key = key.Trim();
            TomlKeyUtils.GetTopLevelAndSubKeys(key, out var ourKeyName, out var restOfKey);

            if (!string.IsNullOrEmpty(restOfKey))
            {
                if (!Entries.TryGetValue(DeQuoteKey(ourKeyName), out var existingValue))
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
                if (existingValue is not TomlTable existingTable)
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
            key = DeQuoteKey(key);

            if (Entries.ContainsKey(key) && lineNumber.HasValue)
                throw new TomlKeyRedefinitionException(lineNumber.Value, key);

            Entries[key] = value;
        }

        public bool ContainsKey(string key)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            TomlKeyUtils.GetTopLevelAndSubKeys(key, out var ourKeyName, out var restOfKey);

            if (string.IsNullOrEmpty(restOfKey))
                //Non-dotted key
                return Entries.ContainsKey(DeQuoteKey(key));

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
            if(key == null)
                throw new ArgumentNullException("key");

            if (!ContainsKey(key))
                throw new TomlNoSuchValueException(key);

            TomlKeyUtils.GetTopLevelAndSubKeys(key, out var ourKeyName, out var restOfKey);

            if (string.IsNullOrEmpty(restOfKey))
                //Non-dotted key
                return Entries[DeQuoteKey(key)];

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
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(TomlUtils.AddCorrectQuotes(key));

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
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(TomlUtils.AddCorrectQuotes(key));

            if (value is not TomlLong lng)
                throw new TomlTypeMismatchException(typeof(TomlLong), value.GetType(), typeof(int));

            return (int) lng.Value;
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
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(TomlUtils.AddCorrectQuotes(key));

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
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(TomlUtils.AddCorrectQuotes(key));

            if (value is not TomlDouble dbl)
                throw new TomlTypeMismatchException(typeof(TomlDouble), value.GetType(), typeof(float));

            return (float) dbl.Value;
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
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(TomlUtils.AddCorrectQuotes(key));

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
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(TomlUtils.AddCorrectQuotes(key));

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
            if(key == null)
                throw new ArgumentNullException("key");

            var value = GetValue(TomlUtils.AddCorrectQuotes(key));

            if (value is not TomlTable tbl)
                throw new TomlTypeMismatchException(typeof(TomlTable), value.GetType(), typeof(TomlTable));

            return tbl;
        }
    }
}