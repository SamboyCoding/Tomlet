namespace Tomlet.Exceptions;

public class TomlNewlineInInlineCommentException : TomlException
{
    public override string Message => "An attempt was made to set an inline comment which contains a newline. This obviously cannot be done, as inline comments must fit on one line.";
}