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

}