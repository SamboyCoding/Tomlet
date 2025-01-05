using System;
using System.Text;
using Tomlet.Exceptions;

namespace Tomlet.Models;

public class TomlCommentData
{
    public int PrecedingPaddingLines { get; set; } = 0;

    private string? _inlineComment;

    public string? PrecedingComment { get; set; }

    public string? InlineComment
    {
        get => _inlineComment;
        set
        {
            if (value == null)
            {
                _inlineComment = null;
                return;
            }

            if (value.Contains("\n") || value.Contains("\r"))
                throw new TomlNewlineInInlineCommentException();

            _inlineComment = value;
        }
    }

    public bool ThereAreNoComments => InlineComment == null && PrecedingComment == null;

    internal string FormatPrecedingComment(int indentCount = 0)
    {
        if (PrecedingComment == null)
            throw new Exception("Preceding comment is null");

        var builder = new StringBuilder();

        var lines = PrecedingComment.Split('\n');
        var first = true;
        foreach (var line in lines)
        {
            if (!first)
                builder.Append('\n');
            first = false;

            var correctIndent = new string('\t', indentCount);
            builder.Append(correctIndent).Append("# ").Append(line);
        }

        return builder.ToString();
    }
}