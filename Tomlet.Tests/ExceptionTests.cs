using System;
using System.Collections.Generic;
using Tomlet.Exceptions;
using Tomlet.Models;
using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests;

public class ExceptionTests
{
    private TomlDocument GetDocument(string resource) => new TomlParser().Parse(resource);
    
    private static void AssertThrows<T>(Action what) where T: Exception
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

    private static void AssertThrows<T>(Func<object> what) where T: Exception
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
        AssertThrows<TomlKeyRedefinitionException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadDottedKeyExample));
    
    [Fact]
    public void BadEnumValueThrows() =>
        AssertThrows<TomlEnumParseException>(() => TomletMain.To<TomlTestClassWithEnum>(DeliberatelyIncorrectTestResources.TomlBadEnumExample));

    [Fact]
    public void ReDefiningASubTableAsASubTableArrayThrowsAnException() => 
        AssertThrows<TomlTableRedefinitionException>(() => GetDocument(DeliberatelyIncorrectTestResources.ReDefiningSubTableAsSubTableArrayTestInput));

    [Fact]
    public void RedefiningAKeyAsATableNameThrowsAnException() => 
        AssertThrows<TomlKeyRedefinitionException>(() => GetDocument(DeliberatelyIncorrectTestResources.KeyRedefinitionViaTableTestInput));
    
    [Fact]
    public void DefiningATableArrayWithTheSameNameAsATableThrowsAnException() => 
        AssertThrows<TomlTableArrayAlreadyExistsAsNonArrayException>(() => GetDocument(DeliberatelyIncorrectTestResources.DefiningAsArrayWhenAlreadyTableTestInput));

    [Fact]
    public void ReDefiningAnArrayAsATableArrayThrowsAnException() => 
        AssertThrows<TomlNonTableArrayUsedAsTableArrayException>(() => GetDocument(DeliberatelyIncorrectTestResources.ReDefiningAnArrayAsATableArrayIsAnErrorTestInput));
    
    [Fact]
    public void InlineTablesWithNewlinesThrowAnException() => 
        AssertThrows<NewLineInTomlInlineTableException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlInlineTableWithNewlineExample));
    
    [Fact]
    public void DoubleDottedKeysThrowAnException() => 
        AssertThrows<TomlDoubleDottedKeyException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlDoubleDottedKeyExample));
    
    [Fact]
    public void MissingTheCommaInAnInlineTableThrows() => 
        AssertThrows<TomlInlineTableSeparatorException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlInlineTableWithMissingSeparatorExample));

    [Fact]
    public void ConvertingAPrimitiveToADocumentThrows() =>
        AssertThrows<TomlPrimitiveToDocumentException>(() => TomletMain.DocumentFrom("hello"));
    
    [Fact]
    public void BadTomlStringThrows() =>
        AssertThrows<TomlStringException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlBadStringExample));

    [Fact]
    public void TripleQuotedKeysThrow() => 
        AssertThrows<TomlTripleQuotedKeyException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTripleQuotedKeyExample));
    
    [Fact]
    public void WhitespaceInKeyThrows() => 
        AssertThrows<TomlMissingEqualsException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlWhitespaceInKeyExample));
    
    [Fact]
    public void MissingEqualsSignThrows() => 
        AssertThrows<TomlMissingEqualsException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlMissingEqualsExample));
    
    [Fact]
    public void TripleSingleQuoteInStringThrows() => 
        AssertThrows<TripleQuoteInTomlMultilineLiteralException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTripleSingleQuoteInStringExample));
    
    [Fact]
    public void TripleDoubleQuoteInStringThrows() => 
        AssertThrows<TripleQuoteInTomlMultilineSimpleStringException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlTripleDoubleQuoteInStringExample));
    
    [Fact]
    public void UnterminatedKeyThrows() => 
        AssertThrows<UnterminatedTomlKeyException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlUnterminatedQuotedKeyExample));
    
    [Fact]
    public void UnterminatedStringThrows() =>
        AssertThrows<UnterminatedTomlStringException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlUnterminatedStringExample));
    
    [Fact]
    public void UnterminatedTableArrayThrows() => 
        AssertThrows<UnterminatedTomlTableArrayException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlUnterminatedTableArrayExample));
    
    [Fact]
    public void UnterminatedTableNameThrows() => 
        AssertThrows<UnterminatedTomlTableNameException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlUnterminatedTableExample));
    
    [Fact]
    public void AttemptingToModifyInlineTablesThrowsAnException() => 
        AssertThrows<TomlTableLockedException>(() => GetDocument(DeliberatelyIncorrectTestResources.InlineTableLockedTestInput));
    
    [Fact]
    public void ReDefiningATableThrowsAnException() => 
        AssertThrows<TomlTableRedefinitionException>(() => GetDocument(DeliberatelyIncorrectTestResources.TableRedefinitionTestInput));
    
    [Fact]
    public void UnicodeControlCharsThrowAnException() => 
        AssertThrows<TomlUnescapedUnicodeControlCharException>(() => GetDocument(DeliberatelyIncorrectTestResources.TomlNullBytesExample));

    //These are all runtime mistakes on otherwise-valid TOML documents, so they aren't in the DeliberatelyIncorrectTestResources file.
    
    [Fact]
    public void UnInstantiableObjectsThrow() => 
        AssertThrows<TomlInstantiationException>(() => TomletMain.To<IConvertible>(""));

    [Fact]
    public void MultipleParameterizedConstructorsThrow() =>
        AssertThrows<TomlInstantiationException>(() => TomletMain.To<ClassWithMultipleParameterizedConstructors>(""));
    
    [Fact]
    public void AbstractClassDeserializationThrows() =>
        AssertThrows<TomlInstantiationException>(() => TomletMain.To<AbstractClass>(""));
    
    [Fact]
    public void MismatchingTypesInPrimitiveMappingThrows() => 
        AssertThrows<TomlTypeMismatchException>(() => TomletMain.To<float>(GetDocument("MyFloat = \"hello\"").GetValue("MyFloat")));

    [Fact]
    public void GettingAValueWhichDoesntExistThrows() =>
        AssertThrows<TomlNoSuchValueException>(() => GetDocument("MyString = \"hello\"").GetValue("MyFloat"));
    
    [Fact]
    public void MismatchingTypesInDeserializationThrow() => 
        AssertThrows<TomlPropertyTypeMismatchException>(() => TomletMain.To<SimplePropertyTestClass>("MyFloat = \"hello\""));

    [Fact]
    public void SettingAnInlineCommentToIncludeANewlineThrows() => 
        AssertThrows<TomlNewlineInInlineCommentException>(() => TomlDocument.CreateEmpty().Comments.InlineComment = "hello\nworld");

    [Fact]
    public void RedefiningDottedKeyThrows() => AssertThrows<TomlDottedKeyParserException>(
        () =>
        {
            var parser = new TomlParser();
            var tomlDocument = parser.Parse("""
            a.b = 2
            a.b.c = 3                       
            """);
        }
    );
}