using Tomlet.Exceptions;
using Tomlet.Models;
using Xunit;

namespace Tomlet.Tests
{
    public class TableTests
    {
        private TomlDocument GetDocument(string resource)
        {
            var parser = new TomlParser();
            return parser.Parse(resource);
        }

        [Fact]
        public void TablesAreSupported()
        {
            var document = GetDocument(TestResources.BasicTableTestInput);
            
            Assert.Equal(2, document.Entries.Count);
            
            Assert.NotNull(document.GetSubTable("table-1"));
            Assert.NotNull(document.GetSubTable("table-2"));
            
            Assert.Equal(2, document.GetSubTable("table-1").Entries.Count);
            Assert.Equal(2, document.GetSubTable("table-2").Entries.Count);
            
            Assert.Equal("some string", document.GetSubTable("table-1").GetString("key1"));
            Assert.Equal(123, document.GetSubTable("table-1").GetInteger("key2"));
            
            Assert.Equal("another string", document.GetSubTable("table-2").GetString("key1"));
            Assert.Equal(456, document.GetSubTable("table-2").GetInteger("key2"));
        }

        [Fact]
        public void TablesCanHaveQuotedKeyNames()
        {
            //Ensure we have enough entries to make sure the table is not re-serialized inline
            var inputString = "[\"Table Name With Spaces\"]\nkey = \"value\"\nkey2 = 1\nkey3 = 2\nkey4 = 3\nkey5 = 4";
            var expectedOutput = "['Table Name With Spaces']\nkey = \"value\"\nkey2 = 1\nkey3 = 2\nkey4 = 3\nkey5 = 4";
            var document = GetDocument(inputString);
            
            Assert.Single(document.Keys, "Table Name With Spaces");
            Assert.Single(document.GetSubTable("Table Name With Spaces").Keys, "key");
            Assert.Equal("value", document.GetSubTable("Table Name With Spaces").GetString("key"));

            var tomlString = document.SerializedValue.Trim();
            Assert.Equal(expectedOutput, tomlString);
        }
        
         [Theory]
         [InlineData("normal-key", "normal-key")]
         [InlineData("normal_key", "normal_key")]
         [InlineData("normalkey", "normalkey")]
         [InlineData("\"key with spaces\"", "'key with spaces'")]
         [InlineData("key!with{}(*%&)random[other+symbols", "'key!with{}(*%&)random[other+symbols'")]
         [InlineData("key/with/slashes", "'key/with/slashes'")]
         [InlineData("Nam\\e", "'Nam\\e'")]
         public void KeysShouldBeSerializedCorrectly(string inputKey, string expectedKey)
         {
             var inputString = $"""
             ["Test Table"]
             {inputKey} = "value"
             """;
             
             var document = GetDocument(inputString);
             var serializedDocument = document.SerializedValue.Trim();
             Assert.Contains(expectedKey, serializedDocument);
         }

        [Fact]
        public void TablesCanHaveQuotedDottedNames()
        {
            var document = GetDocument(TestResources.TableWithQuotedDottedStringTestInput);
            
            Assert.Single(document.Entries);
            
            Assert.Equal("pug", document.GetSubTable("dog").GetSubTable("tater.man").GetSubTable("type").GetString("name"));
        }

        [Fact]
        public void TableIteratorEqualsEntries()
        {
            var document = GetDocument(TestResources.TableIteratorInput);

            Assert.Single(document.Entries);

            Assert.NotNull(document.GetSubTable("table-1"));

            var table1 = document.GetSubTable("table-1");

            Assert.Equal(4, table1.Entries.Count);

            foreach (var entry in table1)
            {
                Assert.True(table1.Entries.ContainsKey(entry.Key));
                Assert.Equal(table1.Entries[entry.Key], entry.Value);
            }
        }
    }
}