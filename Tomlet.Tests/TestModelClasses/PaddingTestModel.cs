using System.Collections.Generic;
using Tomlet.Attributes;

namespace Tomlet.Tests.TestModelClasses;

public class PaddingTestModel
{
    public string A { get; set; }

    public int B { get; set; }
    
    [TomlPaddingLines(1)]
    [TomlDoNotInlineObject]
    [TomlPrecedingComment("Nested Object")]
    public NestedModel C { get; set; }

    [TomlPaddingLines(1)]
    [TomlDoNotInlineObject]
    [TomlPrecedingComment("Nested Array")]
    public List<NestedModel> D { get; set; }
    
    public class NestedModel
    {
        [TomlPrecedingComment("Preceding Comment")]
        [TomlInlineComment("Preceding Comment")]
        public string E { get; set; }
        
        [TomlPrecedingComment("Preceding Comment")]
        [TomlInlineComment("Preceding Comment")]
        public int F { get; set; }
    }
}