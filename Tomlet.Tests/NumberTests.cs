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

        [Fact]
        public void BasicFloatValuesWorkAsIntended()
        {
            var document = GetDocument(TestResources.BasicFloatTestInput);
            
            Assert.Equal(7, document.Entries.Count);

            //Check keys
            Assert.Collection(document.Entries.Keys,
                key => Assert.Equal("flt1", key),
                key => Assert.Equal("flt2", key),
                key => Assert.Equal("flt3", key),
                key => Assert.Equal("flt4", key),
                key => Assert.Equal("flt5", key),
                key => Assert.Equal("flt6", key),
                key => Assert.Equal("flt7", key)
            );
            
            //Check values
            Assert.Collection(document.Entries.Values,
                entry => Assert.Equal(1.0, Assert.IsType<TomlDouble>(entry).Value),
                entry => Assert.Equal(3.1415, Assert.IsType<TomlDouble>(entry).Value),
                entry => Assert.Equal(-0.01, Assert.IsType<TomlDouble>(entry).Value),
                entry => Assert.Equal(5e+22, Assert.IsType<TomlDouble>(entry).Value),
                entry => Assert.Equal(1e06, Assert.IsType<TomlDouble>(entry).Value),
                entry => Assert.Equal(-2e-2, Assert.IsType<TomlDouble>(entry).Value),
                entry => Assert.Equal(6.626e-34, Assert.IsType<TomlDouble>(entry).Value)
            );
        }

        [Fact]
        public void FloatsWithUnderscoresWorkAsIntended()
        {
            var document = GetDocument(TestResources.FloatWithUnderscoresTestInput);

            Assert.Single(document.Entries, kvp => kvp.Key == "flt8" && kvp.Value is TomlDouble {Value: 224617.445991228});
        }

        [Fact]
        public void SpecialFloatConstantsAreAllowedAndReturnTheCorrectValues()
        {
            var document = GetDocument(TestResources.FloatSpecialsTestInput);
            
            Assert.Equal(6, document.Entries.Count);
            
            //Check keys
            Assert.Collection(document.Entries.Keys,
                key => Assert.Equal("sf1", key),
                key => Assert.Equal("sf2", key),
                key => Assert.Equal("sf3", key),
                key => Assert.Equal("sf4", key),
                key => Assert.Equal("sf5", key),
                key => Assert.Equal("sf6", key)
            );
            
            //Check values
            Assert.Collection(document.Entries.Values,
                entry => Assert.Equal(double.PositiveInfinity, Assert.IsType<TomlDouble>(entry).Value),
                entry => Assert.Equal(double.PositiveInfinity, Assert.IsType<TomlDouble>(entry).Value),
                entry => Assert.Equal(double.NegativeInfinity, Assert.IsType<TomlDouble>(entry).Value),
                entry => Assert.Equal(double.NaN, Assert.IsType<TomlDouble>(entry).Value),
                entry => Assert.Equal(double.NaN, Assert.IsType<TomlDouble>(entry).Value),
                entry => Assert.Equal(double.NaN, Assert.IsType<TomlDouble>(entry).Value)
            );
        }

        [Fact]
        public void HexadecimalNumbersWorkAsIntended()
        {
            var document = GetDocument(TestResources.HexadecimalTestInput);
            
            Assert.Equal(2, document.Entries.Count);

            //Test for pure hex-strings including an e which could be mistaken for an exponent
            Assert.Equal(0xdeadbeef, document.GetLong("key"));
            
            //Test for long hex strings that don't fit in an int, even unsigned
            Assert.Equal(0x1234567890FD, document.GetLong("key2"));
        }
    }
}