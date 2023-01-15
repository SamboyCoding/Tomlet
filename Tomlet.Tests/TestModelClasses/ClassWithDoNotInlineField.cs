using System.Collections.Generic;
using Tomlet.Attributes;

namespace Tomlet.Tests.TestModelClasses;

public class ClassWithDoNotInlineField
{
    [TomlDoNotInlineObject] public Dictionary<string, string> ShouldNotBeInlined = new();
    
    public Dictionary<string, string> ShouldBeInlined = new();
}