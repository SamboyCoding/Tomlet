using Tomlet.Attributes;

namespace Tomlet.Tests.TestModelClasses;

[TomlDoNotInlineObject]
public class TomlClassWithNestedArray
{
    public ClassWithArray Root;
    
    [TomlDoNotInlineObject]
    public class ClassWithArray
    {
        public string SomeValue;

        public ArrayItem[] Array;
        
        [TomlDoNotInlineObject]
        public class ArrayItem
        {
            public string A;
            public string B;
        }
    }
}