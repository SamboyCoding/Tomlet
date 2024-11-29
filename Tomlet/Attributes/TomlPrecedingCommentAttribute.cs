using System;

namespace Tomlet.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TomlPrecedingCommentAttribute : TomlPrecedingCommentProviderAttribute
{
    public TomlPrecedingCommentAttribute(string comment) : base(typeof(TomlSimpleCommentProvider),
        new object[] { comment })
    {
    }
}