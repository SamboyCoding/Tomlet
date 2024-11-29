using System;

namespace Tomlet.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TomlPrecedingCommentProviderAttribute : TomlCommentProviderAttribute
{
    public TomlPrecedingCommentProviderAttribute(Type provider) : base(provider, new object[] { })
    {
    }

    public TomlPrecedingCommentProviderAttribute(Type provider, object[] args) : base(provider, args)
    {
    }
}