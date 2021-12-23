using Tomlet.Models;
using Xunit;

namespace Tomlet.Tests;

public class CommentDeserializationTests
{
    private TomlDocument GetDocument(string resource)
    {
        var parser = new TomlParser();
        return parser.Parse(resource);
    }
    
    [Fact]
    public void CommentsCanBeReadFromTheFileCorrectly()
    {
        var doc = GetDocument(TestResources.CommentTestInput);

        var firstValue = doc.GetValue("key");
        Assert.Equal("This is a full-line comment", firstValue.Comments.PrecedingComment);
        Assert.Equal("This is a comment at the end of a line", firstValue.Comments.InlineComment);
    }
}