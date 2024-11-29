namespace Tomlet;

internal class TomlSimpleCommentProvider : ICommentProvider
{
    private readonly string _comment;

    public TomlSimpleCommentProvider(string comment)
    {
        _comment = comment;
    }

    public string GetComment()
    {
        return _comment;
    }
}