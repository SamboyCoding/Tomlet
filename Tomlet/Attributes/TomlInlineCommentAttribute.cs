using System;

namespace Tomlet.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TomlInlineCommentAttribute : Attribute
{
    internal string Comment { get; }

    public TomlInlineCommentAttribute(string comment)
    {
        Comment = comment;
    }
}