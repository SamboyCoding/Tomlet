namespace Tomlet.Tests.TestModelClasses
{
    public record ComplexTestRecordWithAttributeMapping
    {
        [TomlProperty("string")]
        public string MyString { get; init; }
        public WidgetForThisComplexTestRecordWithAttributeMapping MyWidget { get; init; }
    }

    public record WidgetForThisComplexTestRecordWithAttributeMapping
    {
        [TomlProperty("my_int")]
        public int MyInt { get; init; }
    }
}
