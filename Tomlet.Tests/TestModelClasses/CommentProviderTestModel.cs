using Tomlet.Attributes;
using Tomlet.Tests.CommentProvider;

namespace Tomlet.Tests.TestModelClasses;

public class CommentProviderTestModel
{
    [TomlPrecedingCommentProvider(typeof(TestPrecedingCommentProvider), new object[] { "PrecedingComment" })]
    [TomlInlineComment("PlainInlineComment")]
    public string PrecedingComment { get; set; }
    
    [TomlInlineCommentProvider(typeof(TestInlineCommentProvider), new object[] { "InlineComment" })]
    [TomlPrecedingComment("PlainPrecedingComment")]
    public string InlineComment { get; set; }
}