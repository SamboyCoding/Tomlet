using System;

namespace Tomlet.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TomlInlineCommentProviderAttribute : TomlCommentProviderAttribute
{
    public TomlInlineCommentProviderAttribute(Type provider) : base(provider, new object[] { })
    {
    }

    public TomlInlineCommentProviderAttribute(Type provider, object[] args) : base(provider, args)
    {
    }
}