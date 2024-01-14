using System.Collections.Generic;
using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests
{
    public class SerializeSpecialKeysTests
    {
        void AssertEqual(Dictionary<string, string> dictionary, Dictionary<string, string> other)
        {
            Assert.Equal(dictionary.Count, other.Count);
            foreach (var (key, value) in dictionary)
            {
                var otherValue = Assert.Contains(key, (IDictionary<string, string>)other);
                Assert.Equal(value, otherValue);
            }
        }


        [Fact]
        public void NoSpecialKeys()
        {
            var dict = new Dictionary<string, string>
            {
                { "SomeKey", "SomeValue" },
            };
            var tomlString = TomletMain.TomlStringFrom(dict);
            var otherDict = TomletMain.To<Dictionary<string, string>>(tomlString);
            AssertEqual(dict, otherDict);
        }

        [Fact]
        public void DottedKey()
        {
            var dict = new Dictionary<string, string>
            {
                { "Some.Key", "Some.Value" },
                { "Some.", "Some." },
                { ".Key", ".Value" },
                { ".", "." },
            };
            var tomlString = TomletMain.TomlStringFrom(dict);
            var otherDict = TomletMain.To<Dictionary<string, string>>(tomlString);
            AssertEqual(dict, otherDict);
        }

        [Fact]
        public void QuotedKey()
        {
            var dict = new Dictionary<string, string>
            {
                { "'SomeKey'", "'SomeValue'" },
                { "\"SomeKey\"", "\"SomeValue\"" },
                { "\"", "\"" },
                { "'", "'" },
            };
            var tomlString = TomletMain.TomlStringFrom(dict);
            var otherDict = TomletMain.To<Dictionary<string, string>>(tomlString);
            AssertEqual(dict, otherDict);
        }

        [Fact]
        public void QuotedDottedKey()
        {
            var dict = new Dictionary<string, string>
            {
                { "'Some.Key'", "'Some.Value'" },
                { "\"Some.Key\"", "\"Some.Value\"" },
                { "'Some'.Key", "'Some'.Value" },
                { "\"Some\".Key", "\"Some\".Value" },
                { "Some.'Key'", "Some.'Value'" },
                { "Some.\"Key\"", "Some.\"Value\"" },
            };
            var tomlString = TomletMain.TomlStringFrom(dict);
            var otherDict = TomletMain.To<Dictionary<string, string>>(tomlString);
            AssertEqual(dict, otherDict);
        }

        [Fact]
        public void Brackets()
        {
            var dict = new Dictionary<string, string>
            {
                { "[SomeKey]", "[SomeValue]" },
                { "[SomeKey\"", "[SomeValue\"" },
                { "[SomeKey", "[SomeValue" },
                { "SomeKey]", "SomeValue]" },
                { "[", "]" },
                { "]", "]" },
            };
            var tomlString = TomletMain.TomlStringFrom(dict);
            var otherDict = TomletMain.To<Dictionary<string, string>>(tomlString);
            AssertEqual(dict, otherDict);
        }

        [Fact]
        public void NoSpecialKeysWithClass()
        {
            var dict = new Dictionary<string, string>
            {
                { "SomeKey", "SomeValue" },
            };
            var tomlString = TomletMain.TomlStringFrom(
                new ClassWithDictionary
                {
                    GenericDictionary = dict,
                }
            );
            var otherClass = TomletMain.To<ClassWithDictionary>(tomlString);
            AssertEqual(dict, otherClass.GenericDictionary);
        }

        [Fact]
        public void DottedKeyWithClass()
        {
            var dict = new Dictionary<string, string>
            {
                { "Some.Key", "Some.Value" },
                { "Some.", "Some." },
                { ".Key", ".Value" },
                { ".", "." },
            };
            var tomlString = TomletMain.TomlStringFrom(
                new ClassWithDictionary
                {
                    GenericDictionary = dict,
                }
            );
            var otherClass = TomletMain.To<ClassWithDictionary>(tomlString);
            AssertEqual(dict, otherClass.GenericDictionary);
        }

        [Fact]
        public void QuotedKeyWithClass()
        {
            var dict = new Dictionary<string, string>
            {
                { "'SomeKey'", "'SomeValue'" },
                { "\"SomeKey\"", "\"SomeValue\"" },
                { "\"", "\"" },
                { "'", "'" },
            };
            var tomlString = TomletMain.TomlStringFrom(
                new ClassWithDictionary
                {
                    GenericDictionary = dict,
                }
            );
            var otherClass = TomletMain.To<ClassWithDictionary>(tomlString);
            AssertEqual(dict, otherClass.GenericDictionary);
        }

        [Fact]
        public void QuotedDottedKeyWithClass()
        {
            var dict = new Dictionary<string, string>
            {
                { "'Some.Key'", "'Some.Value'" },
                { "\"Some.Key\"", "\"Some.Value\"" },
                { "'Some'.Key", "'Some'.Value" },
                { "\"Some\".Key", "\"Some\".Value" },
                { "Some.'Key'", "Some.'Value'" },
                { "Some.\"Key\"", "Some.\"Value\"" },
            };
            var tomlString = TomletMain.TomlStringFrom(
                new ClassWithDictionary
                {
                    GenericDictionary = dict,
                }
            );
            var otherClass = TomletMain.To<ClassWithDictionary>(tomlString);
            AssertEqual(dict, otherClass.GenericDictionary);
        }

        [Fact]
        public void BracketsWithClass()
        {
            var dict = new Dictionary<string, string>
            {
                { "[SomeKey]", "[SomeValue]" },
                { "[SomeKey\"", "[SomeValue\"" },
                { "[SomeKey", "[SomeValue" },
                { "SomeKey]", "SomeValue]" },
                { "[", "]" },
                { "]", "]" },
            };
            var tomlString = TomletMain.TomlStringFrom(
                new ClassWithDictionary
                {
                    GenericDictionary = dict,
                }
            );
            var otherClass = TomletMain.To<ClassWithDictionary>(tomlString);
            AssertEqual(dict, otherClass.GenericDictionary);
        }
    }
}