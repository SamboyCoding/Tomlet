using System.Collections.Generic;
using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests
{
    public class IssueReproductionTests
    {
        [Fact]
        public void DictionaryKeyWithPeriodShouldSerializeAndDeserializeCorrectly()
        {
            var model = new ClassWithDictionary
            {
                GenericDictionary = new Dictionary<string, string>
                {
                    { "VsPerMonitorDpiAwarenessEnabled.35204", "some value" },
                    { "key.with.dots", "another value" },
                    { "Key3", "value 3" },
                    { "Key4", "value 4" }
                }
            };

            var toml = TomletMain.TomlStringFrom(model);
            // Keys with periods should be quoted in output to prevent being interpreted as dotted keys
            Assert.Contains("\"VsPerMonitorDpiAwarenessEnabled.35204\"", toml);
            Assert.Contains("\"key.with.dots\"", toml);

            // Round-trip: deserialize and verify data integrity
            var modelOut = TomletMain.To<ClassWithDictionary>(toml);
            Assert.True(modelOut.GenericDictionary.ContainsKey("VsPerMonitorDpiAwarenessEnabled.35204"));
            Assert.Equal("some value", modelOut.GenericDictionary["VsPerMonitorDpiAwarenessEnabled.35204"]);
            Assert.True(modelOut.GenericDictionary.ContainsKey("key.with.dots"));
            Assert.Equal("another value", modelOut.GenericDictionary["key.with.dots"]);
        }

        [Fact]
        public void DottedKeysAndDictionaryKeysWithPeriodsBothWork()
        {
            // This test verifies both features work together:
            // 1. Dotted keys in TOML create nested tables (parsing)
            // 2. Dictionary keys containing periods are properly quoted (serialization)

            var tomlInput = @"
[GenericDictionary]
""config.setting.name"" = ""value1""
normal_key = ""value2""
";
            var model = TomletMain.To<ClassWithDictionary>(tomlInput);

            Assert.Equal(2, model.GenericDictionary.Count);
            Assert.True(model.GenericDictionary.ContainsKey("config.setting.name"));
            Assert.Equal("value1", model.GenericDictionary["config.setting.name"]);
            Assert.Equal("value2", model.GenericDictionary["normal_key"]);

            // Round-trip should preserve the keys
            var modelOut = TomletMain.To<ClassWithDictionary>(TomletMain.TomlStringFrom(model));
            Assert.True(modelOut.GenericDictionary.ContainsKey("config.setting.name"));
            Assert.Equal("value1", modelOut.GenericDictionary["config.setting.name"]);
        }
    }
}
