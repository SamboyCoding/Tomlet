using Tomlet.Models;
using Xunit;

namespace Tomlet.Tests
{
    public class NumberTests
    {
        private TomlDocument GetDocument(string resource)
        {
            var parser = new TomlParser();
            return parser.Parse(resource);
        }
        
        [Fact]
        public void BasicIntegersFunctionAsIntended()
        {
            var document = GetDocument(TestResources.BasicIntegerTestInput);
            
            Assert.Equal(4, document.Entries.Count);

            //Check keys
            Assert.Collection(document.Entries.Keys,
                key1 => Assert.Equal("int1", key1),
                key2 => Assert.Equal("int2", key2),
                key3 => Assert.Equal("int3", key3),
                key4 => Assert.Equal("int4", key4)
            );

            //Check values
            Assert.Collection(document.Entries.Values,
                entry => Assert.Equal(99, Assert.IsType<TomlLong>(entry).Value),
                entry => Assert.Equal(42, Assert.IsType<TomlLong>(entry).Value),
                entry => Assert.Equal(0, Assert.IsType<TomlLong>(entry).Value),
                entry => Assert.Equal(-17, Assert.IsType<TomlLong>(entry).Value)
            );
        }
        
        [Fact]
        public void IntegersWithUnderscoresWorkAsIntended()
        {
            var document = GetDocument(TestResources.UnderscoresInIntegersTestInput);
            
            Assert.Equal(4, document.Entries.Count);

            //Check keys
            Assert.Collection(document.Entries.Keys,
                key1 => Assert.Equal("int5", key1),
                key2 => Assert.Equal("int6", key2),
                key3 => Assert.Equal("int7", key3),
                key4 => Assert.Equal("int8", key4)
            );

            //Check values
            Assert.Collection(document.Entries.Values,
                entry => Assert.Equal(1000, Assert.IsType<TomlLong>(entry).Value),
                entry => Assert.Equal(5349221, Assert.IsType<TomlLong>(entry).Value),
                entry => Assert.Equal(5349221, Assert.IsType<TomlLong>(entry).Value),
                entry => Assert.Equal(12345, Assert.IsType<TomlLong>(entry).Value)
            );
        }
    }
}