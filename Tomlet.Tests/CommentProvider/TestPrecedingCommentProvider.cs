using System.Collections.Generic;

namespace Tomlet.Tests.CommentProvider;

public class TestPrecedingCommentProvider : ICommentProvider
{
    public static Dictionary<string, string> Comments = new Dictionary<string, string>();

    private readonly string _name;

    public TestPrecedingCommentProvider(string name)
    {
        _name = name;
    }

    public string GetComment()
    {
        return Comments[_name];
    }
}