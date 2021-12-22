using System;
using Tomlet.Exceptions;
using Tomlet.Models;
using Xunit;

namespace Tomlet.Tests;

public class ExceptionTests
{
    private TomlDocument GetDocument(string resource) => new TomlParser().Parse(resource);

    [Fact]
    public void InvalidInlineTablesThrow() => 
        AssertThrows<InvalidTomlInlineTableException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadInlineTableExample));

    [Fact]
    public void InvalidEscapesThrow() =>
        AssertThrows<InvalidTomlEscapeException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadEscapeExample));
    
    [Fact]
    public void InvalidNumbersThrow() => 
        AssertThrows<InvalidTomlNumberException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadNumberExample));
    
    [Fact]
    public void InvalidDatesThrow() =>
        AssertThrows<InvalidTomlDateTimeException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadDateExample));
    
    [Fact]
    public void TruncatedFilesThrow() =>
        AssertThrows<TomlEndOfFileException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTruncatedFileExample));

    [Fact]
    public void UndefinedTableArraysThrow() => 
        AssertThrows<MissingIntermediateInTomlTableArraySpecException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTableArrayWithMissingIntermediateExample));
    
    [Fact]
    public void MissingKeysThrow() =>
        AssertThrows<NoTomlKeyException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlMissingKeyExample));
    
    [Fact]
    public void TimesWithOffsetsButNoDateThrow() =>
        AssertThrows<TimeOffsetOnTomlDateOrTimeException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlLocalTimeWithOffsetExample));
    
    [Fact]
    public void IncorrectlyFormattedArraysThrow() =>
        AssertThrows<TomlArraySyntaxException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadArrayExample));
    
    [Fact]
    public void DateTimesWithNoSeparatorThrow() =>
        AssertThrows<TomlDateTimeMissingSeparatorException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlDateTimeWithNoSeparatorExample));
    
    [Fact]
    public void DatesWithUnnecessarySeparatorThrow() =>
        AssertThrows<TomlDateTimeUnnecessarySeparatorException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlUnnecessaryDateTimeSeparatorExample));
    
    [Fact]
    public void ImplyingAValueIsATableViaDottedKeyInADocumentWhenItIsNotThrows() =>
        AssertThrows<TomlDottedKeyParserException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadDottedKeyExample));

    [Fact]
    public void ImplyingAValueIsATableViaDottedKeyWhenItIsNotThrows()
    {
        var doc = GetDocument(TestResources.ArrayOfEmptyStringTestInput);
        AssertThrows<TomlDottedKeyException>(() => doc.Put("array.a", "foo"));
    }
    
    [Fact]
    public void BadEnumValueThrows() =>
        AssertThrows<TomlEnumParseException>(() => TomletMain.To<TomlTestClassWithEnum>(DeliberatelyIncorrectTestResources.TomlBadEnumExample));

    [Fact]
    public void BadKeysThrow()
    {
        var doc = GetDocument("");
        
        //A key with both quotes
        AssertThrows<InvalidTomlKeyException>(() => doc.GetLong("\"hello'"));
    }
    
    public void AssertThrows<T>(Action what) where T: Exception
    {
        Assert.Throws<T>(() =>
        {
            try
            {
                what();
            }
            catch (Exception e)
            {
                var _ = e.Message; //Call this for coverage idc
                throw;
            }
        });
    }

    public void AssertThrows<T>(Func<object> what) where T: Exception
    {
        Assert.Throws<T>(() =>
        {
            try
            {
                what();
            }
            catch (Exception e)
            {
                var _ = e.Message; //Call this for coverage idc
                throw;
            }
        });
    }
}