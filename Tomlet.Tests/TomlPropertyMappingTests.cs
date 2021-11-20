using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests
{
    public class TomlPropertyMappingTests
    {
        [Fact]
        public void DeserializationWorks()
        {
            var record = TomletMain.To<ComplexTestRecordWithAttributeMapping>(TestResources.ComplexTestRecordForAttributeMapping);
            Assert.Equal("Test", record.MyString);
            Assert.IsType<WidgetForThisComplexTestRecordWithAttributeMapping>(record.MyWidget);
            Assert.Equal(42, record.MyWidget.MyInt);
        }

        [Fact]
        public void SerializationWorks()
        {
            var testString = TestResources.ComplexTestRecordForAttributeMapping.Trim();
            var record = TomletMain.To<ComplexTestRecordWithAttributeMapping>(testString);
            var serialized = TomletMain.TomlStringFrom(record).Trim();
            Assert.Equal(testString, serialized);
        }
    }
}