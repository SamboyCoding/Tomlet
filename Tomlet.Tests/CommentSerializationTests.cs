using Tomlet.Models;
using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests;

public class CommentSerializationTests
{
    [Fact]
    public void CommentsOnSimpleKeyValuePairsWork()
    {
        var doc = TomlDocument.CreateEmpty();
        var tomlString = TomletMain.ValueFrom("value");
        tomlString.Comments.InlineComment = "This is an inline comment";
        doc.PutValue("key", tomlString);

        var expected = @"key = ""value"" # This is an inline comment";
        Assert.Equal(expected, doc.SerializedValue.Trim());

        tomlString.Comments.PrecedingComment = "This is a multiline\nPreceding Comment";
        expected = "# This is a multiline\n# Preceding Comment\n" + expected;
        Assert.Equal(expected, doc.SerializedValue.Trim());

        tomlString.Comments.InlineComment = null;
        expected = "# This is a multiline\n# Preceding Comment\nkey = \"value\"";
        Assert.Equal(expected, doc.SerializedValue.Trim());
    }

    [Fact]
    public void CommentsOnTablesWork()
    {
        //Test comments being added to table headers.
        //To force the table to be serialized long form, we add a comment to an element in it
        var doc = TomlDocument.CreateEmpty();
        var table = new TomlTable
        {
            Comments =
            {
                PrecedingComment = "This is a multiline\nPreceding Comment",
                InlineComment = "This is an inline comment"
            }
        };

        var tomlString = TomletMain.ValueFrom("value");
        tomlString.Comments.InlineComment = "Inline comment on value";
        table.PutValue("key", tomlString);
        doc.PutValue("table", table);

        var expected = @"# This is a multiline
# Preceding Comment
[table] # This is an inline comment
key = ""value"" # Inline comment on value";

        Assert.Equal(expected, doc.SerializedValue.Trim());
    }

    [Fact]
    public void CommentsOnTableArraysWork()
    {
        //Test comments on table-array headers, and the logic around preceding comments on both the TA and first table

        var doc = TomlDocument.CreateEmpty();
        var table = new TomlTable
        {
            Comments =
            {
                PrecedingComment = "This is a preceding comment on the table",
                InlineComment = "This is an inline comment on the table"
            }
        };
        var tomlString = TomletMain.ValueFrom("value");
        tomlString.Comments.InlineComment = "Inline comment on value";
        table.PutValue("key", tomlString);

        var tableArray = new TomlArray {table};
        tableArray.Comments.PrecedingComment = "This is a preceding comment on the table-array itself";

        doc.PutValue("table-array", tableArray);

        var expected = @"
# This is a preceding comment on the table-array itself

# This is a preceding comment on the table
[[table-array]] # This is an inline comment on the table
key = ""value"" # Inline comment on value".Trim();

        Assert.Equal(expected, doc.SerializedValue.Trim());
    }

    [Fact]
    public void CommentsOnPrimitiveArraysWork()
    {
        var doc = TomlDocument.CreateEmpty();
        var tomlNumbers = new TomlArray {1, 2, 3};
        doc.PutValue("numbers", tomlNumbers);

        tomlNumbers[0].Comments.PrecedingComment = "This is a preceding comment on the first value of the array";
        tomlNumbers[1].Comments.InlineComment = "This is an inline comment on the second value of the array";

        var expected = @"numbers = [
    # This is a preceding comment on the first value of the array
    1,
    2, # This is an inline comment on the second value of the array
    3,
]";
        
        //Replace tabs with spaces because this source file uses spaces
        var actual = doc.SerializedValue.Trim().Replace("\t", "    ");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CommentAttributesWork()
    {
        var config = TomletMain.To<ExampleMailboxConfigClass>(TestResources.ExampleMailboxConfigurationTestInput);

        var doc = TomletMain.DocumentFrom(config);
        
        Assert.Equal("The name of the mailbox", doc.GetValue("mailbox").Comments.InlineComment);
        Assert.Equal("Your username for the mailbox", doc.GetValue("username").Comments.InlineComment);
        Assert.Equal("The password you use to access the mailbox", doc.GetValue("password").Comments.InlineComment);
        Assert.Equal("The rules for the mailbox follow", doc.GetArray("rules").Comments.PrecedingComment);
    }
}