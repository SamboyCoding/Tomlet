using Tomlet.Exceptions;
using Tomlet.Models;
using Xunit;

namespace Tomlet.Tests;

public class ExceptionTests
{
    private TomlDocument GetDocument(string resource) => new TomlParser().Parse(resource);

    [Fact]
    public void InvalidInlineTablesThrow() => 
        Assert.Throws<InvalidTomlInlineTableException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadInlineTableExample));

    [Fact]
    public void InvalidEscapesThrow() =>
        Assert.Throws<InvalidTomlEscapeException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadEscapeExample));
    
    [Fact]
    public void InvalidNumbersThrow() => 
        Assert.Throws<InvalidTomlNumberException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadNumberExample));
    
    [Fact]
    public void InvalidDatesThrow() =>
        Assert.Throws<InvalidTomlDateTimeException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadDateExample));
    
    [Fact]
    public void TruncatedFilesThrow() =>
        Assert.Throws<TomlEndOfFileException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTruncatedFileExample));

    [Fact]
    public void UndefinedTableArraysThrow() => 
        Assert.Throws<MissingIntermediateInTomlTableArraySpecException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTableArrayWithMissingIntermediateExample));
    
    [Fact]
    public void MissingKeysThrow() =>
        Assert.Throws<NoTomlKeyException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlMissingKeyExample));
    
    [Fact]
    public void TimesWithOffsetsButNoDateThrow() =>
        Assert.Throws<TimeOffsetOnTomlDateOrTimeException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlLocalTimeWithOffsetExample));
    
    [Fact]
    public void IncorrectlyFormattedArraysThrow() =>
        Assert.Throws<TomlArraySyntaxException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadArrayExample));
    
    [Fact]
    public void DateTimesWithNoSeparatorThrow() =>
        Assert.Throws<TomlDateTimeMissingSeparatorException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlDateTimeWithNoSeparatorExample));
    
    [Fact]
    public void DatesWithUnnecessarySeparatorThrow() =>
        Assert.Throws<TomlDateTimeUnnecessarySeparatorException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlUnnecessaryDateTimeSeparatorExample));
    
    [Fact]
    public void ImplyingAValueIsATableViaDottedKeyInADocumentWhenItIsNotThrows() =>
        Assert.Throws<TomlDottedKeyParserException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadDottedKeyExample));

    [Fact]
    public void ImplyingAValueIsATableViaDottedKeyWhenItIsNotThrows()
    {
        var doc = GetDocument(TestResources.ArrayOfEmptyStringTestInput);
        Assert.Throws<TomlDottedKeyException>(() => doc.Put("array.a", "foo"));
    }
    
    [Fact]
    public void BadEnumValueThrows() =>
        Assert.Throws<TomlEnumParseException>(() => TomletMain.To<TomlTestClassWithEnum>(DeliberatelyIncorrectTestResources.TomlBadEnumExample));

    [Fact]
    public void BadKeysThrow()
    {
        var doc = GetDocument("");
        
        //A key with both quotes
        Assert.Throws<InvalidTomlKeyException>(() => doc.GetLong("\"hello'"));
    }
}