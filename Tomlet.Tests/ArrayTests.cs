using System.Linq;
using Tomlet.Models;
using Xunit;

namespace Tomlet.Tests
{
    public class ArrayTests
    {
        private TomlDocument GetDocument(string resource)
        {
            var parser = new TomlParser();
            return parser.Parse(resource);
        }

        [Fact]
        public void PrimitiveArraysCanBeRead()
        {
            var document = GetDocument(TestResources.PrimitiveArraysTestInput);

            Assert.Equal(3, document.Entries.Count);

            var tomlArrays = document.Entries.Values.Select(Assert.IsType<TomlArray>).ToList();

            Assert.Equal(3, tomlArrays[0].Count);
            Assert.Equal(3, tomlArrays[1].Count);
            Assert.Equal(4, tomlArrays[2].Count);
        }

        [Fact]
        public void NestedArraysCanBeRead()
        {
            var document = GetDocument(TestResources.NestedArraysTestInput);

            Assert.Equal(2, document.Entries.Count);

            var tomlArrays = document.Entries.Values.Select(Assert.IsType<TomlArray>).ToList();

            //Check nested_arrays_of_ints contains two values
            Assert.Equal(2, tomlArrays[0].Count);

            //And that those values are also arrays and that there's 2 and 3 values within the nested arrays, respectively.
            Assert.Equal(new[] {2, 3}, tomlArrays[0].Select(Assert.IsType<TomlArray>).Select(arr => arr.ArrayValues.Count));

            //Check nested_mixed_array contains two values
            Assert.Equal(2, tomlArrays[1].Count);

            //And that those values are also arrays and that there's 2 and 3 values within the nested arrays, respectively.
            Assert.Equal(new[] {2, 3}, tomlArrays[0].Select(Assert.IsType<TomlArray>).Select(arr => arr.ArrayValues.Count));
        }

        [Fact]
        public void ArraysCanHaveTrailingCommas()
        {
            var document = GetDocument(TestResources.ArrayWithTrailingCommaTestInput);

            Assert.Single(document.Entries.Values,
                value => Assert.IsType<TomlArray>(value).Count == 2);
        }
    }
}