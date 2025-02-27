using System;
using System.Collections.Generic;
using Tomlet.Models;
using Tomlet.Tests.CommentProvider;
using Tomlet.Tests.TestModelClasses;
using Xunit;
using Xunit.Abstractions;

namespace Tomlet.Tests;

public class CommentSerializationTests
{
    public CommentSerializationTests(ITestOutputHelper testOutputHelper)
    {
    }

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

        Assert.Equal(expected.ReplaceLineEndings(), doc.SerializedValue.Trim().ReplaceLineEndings());
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

        var tableArray = new TomlArray { table };
        tableArray.Comments.PrecedingComment = "This is a preceding comment on the table-array itself";

        doc.PutValue("table-array", tableArray);

        var expected = @"
# This is a preceding comment on the table-array itself

# This is a preceding comment on the table
[[table-array]] # This is an inline comment on the table
key = ""value"" # Inline comment on value".Trim();

        Assert.Equal(expected.ReplaceLineEndings(), doc.SerializedValue.Trim().ReplaceLineEndings());
    }

    [Fact]
    public void CommentsOnPrimitiveArraysWork()
    {
        var doc = TomlDocument.CreateEmpty();
        var tomlNumbers = new TomlArray { 1, 2, 3 };
        doc.PutValue("numbers", tomlNumbers);

        tomlNumbers[0].Comments.PrecedingComment = "This is a preceding comment on the first value of the array";
        tomlNumbers[1].Comments.InlineComment = "This is an inline comment on the second value of the array";

        var expected = @"numbers = [
    # This is a preceding comment on the first value of the array
    1,
    2, # This is an inline comment on the second value of the array
    3,
]".ReplaceLineEndings();

        //Replace tabs with spaces because this source file uses spaces
        var actual = doc.SerializedValue.Trim().Replace("\t", "    ").ReplaceLineEndings();
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

    [Fact]
    public void CommentProviderTest()
    {
        TestPrecedingCommentProvider.Comments["PrecedingComment"] = Guid.NewGuid().ToString();
        TestInlineCommentProvider.Comments["InlineComment"] = Guid.NewGuid().ToString();

        var data = new CommentProviderTestModel()
        {
            PrecedingComment = "Dynamic Preceding Comment",
            InlineComment = "Inline Comment",
        };

        var doc = TomletMain.DocumentFrom(data);

        Assert.Equal(TestPrecedingCommentProvider.Comments["PrecedingComment"],
            doc.GetValue("PrecedingComment").Comments.PrecedingComment);
        Assert.Equal("PlainInlineComment", doc.GetValue("PrecedingComment").Comments.InlineComment);

        Assert.Equal(TestInlineCommentProvider.Comments["InlineComment"],
            doc.GetValue("InlineComment").Comments.InlineComment);
        Assert.Equal("PlainPrecedingComment", doc.GetValue("InlineComment").Comments.PrecedingComment);
    }
    
    [Fact]
    public void PaddingLinesTest()
    {
        var data = new PaddingTestModel()
        {
            A = "str a",
            B = 1,
            C = new PaddingTestModel.NestedModel()
            {
                E = "str",
                F = 2,
            },
            D = new List<PaddingTestModel.NestedModel>()
            {
                new()
                {
                    E = "str0",
                    F = 0,
                },
                new()
                {
                    E = "str1",
                    F = 1,
                },
            }
        };

        var expected = @"
A = ""str a""
B = 1

# Nested Object
[C]
# Preceding Comment
E = ""str"" # Preceding Comment
# Preceding Comment
F = 2 # Preceding Comment


# Nested Array
[[D]]
# Preceding Comment
E = ""str0"" # Preceding Comment
# Preceding Comment
F = 0 # Preceding Comment

[[D]]
# Preceding Comment
E = ""str1"" # Preceding Comment
# Preceding Comment
F = 1 # Preceding Comment
".Trim();
        Assert.Equal(expected.ReplaceLineEndings(), TomletMain.TomlStringFrom(data).ReplaceLineEndings().Trim());
    }
}