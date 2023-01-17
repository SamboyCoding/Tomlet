using System.Collections.Generic;
using Tomlet.Attributes;

namespace Tomlet.Tests.TestModelClasses;

public class ClassWithDoNotInlineMembers
{
    [TomlDoNotInlineObject] public Dictionary<string, string> ShouldNotBeInlinedField = new();

    [TomlDoNotInlineObject] public Dictionary<string, string> ShouldNotBeInlinedProp { get; set; } = new();

    public Dictionary<string, string> ShouldBeInlined = new();
}