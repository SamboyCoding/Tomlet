using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Tomlet.Models;

public class TomlArray : TomlValue, IEnumerable<TomlValue>
{
    public readonly List<TomlValue> ArrayValues = new();
    internal bool IsLockedToBeTableArray;
    public override string StringValue => $"Toml Array ({ArrayValues.Count} values)";

#if MODERN_DOTNET
    public void Add<[DynamicallyAccessedMembers(TomlSerializationMethods.MainDeserializerAccessedMemberTypes)] T>(T t) where T: new()
#else
    public void Add<T>(T t) where T: new()
#endif
    {
        var tomlValue = t is TomlValue tv ? tv : TomletMain.ValueFrom(t);
        if(tomlValue != null)
            ArrayValues.Add(tomlValue);
    }

    public bool IsTableArray => IsLockedToBeTableArray || ArrayValues.All(t => t is TomlTable);

    public bool CanBeSerializedInline => !IsTableArray || //Simple array
                                         ArrayValues.All(o => o is TomlTable { ShouldBeSerializedInline: true }) && ArrayValues.Count <= 5; //Table array of inline tables, 5 or fewer of them.

    /// <summary>
    /// Returns true if this is not a table-array, there are not any sub-arrays or tables, and none of the entries contain comments.
    /// </summary>
    public bool IsSimpleArray => !IsLockedToBeTableArray && !ArrayValues.Any(o => o is TomlArray or TomlTable || !o.Comments.ThereAreNoComments);

    // ReSharper disable once UnusedMember.Global
    public TomlValue this[int index] => ArrayValues[index];

    public int Count => ArrayValues.Count;

    public override string SerializedValue => SerializeInline(!IsSimpleArray); //If non-simple, put newlines after commas.

    public string SerializeInline(bool multiline)
    {
        if (!CanBeSerializedInline)
            throw new Exception("Complex Toml Tables cannot be serialized into a TomlArray if the TomlArray is not a Table Array. " +
                                "This means that the TOML array cannot contain anything other than tables. If you are manually accessing SerializedValue on the TomlArray, you should probably be calling SerializeTableArray here. " +
                                "(Check the CanBeSerializedInline property and call that method if it is false)");

        var builder = new StringBuilder("[");

        var sep = multiline ? '\n' : ' ';
            
        foreach (var value in this)
        {
            builder.Append(sep);

            if (value.Comments.PrecedingComment != null)
            {
                builder.Append(value.Comments.FormatPrecedingComment(1))
                    .Append('\n');
            }
                
            if(multiline)
                builder.Append('\t');

            builder.Append(value.SerializedValue);

            builder.Append(',');

            if (value.Comments.InlineComment != null)
            {
                builder.Append(" # ")
                    .Append(value.Comments.InlineComment);
            }
        }

        builder.Append(sep);

        builder.Append(']');

        return builder.ToString();
    }

    public string SerializeTableArray(string key)
    {
        if (!IsTableArray)
            throw new Exception("Cannot serialize normal arrays using this method. Use the normal TomlValue.SerializedValue property.");

        var builder = new StringBuilder();

        if (Comments.InlineComment != null)
            throw new Exception("Sorry, but inline comments aren't supported on table-arrays themselves. See https://github.com/SamboyCoding/Tomlet/blob/master/Docs/InlineCommentsOnTableArrays.md for my rationale on this.");

        var first = true;
        foreach (var value in this)
        {
            if (value is not TomlTable table)
                throw new Exception($"Toml Table-Array contains non-table entry? Value is {value}");

            if (value.Comments.PrecedingComment != null)
            {
                if (first && Comments.PrecedingComment != null)
                    //if we have a preceding comment on the array itself, we add a blank line
                    //prior to the preceding comment on the first table.
                    builder.Append('\n');
                    
                builder.Append(value.Comments.FormatPrecedingComment())
                    .Append('\n');
            }

            first = false;

            //Write table-array header
            builder.Append("[[").Append(key).Append("]]");

            if (value.Comments.InlineComment != null)
                //Append inline comment on the table itself to the table-array header. 
                builder.Append(" # ").Append(value.Comments.InlineComment);

            //Blank line before the table body itself
            builder.Append('\n');

            //Write table body without header (as we just wrote that), then a blank line.
            builder.Append(table.SerializeNonInlineTable(key, false))
                .Append('\n');
        }

        return builder.ToString();
    }

    public IEnumerator<TomlValue> GetEnumerator()
    {
        return ArrayValues.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ArrayValues.GetEnumerator();
    }
}