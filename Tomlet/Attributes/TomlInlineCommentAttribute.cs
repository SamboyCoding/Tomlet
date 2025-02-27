using System;

namespace Tomlet.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TomlInlineCommentAttribute : TomlInlineCommentProviderAttribute
{
    public TomlInlineCommentAttribute(string comment) : base(typeof(TomlSimpleCommentProvider),
        new object[] { comment })
    {
    }
}