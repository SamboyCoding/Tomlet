using System.Collections.Generic;

namespace Tomlet.Tests.CommentProvider;

public class TestInlineCommentProvider : ICommentProvider
{
    public static Dictionary<string, string> Comments = new Dictionary<string, string>();

    private readonly string _name;

    public TestInlineCommentProvider(string name)
    {
        _name = name;
    }

    public string GetComment()
    {
        return Comments[_name];
    }
}