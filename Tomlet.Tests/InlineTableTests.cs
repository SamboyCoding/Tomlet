using Tomlet.Exceptions;
using Tomlet.Models;
using Xunit;

namespace Tomlet.Tests
{
    public class InlineTableTests
    {
        private TomlDocument GetDocument(string resource)
        {
            var parser = new TomlParser();
            return parser.Parse(resource);
        }

        [Fact]
        public void InlineTablesAreSupported()
        {
            var document = GetDocument(TestResources.BasicInlineTableTestInput);
            
            Assert.Equal(3, document.Entries.Count);
            
            Assert.NotNull(document.GetSubTable("name"));
            Assert.NotNull(document.GetSubTable("point"));
            Assert.NotNull(document.GetSubTable("animal"));
        }

        [Fact]
        public void AttemptingToModifyInlineTablesThrowsAnException()
        {
            Assert.Throws<TomlTableLockedException>(() => GetDocument(TestResources.InlineTableLockedTestInput));
        }
    }
}