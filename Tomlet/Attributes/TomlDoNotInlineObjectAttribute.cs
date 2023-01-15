using System;

namespace Tomlet.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
public class TomlDoNotInlineObjectAttribute : Attribute
{
    
}