using System;

namespace Tomlet;

internal static class CommentProviderUtil
{
    public static string GetComment(Type provider)
    {
        var constructor = provider.GetConstructor(Type.EmptyTypes);
        if (constructor == null)
        {
            throw new ArgumentException("Provider must have a parameterless constructor");
        }

        var instance = (ICommentProvider)constructor.Invoke(null);
        return instance.GetComment();
    }
}