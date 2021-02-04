using Tomlet.Models;
using Xunit;

namespace Tomlet.Tests
{
    public class DottedKeyTests
    {
        private TomlDocument GetDocument(string resource)
        {
            var parser = new TomlParser();
            return parser.Parse(resource);
        }

        [Fact]
        public void DottedKeysCreateTables()
        {
            var document = GetDocument(TestResources.SimpleDottedKeyTestInput);
            
            Assert.Equal(2, document.Entries.Count);
            
            Assert.Equal("Orange", document.GetString("name"));
            Assert.NotNull(document.GetSubTable("physical"));
            Assert.Equal(2, document.GetSubTable("physical").Entries.Count);
            
            Assert.Equal("orange", document.GetSubTable("physical").GetString("color"));
            Assert.Equal("round", document.GetSubTable("physical").GetString("shape"));
        }

        [Fact]
        public void DottedKeysCanHaveQuotedSubkeys()
        {
            var document = GetDocument(TestResources.DottedKeysCanHaveQuotedSubkeysTestInput);

            Assert.Single(document.Entries);
            
            Assert.NotNull(document.GetSubTable("site"));
            Assert.False(document.GetSubTable("site").GetBoolean("youtube.com"));
            
            Assert.NotNull(document.GetSubTable("site").GetSubTable("google.com"));
            
            Assert.True(document.GetSubTable("site").GetSubTable("google.com").GetBoolean("allowed"));
            Assert.Equal("Google", document.GetSubTable("site").GetSubTable("google.com").GetString("name"));
        }

        [Fact]
        public void WhitespaceInDottedKeysIsIgnored()
        {
            var document = GetDocument(TestResources.DottedKeyWhitespaceTestInput);

            Assert.Single(document.Entries);
            
            Assert.NotNull(document.GetSubTable("fruit"));
            
            Assert.Equal("banana", document.GetSubTable("fruit").GetString("name"));
            Assert.Equal("yellow", document.GetSubTable("fruit").GetString("color"));
            Assert.Equal("banana", document.GetSubTable("fruit").GetString("flavor"));
        }
    }
}