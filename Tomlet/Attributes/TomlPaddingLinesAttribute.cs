using System;

namespace Tomlet.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
public class TomlPaddingLinesAttribute: Attribute
{
    public int PaddingLines { get; }

    public TomlPaddingLinesAttribute(int paddingLines)
    {
        PaddingLines = paddingLines;
    }
}