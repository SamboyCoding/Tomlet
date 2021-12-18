using Tomlet.Exceptions;
using Tomlet.Models;
using Xunit;

namespace Tomlet.Tests
{
    public class TomlTableArrayTests
    {
        private TomlDocument GetDocument(string resource)
        {
            var parser = new TomlParser();
            return parser.Parse(resource);
        }

        [Fact]
        public void SimpleTableArraysAreSupported()
        {
            var document = GetDocument(TestResources.SimpleTableArrayTestInput);

            Assert.Single(document.Entries.Keys, keyName => keyName == "products");

            var products = Assert.IsType<TomlArray>(document.GetValue("products"));
            Assert.Equal(3, products.Count);

            var product1 = Assert.IsType<TomlTable>(products[0]);
            var product2 = Assert.IsType<TomlTable>(products[1]);
            var product3 = Assert.IsType<TomlTable>(products[2]);
            
            Assert.Equal("Hammer", product1.GetString("name"));
            Assert.Equal(738594937, product1.GetInteger("sku"));
            
            Assert.Empty(product2.Entries);
            
            Assert.Equal("Nail", product3.GetString("name"));
            Assert.Equal(284758393, product3.GetInteger("sku"));
            Assert.Equal("gray", product3.GetString("color"));
        }

        [Fact]
        public void ComplexTableArraysAreSupported()
        {
            var document = GetDocument(TestResources.ComplexTableArrayTestInput);

            Assert.Single(document.Entries);
            
            Assert.NotNull(document.GetArray("fruits"));
            Assert.Equal(2, document.GetArray("fruits").Count);

            //Apple
            var firstFruit = Assert.IsType<TomlTable>(document.GetArray("fruits")[0]);
            Assert.Equal("apple", firstFruit.GetString("name"));

            var physical = Assert.IsType<TomlTable>(firstFruit.GetValue("physical"));
            var jam = Assert.IsType<TomlTable>(firstFruit.GetValue("jam"));
            var varieties = Assert.IsType<TomlArray>(firstFruit.GetValue("varieties"));
            
            Assert.Equal("red", physical.GetString("color"));
            Assert.Equal("round", physical.GetString("shape"));

            Assert.Equal("yellow", jam.GetString("color"));
            Assert.Equal("sticky", jam.GetString("feel"));

            Assert.Equal(2, varieties.Count);
            Assert.Equal("red delicious", Assert.IsType<TomlTable>(varieties[0]).GetString("name"));
            Assert.Equal("granny smith", Assert.IsType<TomlTable>(varieties[1]).GetString("name"));
            
            //Banana
            var secondFruit = Assert.IsType<TomlTable>(document.GetArray("fruits")[1]);
            Assert.Equal("banana", secondFruit.GetString("name"));

            physical = Assert.IsType<TomlTable>(secondFruit.GetValue("physical"));
            var newtonian = Assert.IsType<TomlTable>(physical.GetValue("newtonian"));
            varieties = Assert.IsType<TomlArray>(secondFruit.GetValue("varieties"));

            Assert.Equal("yellow", physical.GetString("color"));
            Assert.Equal(118, newtonian.GetInteger("weight"));

            Assert.Single(varieties, val => Assert.IsType<TomlTable>(val).GetString("name") == "plantain");
        }

        [Fact]
        public void DefiningATableArrayWithTheSameNameAsATableThrowsAnException()
        {
            Assert.Throws<TomlTableArrayAlreadyExistsAsNonArrayException>(() => GetDocument(TestResources.DefiningAsArrayWhenAlreadyTableTestInput));
        }

        [Fact]
        public void ReDefiningAnArrayAsATableArrayThrowsAnException()
        {
            Assert.Throws<TomlNonTableArrayUsedAsTableArrayException>(() => GetDocument(TestResources.ReDefiningAnArrayAsATableArrayIsAnErrorTestInput));
        }

        [Fact]
        public void ReDefiningASubTableAsASubTableArrayThrowsAnException()
        {
            Assert.Throws<TomlKeyRedefinitionException>(() => GetDocument(TestResources.ReDefiningSubTableAsSubTableArrayTestInput));
        }

        [Fact]
        public void TableArraySerializationWorks()
        {
            //In order for table-array serialization to trigger, at least one of the tables has to be complicated (>= 5 entries or a nested one)
            var aComplexObject = new {
                name = "a",
                value = new {
                    a = "b",
                    c = "d"
                }
            };

            var array = new[] {aComplexObject, aComplexObject};
            
            var documentRoot = new {
                array
            };
            
            var tomlString = TomletMain.TomlStringFrom(documentRoot)
                .Replace("i__Field", "") //For my sanity
                .Trim();

            var expectedResult = @"
[[<array>]]
<name> = ""a""
<value> = { <a> = ""b"", <c> = ""d"" }

[[<array>]]
<name> = ""a""
<value> = { <a> = ""b"", <c> = ""d"" }
".Trim();
            
            Assert.Equal(expectedResult, tomlString);
        }
    }
}