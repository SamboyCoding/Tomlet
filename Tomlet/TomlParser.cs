using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Tomlet.Attributes;
using Tomlet.Exceptions;
using Tomlet.Extensions;
using Tomlet.Models;

namespace Tomlet;

public class TomlParser
{
    private static readonly char[] TrueChars = {'t', 'r', 'u', 'e'};
    private static readonly char[] FalseChars = {'f', 'a', 'l', 's', 'e'};

    private int _lineNumber = 1;

    private string[] _tableNames = new string[0];
    private TomlTable? _currentTable;

    // ReSharper disable once UnusedMember.Global
    [ExcludeFromCodeCoverage]
    public static TomlDocument ParseFile(string filePath)
    {
        var fileContent = File.ReadAllText(filePath);
        TomlParser parser = new();
        return parser.Parse(fileContent);
    }

    public TomlDocument Parse(string input)
    {
        try
        {
            var document = new TomlDocument();
            using var reader = new TomletStringReader(input);

            string? lastPrecedingComment = null;
            while (reader.TryPeek(out _))
            {
                //We have more to read.
                //By the time we get back to this position in the main loop, we've fully consumed any structure.
                //So that means we're at the start of a line - which could be a comment, table-array or table header, a key-value pair, or just whitespace.
                _lineNumber += reader.SkipAnyNewlineOrWhitespace();

                lastPrecedingComment = ReadAnyPotentialMultilineComment(reader);

                if (!reader.TryPeek(out var nextChar))
                    break;

                if (nextChar == '[')
                {
                    reader.Read(); //Consume the [

                    //Table or table-array?
                    if (!reader.TryPeek(out var potentialSecondBracket))
                        throw new TomlEndOfFileException(_lineNumber);

                    TomlValue valueFromSquareBracket;
                    if (potentialSecondBracket != '[')
                        valueFromSquareBracket = ReadTableStatement(reader, document);
                    else
                        valueFromSquareBracket = ReadTableArrayStatement(reader, document);

                    valueFromSquareBracket.Comments.PrecedingComment = lastPrecedingComment;

                    continue; //Restart loop.
                }

                //Read a key-value pair
                ReadKeyValuePair(reader, out var key, out var value);

                value.Comments.PrecedingComment = lastPrecedingComment;
                lastPrecedingComment = null;

                if (_currentTable != null)
                    //Insert into current table
                    _currentTable.ParserPutValue(key, value, _lineNumber);
                else
                    //Insert into the document
                    document.ParserPutValue(key, value, _lineNumber);

                //Read up until the end of the line, ignoring any comments or whitespace
                reader.SkipWhitespace();

                //Ensure we have a newline
                reader.SkipPotentialCarriageReturn();
                if (!reader.ExpectAndConsume('\n') && reader.TryPeek(out var shouldHaveBeenLf))
                    //Not EOF and found a non-newline char
                    throw new TomlMissingNewlineException(_lineNumber, (char) shouldHaveBeenLf);

                _lineNumber++; //We've consumed a newline, move to the next line number.
            }

            document.TrailingComment = lastPrecedingComment;

            return document;
        }
        catch (Exception e) when (e is not TomlException)
        {
            throw new TomlInternalException(_lineNumber, e);
        }
    }

    private void ReadKeyValuePair(TomletStringReader reader, out string key, out TomlValue value)
    {
        //Read the key
        key = ReadKey(reader);

        //Consume the equals sign, potentially with whitespace either side.
        reader.SkipWhitespace();
        if (!reader.ExpectAndConsume('='))
        {
            if (reader.TryPeek(out var shouldHaveBeenEquals))
                throw new TomlMissingEqualsException(_lineNumber, (char) shouldHaveBeenEquals);

            throw new TomlEndOfFileException(_lineNumber);
        }

        reader.SkipWhitespace();

        //Read the value
        value = ReadValue(reader);
    }

    private string ReadKey(TomletStringReader reader)
    {
        reader.SkipWhitespace();

        if (!reader.TryPeek(out var nextChar))
            return "";

        if (nextChar.IsEquals())
            throw new NoTomlKeyException(_lineNumber);

        //Read a key
        reader.SkipWhitespace();

        string key;
        if (nextChar.IsDoubleQuote())
        {
            //Read double-quoted key
            reader.Read();
            if (reader.TryPeek(out var maybeSecondDoubleQuote) && maybeSecondDoubleQuote.IsDoubleQuote())
            {
                reader.Read(); //Consume second double quote.

                //Check for third quote => invalid key
                //Else => empty key
                if (reader.TryPeek(out var maybeThirdDoubleQuote) && maybeThirdDoubleQuote.IsDoubleQuote())
                    throw new TomlTripleQuotedKeyException(_lineNumber);

                return string.Empty;
            }

            //We delegate to the dedicated string reading function here because a double-quoted key can contain everything a double-quoted string can. 
            key = '"' + ReadSingleLineBasicString(reader, false).StringValue + '"';

            if (!reader.ExpectAndConsume('"'))
                throw new UnterminatedTomlKeyException(_lineNumber);
        }
        else if (nextChar.IsSingleQuote())
        {
            reader.Read(); //Consume opening quote.

            //Read single-quoted key
            key = "'" + ReadSingleLineLiteralString(reader, false).StringValue + "'";
            if (!reader.ExpectAndConsume('\''))
                throw new UnterminatedTomlKeyException(_lineNumber);
        }
        else
            //Read unquoted key
            key = ReadKeyInternal(reader, keyChar => keyChar.IsEquals() || keyChar.IsHashSign());

        key = key.Replace("\\n", "\n")
            .Replace("\\t", "\t");

        return key;
    }

    private string ReadKeyInternal(TomletStringReader reader, Func<int, bool> charSignalsEndOfKey)
    {
        var parts = new List<string>();

        //Parts loop
        while (reader.TryPeek(out var nextChar))
        {
            if (charSignalsEndOfKey(nextChar))
                return string.Join(".", parts.ToArray());

            if (nextChar.IsPeriod())
                throw new TomlDoubleDottedKeyException(_lineNumber);

            var thisPart = new StringBuilder();
            //Part loop
            while (reader.TryPeek(out nextChar))
            {
                nextChar.EnsureLegalChar(_lineNumber);
                    
                var numLeadingWhitespace = reader.SkipWhitespace();
                reader.TryPeek(out var charAfterWhitespace);
                if (charAfterWhitespace.IsPeriod())
                {
                    //Whitespace is permitted in keys only around periods
                    parts.Add(thisPart.ToString()); //Add this part

                    //Consume period and any trailing whitespace
                    reader.ExpectAndConsume('.');
                    reader.SkipWhitespace();
                    break; //End of part, move to next
                }

                if (numLeadingWhitespace > 0 && charSignalsEndOfKey(charAfterWhitespace))
                {
                    //Add this part to the list of parts and break out of the loop, without consuming the char (it'll be picked up by the outer loop)
                    parts.Add(thisPart.ToString());
                    break;
                }

                //Un-skip the whitespace
                reader.Backtrack(numLeadingWhitespace);

                //NextChar is still the whitespace itself
                if (charSignalsEndOfKey(nextChar))
                {
                    //Add this part to the list of parts and break out of the loop, without consuming the char (it'll be picked up by the outer loop)
                    parts.Add(thisPart.ToString());
                    break;
                }

                if (numLeadingWhitespace > 0)
                    //Whitespace is not allowed outside of the area immediately around a period in a dotted key
                    throw new TomlWhitespaceInKeyException(_lineNumber);

                //Append this char to the part
                thisPart.Append((char) reader.Read());
            }
        }

        throw new TomlEndOfFileException(_lineNumber);
    }

    private TomlValue ReadValue(TomletStringReader reader)
    {
        if (!reader.TryPeek(out var startOfValue))
            throw new TomlEndOfFileException(_lineNumber);

        TomlValue value;
        switch (startOfValue)
        {
            case '[':
                //Array
                value = ReadArray(reader);
                break;
            case '{':
                //Inline table
                value = ReadInlineTable(reader);
                break;
            case '"':
            case '\'':
                //Basic or Literal String, maybe multiline
                var startQuote = reader.Read();
                var maybeSecondQuote = reader.Peek();
                if (maybeSecondQuote != startQuote)
                    //Second char is not first, this is a single-line string.
                    value = startQuote.IsSingleQuote() ? ReadSingleLineLiteralString(reader) : ReadSingleLineBasicString(reader);
                else
                {
                    reader.Read(); //Consume second char

                    //Check the third char. If it's another quote, we have a multiline string. If it's whitespace, a newline, part of an inline array, or a #, we have an empty string.
                    //Anything else is an error.
                    var maybeThirdQuote = reader.Peek();
                    if (maybeThirdQuote == startQuote)
                    {
                        reader.Read(); //Consume the third opening quote, for simplicity's sake.
                        value = startQuote.IsSingleQuote() ? ReadMultiLineLiteralString(reader) : ReadMultiLineBasicString(reader);
                    }
                    else if (maybeThirdQuote.IsWhitespace() || maybeThirdQuote.IsNewline() || maybeThirdQuote.IsHashSign() || maybeThirdQuote.IsComma() || maybeThirdQuote.IsEndOfArrayChar() || maybeThirdQuote == -1)
                    {
                        value = TomlString.Empty;
                    }
                    else
                    {
                        throw new TomlStringException(_lineNumber);
                    }
                }

                break;
            case '+':
            case '-':
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
            case 'i':
            case 'n':
                //I kind of hate that but it's probably fast.
                //Number. Maybe floating-point.
                //i and n indicate special floating point values (inf and nan).

                //Read a string, stopping if we hit an equals, whitespace, newline, or comment.
                var stringValue = reader.ReadWhile(valueChar => !valueChar.IsEquals() && !valueChar.IsNewline() && !valueChar.IsHashSign() && !valueChar.IsComma() && !valueChar.IsEndOfArrayChar() && !valueChar.IsEndOfInlineObjectChar())
                    .ToLowerInvariant().Trim();

                if (stringValue.Contains(':') || stringValue.Contains('t') || stringValue.Contains(' ') || stringValue.Contains('z'))
                    value = TomlDateTimeUtils.ParseDateString(stringValue, _lineNumber) ?? throw new InvalidTomlDateTimeException(_lineNumber, stringValue);
                else if (stringValue.Contains('.') || (stringValue.Contains('e') && !stringValue.StartsWith("0x")) || stringValue.Contains('n') || stringValue.Contains('i'))
                    //Try parse as a double, then fall back to a date/time.
                    value = TomlDouble.Parse(stringValue) ?? TomlDateTimeUtils.ParseDateString(stringValue, _lineNumber) ?? throw new InvalidTomlNumberException(_lineNumber, stringValue);
                else
                    //Try parse as a long, then fall back to a date/time.
                    value = TomlLong.Parse(stringValue) ?? TomlDateTimeUtils.ParseDateString(stringValue, _lineNumber) ?? throw new InvalidTomlNumberException(_lineNumber, stringValue);

                break;
            case 't':
            {
                //Either "true" or an error
                var charsRead = reader.ReadChars(4);

                if (!TrueChars.SequenceEqual(charsRead))
                    throw new TomlInvalidValueException(_lineNumber, (char) startOfValue);

                value = TomlBoolean.True;
                break;
            }
            case 'f':
            {
                //Either "false" or an error
                var charsRead = reader.ReadChars(5);

                if (!FalseChars.SequenceEqual(charsRead))
                    throw new TomlInvalidValueException(_lineNumber, (char) startOfValue);

                value = TomlBoolean.False;
                break;
            }
            default:
                throw new TomlInvalidValueException(_lineNumber, (char) startOfValue);
        }

        reader.SkipWhitespace();
        value.Comments.InlineComment = ReadAnyPotentialInlineComment(reader);

        return value;
    }

    private TomlValue ReadSingleLineBasicString(TomletStringReader reader, bool consumeClosingQuote = true)
    {
        //No simple read here, we have to accomodate escaped double quotes.
        var content = new StringBuilder();

        var escapeMode = false;
        var fourDigitUnicodeMode = false;
        var eightDigitUnicodeMode = false;

        var unicodeStringBuilder = new StringBuilder();
        while (reader.TryPeek(out var nextChar))
        {
            nextChar.EnsureLegalChar(_lineNumber);
            if (nextChar == '"' && !escapeMode)
                break;

            reader.Read(); //Consume the next char

            if (nextChar == '\\' && !escapeMode)
            {
                escapeMode = true;
                continue; //Don't append
            }

            if (escapeMode)
            {
                escapeMode = false;
                var toAppend = HandleEscapedChar(nextChar, out fourDigitUnicodeMode, out eightDigitUnicodeMode);

                if (toAppend.HasValue)
                    content.Append(toAppend.Value);
                continue;
            }

            if (fourDigitUnicodeMode || eightDigitUnicodeMode)
            {
                //Handle \u1234 and \U12345678
                unicodeStringBuilder.Append((char) nextChar);

                if (fourDigitUnicodeMode && unicodeStringBuilder.Length == 4 || eightDigitUnicodeMode && unicodeStringBuilder.Length == 8)
                {
                    var unicodeString = unicodeStringBuilder.ToString();

                    content.Append(DecipherUnicodeEscapeSequence(unicodeString, fourDigitUnicodeMode));

                    fourDigitUnicodeMode = false;
                    eightDigitUnicodeMode = false;
                    unicodeStringBuilder = new StringBuilder();
                }

                continue;
            }

            if (nextChar.IsNewline())
                throw new UnterminatedTomlStringException(_lineNumber);

            content.Append((char) nextChar);
        }

        if (consumeClosingQuote)
        {
            if (!reader.ExpectAndConsume('"'))
                throw new UnterminatedTomlStringException(_lineNumber);
        }

        return new TomlString(content.ToString());
    }

    private string DecipherUnicodeEscapeSequence(string unicodeString, bool fourDigitMode)
    {
        if (unicodeString.Any(c => !c.IsHexDigit()))
            throw new InvalidTomlEscapeException(_lineNumber, $"\\{(fourDigitMode ? 'u' : 'U')}{unicodeString}");

        if (fourDigitMode)
        {
            //16-bit char
            var decodedChar = short.Parse(unicodeString, NumberStyles.HexNumber);
            return ((char) decodedChar).ToString();
        }

        //32-bit char
        var decodedChars = int.Parse(unicodeString, NumberStyles.HexNumber);
        return char.ConvertFromUtf32(decodedChars);
    }

    private char? HandleEscapedChar(int escapedChar, out bool fourDigitUnicodeMode, out bool eightDigitUnicodeMode, bool allowNewline = false)
    {
        eightDigitUnicodeMode = false;
        fourDigitUnicodeMode = false;

        char toAppend;
        switch (escapedChar)
        {
            case 'b':
                toAppend = '\b';
                break;
            case 't':
                toAppend = '\t';
                break;
            case 'n':
                toAppend = '\n';
                break;
            case 'f':
                toAppend = '\f';
                break;
            case 'r':
                toAppend = '\r';
                break;
            case '"':
                toAppend = '"';
                break;
            case '\\':
                toAppend = '\\';
                break;
            case 'u':
                fourDigitUnicodeMode = true;
                return null;
            case 'U':
                eightDigitUnicodeMode = true;
                return null;
            default:
                if (allowNewline && escapedChar.IsNewline())
                    return null;
                throw new InvalidTomlEscapeException(_lineNumber, $"\\{(char) escapedChar}");
        }

        return toAppend;
    }

    private TomlValue ReadSingleLineLiteralString(TomletStringReader reader, bool consumeClosingQuote = true)
    {
        //Literally (hah) just read until a single-quote
        var stringContent = reader.ReadWhile(valueChar => !valueChar.IsSingleQuote() && !valueChar.IsNewline());
            
        foreach (var i in stringContent.Select(c => (int) c)) 
            i.EnsureLegalChar(_lineNumber);

        if (!reader.TryPeek(out var terminatingChar))
            //Unexpected EOF
            throw new TomlEndOfFileException(_lineNumber);

        if (!terminatingChar.IsSingleQuote())
            throw new UnterminatedTomlStringException(_lineNumber);

        if (consumeClosingQuote)
            reader.Read(); //Consume terminating quote.

        return new TomlString(stringContent);
    }

    private TomlValue ReadMultiLineLiteralString(TomletStringReader reader)
    {
        var content = new StringBuilder();
        //Ignore any first-line newlines
        _lineNumber += reader.SkipAnyNewline();
        while (reader.TryPeek(out _))
        {
            var nextChar = reader.Read();
            nextChar.EnsureLegalChar(_lineNumber);

            if (!nextChar.IsSingleQuote())
            {
                content.Append((char) nextChar);

                if (nextChar == '\n')
                    _lineNumber++; //We've wrapped to a new line.

                continue;
            }

            //We have a single quote.
            //Is it alone? if so, just continue.
            if (!reader.TryPeek(out var potentialSecondQuote) || !potentialSecondQuote.IsSingleQuote())
            {
                content.Append('\'');
                continue;
            }

            //We have two quotes in a row. Consume the second one
            reader.Read();

            //Do we have three?
            if (!reader.TryPeek(out var potentialThirdQuote) || !potentialThirdQuote.IsSingleQuote())
            {
                content.Append('\'');
                content.Append('\'');
                continue;
            }

            //Ok we have at least three quotes. Consume the third.
            reader.Read();

            if (!reader.TryPeek(out var afterThirdQuote) || !afterThirdQuote.IsSingleQuote())
                //And ONLY three quotes. End of literal.
                break;

            //We're at 4 single quotes back-to-back at this point, and the max is 5. I'm just going to do this without a loop because it's probably actually less code.
            //Consume the fourth.
            reader.Read();
            //And we have to append one single quote to our string.
            content.Append('\'');

            //Check for a 5th.
            if (!reader.TryPeek(out var potentialFifthQuote) || !potentialFifthQuote.IsSingleQuote())
                //Four in total, so we bail out here.
                break;

            //We have a 5th. Consume it.
            reader.Read();
            //And append to output
            content.Append('\'');

            //Check for sixth
            if (!reader.TryPeek(out var potentialSixthQuote) || !potentialSixthQuote.IsSingleQuote())
                //Five in total, so we bail out here.
                break;

            //We have a sixth. This is a syntax error.
            throw new TripleQuoteInTomlMultilineLiteralException(_lineNumber);
        }

        return new TomlString(content.ToString());
    }

    private TomlValue ReadMultiLineBasicString(TomletStringReader reader)
    {
        var content = new StringBuilder();

        var escapeMode = false;
        var fourDigitUnicodeMode = false;
        var eightDigitUnicodeMode = false;

        var unicodeStringBuilder = new StringBuilder();

        //Leading newlines are ignored
        _lineNumber += reader.SkipAnyNewline();

        while (reader.TryPeek(out _))
        {
            var nextChar = reader.Read();
            nextChar.EnsureLegalChar(_lineNumber);

            if (nextChar == '\\' && !escapeMode)
            {
                escapeMode = true;
                continue; //Don't append
            }

            if (escapeMode)
            {
                escapeMode = false;
                
                //If the escaped char is whitespace, and there's no other non-whitespace on this line, we skip all whitespace until the next non-whitespace char.
                if (nextChar.IsWhitespace() || nextChar.IsNewline())
                {
                    //Check that everything else on this line is whitespace
                    var backtrack = 0;
                    var skipWhitespace = false;
                    while (reader.TryPeek(out var nextCharOnLine))
                    {
                        backtrack++;
                        reader.Read();

                        if (nextCharOnLine.IsNewline())
                        {
                            skipWhitespace = true;
                            break;
                        }

                        if (!nextCharOnLine.IsWhitespace())
                        {
                            //Backslash-whitespace but then non-whitespace char before newline, this is invalid, we can break here and let HandleEscapedChar deal with it
                            break;
                        }
                    }
                    
                    //Return back to where we were
                    reader.Backtrack(backtrack);

                    if (skipWhitespace)
                    {
                        _lineNumber += reader.SkipAnyNewlineOrWhitespace();
                        continue; //The main while loop
                    }
                    
                    //Otherwise we drop down to HandleEscapedChar, which will throw due to the whitespace
                }
                
                var toAppend = HandleEscapedChar(nextChar, out fourDigitUnicodeMode, out eightDigitUnicodeMode, true);

                if (toAppend.HasValue)
                    content.Append(toAppend.Value);
                else if (nextChar.IsNewline())
                {
                    //Ensure we've fully consumed the newline
                    if (nextChar == '\r' && !reader.ExpectAndConsume('\n'))
                        throw new Exception($"Found a CR without an LF on line {_lineNumber}");

                    //Increment line number
                    _lineNumber++;

                    //Escaped newline indicates we skip this newline and any whitespace at the start of the next line
                    _lineNumber += reader.SkipAnyNewlineOrWhitespace();
                }

                continue;
            }

            if (fourDigitUnicodeMode || eightDigitUnicodeMode)
            {
                //Handle \u1234 and \U12345678
                unicodeStringBuilder.Append((char) nextChar);

                if (fourDigitUnicodeMode && unicodeStringBuilder.Length == 4 || eightDigitUnicodeMode && unicodeStringBuilder.Length == 8)
                {
                    var unicodeString = unicodeStringBuilder.ToString();

                    content.Append(DecipherUnicodeEscapeSequence(unicodeString, fourDigitUnicodeMode));

                    fourDigitUnicodeMode = false;
                    eightDigitUnicodeMode = false;
                    unicodeStringBuilder = new StringBuilder();
                }

                continue;
            }

            if (!nextChar.IsDoubleQuote())
            {
                if (nextChar == '\n')
                    _lineNumber++;

                content.Append((char) nextChar);
                continue;
            }

            //Like above, check for up to 6 quotes.

            //We have a double quote.
            //Is it alone? if so, just continue.
            if (!reader.TryPeek(out var potentialSecondQuote) || !potentialSecondQuote.IsDoubleQuote())
            {
                content.Append('"');
                continue;
            }

            //We have two quotes in a row. Consume the second one
            reader.Read();

            //Do we have three?
            if (!reader.TryPeek(out var potentialThirdQuote) || !potentialThirdQuote.IsDoubleQuote())
            {
                content.Append('"');
                content.Append('"');
                continue;
            }

            //Ok we have at least three quotes. Consume the third.
            reader.Read();

            if (!reader.TryPeek(out var afterThirdQuote) || !afterThirdQuote.IsDoubleQuote())
                //And ONLY three quotes. End of literal.
                break;

            //Like above, just going to bruteforce this out instead of writing a loop.
            //Consume the fourth.
            reader.Read();
            //And we have to append one double quote to our string.
            content.Append('"');

            //Check for a 5th.
            if (!reader.TryPeek(out var potentialFifthQuote) || !potentialFifthQuote.IsDoubleQuote())
                //Four in total, so we bail out here.
                break;

            //We have a 5th. Consume it.
            reader.Read();
            //And append to output
            content.Append('"');

            //Check for sixth
            if (!reader.TryPeek(out var potentialSixthQuote) || !potentialSixthQuote.IsDoubleQuote())
                //Five in total, so we bail out here.
                break;

            //We have a sixth. This is a syntax error.
            throw new TripleQuoteInTomlMultilineSimpleStringException(_lineNumber);
        }

        return new TomlString(content.ToString());
    }

    private TomlArray ReadArray(TomletStringReader reader)
    {
        //Consume the opening bracket
        if (!reader.ExpectAndConsume('['))
            throw new ArgumentException("Internal Tomlet Bug: ReadArray called and first char is not a [");

        //Move to the first value
        _lineNumber += reader.SkipAnyCommentNewlineWhitespaceEtc();

        var result = new TomlArray();

        while (reader.TryPeek(out _))
        {
            //Skip any empty lines
            _lineNumber += reader.SkipAnyCommentNewlineWhitespaceEtc();

            if (!reader.TryPeek(out var nextChar))
                throw new TomlEndOfFileException(_lineNumber);

            //Check for end of array here (helps with trailing commas, which are legal)
            if (nextChar.IsEndOfArrayChar())
                break;

            //Read a value
            result.ArrayValues.Add(ReadValue(reader));

            //Skip any whitespace or newlines, NOT comments - that would be a syntax error
            _lineNumber += reader.SkipAnyNewlineOrWhitespace();

            if (!reader.TryPeek(out var postValueChar))
                throw new TomlEndOfFileException(_lineNumber);

            if (postValueChar.IsEndOfArrayChar())
                break; //end of array

            if (!postValueChar.IsComma())
                throw new TomlArraySyntaxException(_lineNumber, (char) postValueChar);

            reader.ExpectAndConsume(','); //We've already verified we have one.
        }

        reader.ExpectAndConsume(']');

        return result;
    }

    private TomlTable ReadInlineTable(TomletStringReader reader)
    {
        //Consume the opening brace
        if (!reader.ExpectAndConsume('{'))
            throw new ArgumentException("Internal Tomlet Bug: ReadInlineTable called and first char is not a {");

        //Move to the first key
        _lineNumber += reader.SkipAnyCommentNewlineWhitespaceEtc();

        var result = new TomlTable {Defined = true};

        while (reader.TryPeek(out _))
        {
            //Skip any whitespace. Do not skip comments or newlines, those aren't allowed. 
            reader.SkipWhitespace();

            if (!reader.TryPeek(out var nextChar))
                throw new TomlEndOfFileException(_lineNumber);

            //Note that this is only needed when we first enter the loop, in case of an empty inline table
            if (nextChar.IsEndOfInlineObjectChar())
                break;

            //Newlines are not permitted
            if (nextChar.IsNewline())
                throw new NewLineInTomlInlineTableException(_lineNumber);

            //Note that unlike in the above case, we do not check for the end of the value here. Trailing commas aren't permitted
            //and so all cases where the table ends should be handled at the end of this look
            try
            {
                //Read a key-value pair
                ReadKeyValuePair(reader, out var key, out var value);
                //Insert into the table
                result.ParserPutValue(key, value, _lineNumber);
            }
            catch (TomlException ex) when (ex is TomlMissingEqualsException or NoTomlKeyException or TomlWhitespaceInKeyException)
            {
                //Wrap missing keys or equals signs in a parent exception.
                throw new InvalidTomlInlineTableException(_lineNumber, ex);
            }

            if (!reader.TryPeek(out var postValueChar))
                throw new TomlEndOfFileException(_lineNumber);

            if (reader.ExpectAndConsume(','))
                continue; //Comma, we have more.

            //Non-comma, consume any whitespace
            reader.SkipWhitespace();

            if (!reader.TryPeek(out postValueChar))
                throw new TomlEndOfFileException(_lineNumber);

            if (postValueChar.IsEndOfInlineObjectChar())
                break; //end of table

            throw new TomlInlineTableSeparatorException(_lineNumber, (char) postValueChar);
        }

        reader.ExpectAndConsume('}');

        result.Locked = true; //Defined inline, cannot be later modified
        return result;
    }

    private TomlTable ReadTableStatement(TomletStringReader reader, TomlDocument document)
    {
        //Table name
        var currentTableKey = reader.ReadWhile(c => !c.IsEndOfArrayChar() && !c.IsNewline());

        var parent = (TomlTable) document;
        var relativeKey = currentTableKey;
        FindParentAndRelativeKey(ref parent, ref relativeKey);

        TomlTable table;
        try
        {
            if (parent.ContainsKey(relativeKey))
            {
                try
                {
                    table = (TomlTable) parent.GetValue(relativeKey);

                    //The cast succeeded - we are defining an existing table
                    if (table.Defined)
                    {
                        // The table was not one created automatically
                        throw new TomlTableRedefinitionException(_lineNumber, currentTableKey);
                    }
                }
                catch (InvalidCastException)
                {
                    //The cast failed, we are re-defining a non-table.
                    throw new TomlKeyRedefinitionException(_lineNumber, currentTableKey);
                }
            }
            else
            {
                table = new TomlTable {Defined = true};
                parent.ParserPutValue(relativeKey, table, _lineNumber);
            }
        }
        catch (TomlContainsDottedKeyNonTableException e)
        {
            //Re-throw with correct line number and exception type.
            //To be clear - here we're re-defining a NON-TABLE key as a table, so this is a dotted key exception
            //while the one above is a TableRedefinition exception because it's re-defining a key which is already a table.
            throw new TomlDottedKeyParserException(_lineNumber, e.Key);
        }

        if (!reader.TryPeek(out _))
            throw new TomlEndOfFileException(_lineNumber);

        if (!reader.ExpectAndConsume(']'))
            throw new UnterminatedTomlTableNameException(_lineNumber);

        reader.SkipWhitespace();
        table.Comments.InlineComment = ReadAnyPotentialInlineComment(reader);
        reader.SkipPotentialCarriageReturn();

        if (!reader.TryPeek(out var shouldBeNewline))
            throw new TomlEndOfFileException(_lineNumber);

        if (!shouldBeNewline.IsNewline())
            throw new TomlMissingNewlineException(_lineNumber, (char) shouldBeNewline);

        _currentTable = table;

        //Save table names
        _tableNames = currentTableKey.Split('.');

        return table;
    }

    private TomlArray ReadTableArrayStatement(TomletStringReader reader, TomlDocument document)
    {
        //Consume the (second) opening bracket
        if (!reader.ExpectAndConsume('['))
            throw new ArgumentException("Internal Tomlet Bug: ReadTableArrayStatement called and first char is not a [");

        //Array
        var arrayName = reader.ReadWhile(c => !c.IsEndOfArrayChar() && !c.IsNewline());

        if (!reader.ExpectAndConsume(']') || !reader.ExpectAndConsume(']'))
            throw new UnterminatedTomlTableArrayException(_lineNumber);

        TomlTable parentTable = document;
        var relativeKey = arrayName;
        FindParentAndRelativeKey(ref parentTable, ref relativeKey);

        if (parentTable == document)
        {
            if (relativeKey.Contains('.'))
                throw new MissingIntermediateInTomlTableArraySpecException(_lineNumber, relativeKey);
        }

        //Find existing array or make new one
        TomlArray array;
        if (parentTable.ContainsKey(relativeKey))
        {
            var value = parentTable.GetValue(relativeKey);
            if (value is TomlArray arr)
                array = arr;
            else
                throw new TomlTableArrayAlreadyExistsAsNonArrayException(_lineNumber, arrayName);

            if (!array.IsLockedToBeTableArray)
            {
                throw new TomlNonTableArrayUsedAsTableArrayException(_lineNumber, arrayName);
            }
        }
        else
        {
            array = new TomlArray {IsLockedToBeTableArray = true};
            //Insert into parent table
            parentTable.ParserPutValue(relativeKey, array, _lineNumber);
        }

        // Create new table and add it to the array
        _currentTable = new TomlTable {Defined = true};
        array.ArrayValues.Add(_currentTable);

        //Save table names
        _tableNames = arrayName.Split('.');
            
        return array;
    }

    private void FindParentAndRelativeKey(ref TomlTable parent, ref string relativeName)
    {
        for (var index = 0; index < _tableNames.Length; index++)
        {
            var rootTableName = _tableNames[index];
            if (!relativeName.StartsWith(rootTableName + "."))
            {
                break;
            }

            var value = parent.GetValue(rootTableName);
            if (value is TomlTable subTable)
            {
                parent = subTable;
            }
            else if (value is TomlArray array)
            {
                parent = (TomlTable) array.Last();
            }
            else
            {
                // Note: Expects either TomlArray or TomlTable
                throw new TomlTypeMismatchException(typeof(TomlArray), value.GetType(), typeof(TomlArray));
            }

            relativeName = relativeName.Substring(rootTableName.Length + 1);
        }
    }

    private string? ReadAnyPotentialInlineComment(TomletStringReader reader)
    {
        if (!reader.ExpectAndConsume('#'))
            return null; //No comment
            
        var ret = reader.ReadWhile(c => !c.IsNewline()).Trim();

        if (ret.Length < 1) 
            return null;
            
        if(ret[0] == ' ')
            ret = ret.Substring(1);
            
        foreach (var i in ret.Select(c => (int) c)) 
            i.EnsureLegalChar(_lineNumber);

        return ret;

    }
        
    private string? ReadAnyPotentialMultilineComment(TomletStringReader reader)
    {
        var ret = new StringBuilder();
        while (reader.ExpectAndConsume('#'))
        {
            var line = reader.ReadWhile(c => !c.IsNewline());
                
            if(line.Length > 0 && line[0] == ' ')
                line = line.Substring(1);
                
            foreach (var i in line.Select(c => (int) c)) 
                i.EnsureLegalChar(_lineNumber);
                
            ret.Append(line);

            _lineNumber += reader.SkipAnyNewlineOrWhitespace();
        }

        if (ret.Length == 0)
            return null;

        return ret.ToString();
    }
}