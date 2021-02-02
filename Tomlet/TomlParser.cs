using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace Tomlet
{
    public class TomlParser
    {
        private static readonly char[] TrueChars = {'t', 'r', 'u', 'e'};
        private static readonly char[] FalseChars = {'f', 'a', 'l', 's', 'e'};

        private int _lineNumber = 1;

        public TomlDocument Parse(string input)
        {
            var document = new TomlDocument();
            using var reader = new StringReader(input);

            while (reader.TryPeek(out var nextChar))
            {
                //We have more to read.
                reader.SkipAnyComment();
                reader.SkipAnyNewlineOrWhitespace();

                //TODO Check for [ here

                //Read a key-value pair
                var (key, value) = ReadKeyValuePair(reader);

                //Insert into the document
                document.PutValue(key, value);

                //Read up until the end of the line, ignoring any comments or whitespace
                reader.SkipWhitespace();
                reader.SkipAnyComment();

                //Ensure we have a newline
                reader.SkipPotentialCR();
                if (!reader.ExpectAndConsume('\n') && reader.TryPeek(out var shouldHaveBeenLF))
                    //Not EOF and found a non-newline char
                    throw new TomlMissingNewlineException(_lineNumber, (char) shouldHaveBeenLF);

                _lineNumber++; //We've consumed a newline, move to the next line number.
            }

            return document;
        }

        private (string key, TomlValue value) ReadKeyValuePair(StringReader reader)
        {
            //Read the key
            var key = ReadKey(reader);

            //Consume the equals sign, potentially with whitespace either side.
            reader.SkipWhitespace();
            if (!reader.ExpectAndConsume('='))
            {
                if (reader.TryPeek(out var shouldHaveBeenEquals))
                    throw new TomlMissingEqualsException(_lineNumber, (char) shouldHaveBeenEquals);

                throw new TomlEOFException(_lineNumber);
            }

            reader.SkipWhitespace();

            //Read the value
            var value = ReadValue(reader);

            return (key, value);
        }

        private string ReadKey(StringReader reader)
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
                reader.Read(); //Consume opening quote
                //Read double-quoted key
                key = reader.ReadWhile(keyChar => !keyChar.IsEquals() && !keyChar.IsNewline() && !keyChar.IsDoubleQuote());
                if (!reader.ExpectAndConsume('"'))
                    throw new UnterminatedTomlKeyException(_lineNumber);
            }
            else if (nextChar.IsSingleQuote())
            {
                reader.Read(); //Consume opening quote.
                
                //Read single-quoted key
                key = reader.ReadWhile(keyChar => !keyChar.IsEquals() && !keyChar.IsNewline() && !keyChar.IsSingleQuote());
                if (!reader.ExpectAndConsume('\''))
                    throw new UnterminatedTomlKeyException(_lineNumber);
            }
            else
                //Read unquoted key
                key = reader.ReadWhile(keyChar => !keyChar.IsEquals() && !keyChar.IsWhitespace() && !keyChar.IsHashSign());

            return key;
        }

        private TomlValue ReadValue(StringReader reader)
        {
            if (!reader.TryPeek(out var startOfValue))
                throw new TomlEOFException(_lineNumber);

            TomlValue value;
            switch (startOfValue)
            {
                case '[':
                    //Array
                    throw new NotImplementedException("Reading arrays is not supported yet");
                case '{':
                    //Inline table
                    throw new NotImplementedException("Reading inline tables is not supported yet");
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
                        
                        //Check the third char. If it's another quote, we have a multiline string. If it's whitespace, a newline, or a #, we have an empty string.
                        //Anything else is an error.
                        var maybeThirdQuote = reader.Peek();
                        if (maybeThirdQuote == startQuote)
                        {
                            reader.Read(); //Consume the third opening quote, for simplicity's sake.
                            value = startQuote.IsSingleQuote() ? ReadMultiLineLiteralString(reader) : ReadMultiLineBasicString(reader);
                        }
                        else if(maybeThirdQuote.IsWhitespace() || maybeThirdQuote.IsNewline() || maybeThirdQuote.IsHashSign())
                        {
                            value = TomlString.EMPTY;
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
                    //I kind of hate that but it's probably fast.
                    //Number. Maybe floating-point.

                    //Read a string, stopping if we hit an equals, whitespace, newline, or comment.
                    var stringValue = reader.ReadWhile(valueChar => !valueChar.IsEquals() && !valueChar.IsWhitespace() && !valueChar.IsNewline() && !valueChar.IsHashSign()).ToLowerInvariant();

                    if (stringValue.Contains('.') || stringValue.Contains('e'))
                        //Try parse as a double.
                        value = TomlDouble.Parse(stringValue) ?? throw new InvalidTomlNumberException(_lineNumber, stringValue);
                    else
                        //Try parse as a long.
                        value = TomlLong.Parse(stringValue) ?? throw new InvalidTomlNumberException(_lineNumber, stringValue);

                    break;
                case 't':
                {
                    //Either "true" or an error
                    var charsRead = reader.ReadChars(4);

                    if (!TrueChars.SequenceEqual(charsRead))
                        throw new TomlInvalidValueException(_lineNumber, (char) startOfValue);

                    value = TomlBoolean.TRUE;
                    break;
                }
                case 'f':
                {
                    //Either "false" or an error
                    var charsRead = reader.ReadChars(5);

                    if (!FalseChars.SequenceEqual(charsRead))
                        throw new TomlInvalidValueException(_lineNumber, (char) startOfValue);

                    value = TomlBoolean.FALSE;
                    break;
                }
                default:
                    throw new TomlInvalidValueException(_lineNumber, (char) startOfValue);
            }

            return value;
        }

        private TomlValue ReadSingleLineBasicString(StringReader reader)
        {
            //No simple read here, we have to accomodate escaped double quotes.
            var content = new StringBuilder();
            
            var escapeMode = false;
            var fourDigitUnicodeMode = false;
            var eightDigitUnicodeMode = false;

            var unicodeStringBuilder = new StringBuilder();
            while (reader.TryPeek(out _))
            {
                var nextChar = reader.Read();
                
                if(nextChar == '"' && !escapeMode)
                    break;

                if (nextChar == '\\' && !escapeMode)
                {
                    escapeMode = true;
                    continue; //Don't append
                }

                if (escapeMode)
                {
                    escapeMode = false;
                    var toAppend = HandleEscapedChar(nextChar, out fourDigitUnicodeMode, out eightDigitUnicodeMode);

                    if(toAppend.HasValue)
                        content.Append(toAppend.Value);
                    continue;
                }

                if (fourDigitUnicodeMode || eightDigitUnicodeMode)
                {
                    //Handle \u1234 and \U12345678
                    unicodeStringBuilder.Append(nextChar);

                    if (fourDigitUnicodeMode && unicodeStringBuilder.Length == 4 || eightDigitUnicodeMode &&  unicodeStringBuilder.Length == 8)
                    {
                        var unicodeString = unicodeStringBuilder.ToString();

                        content.Append(DecipherUnicodeEscapeSequence(unicodeString, fourDigitUnicodeMode));

                        fourDigitUnicodeMode = false;
                        eightDigitUnicodeMode = false;
                        unicodeStringBuilder.Clear();
                        continue;
                    }
                }

                if (nextChar.IsNewline())
                    throw new UnterminatedTomlStringException(_lineNumber);

                content.Append((char) nextChar);
            }

            return new TomlString(content.ToString());
        }

        private char[] DecipherUnicodeEscapeSequence(string unicodeString, bool fourDigitMode)
        {
            char[] toAppend;
            if (unicodeString.Any(c => !c.IsHexDigit()))
                throw new InvalidTomlEscapeException(_lineNumber, $"\\{(fourDigitMode ? 'u' : 'U')}{unicodeString}");

            if (fourDigitMode)
            {
                //16-bit char
                var decodedChar = short.Parse(unicodeString, NumberStyles.HexNumber);
                toAppend = new[] {(char) decodedChar};
            }
            else
            {
                //32-bit char
                var decodedChars = int.Parse(unicodeString, NumberStyles.HexNumber);
                var chars = Encoding.Unicode.GetChars(BitConverter.GetBytes(decodedChars));
                toAppend = chars;
            }

            return toAppend;
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
                    if (allowNewline && escapedChar.IsWhitespace())
                        return null;
                    throw new InvalidTomlEscapeException(_lineNumber, $"\\{escapedChar}");
            }

            return toAppend;
        }

        private TomlValue ReadSingleLineLiteralString(StringReader reader)
        {
            //Literally (hah) just read until a single-quote
            var stringContent = reader.ReadWhile(valueChar => !valueChar.IsWhitespace() && !valueChar.IsSingleQuote() && !valueChar.IsNewline());

            if (!reader.TryPeek(out var terminatingChar))
                //Unexpected EOF
                throw new TomlEOFException(_lineNumber);

            if (!terminatingChar.IsSingleQuote())
                throw new UnterminatedTomlStringException(_lineNumber);

            reader.Read(); //Consume terminating quote.

            return new TomlString(stringContent);
        }

        private TomlValue ReadMultiLineLiteralString(StringReader reader)
        {
            var content = new StringBuilder();
            while (reader.TryPeek(out _))
            {
                var nextChar = reader.Read();

                if (!nextChar.IsSingleQuote())
                {
                    content.Append((char) nextChar);

                    if (nextChar == '\n')
                        _lineNumber++; //We've wrapped to a new line.
                    
                    continue;
                }
                
                //We have a single quote.
                //Is it alone? if so, just continue.
                if(reader.TryPeek(out var potentialSecondQuote) && !potentialSecondQuote.IsSingleQuote())
                    continue;
                
                //We have two quotes in a row. Consume the second one
                reader.Read();
                
                //Do we have three?
                if(reader.TryPeek(out var potentialThirdQuote) && !potentialThirdQuote.IsSingleQuote())
                    continue;
                
                //Ok we have at least three quotes. Consume the third.
                reader.Read();
                
                if(reader.TryPeek(out var afterThirdQuote) && !afterThirdQuote.IsSingleQuote())
                    //And ONLY three quotes. End of literal.
                    break;
                
                //We're at 4 single quotes back-to-back at this point, and the max is 5. I'm just going to do this without a loop because it's probably actually less code.
                //Consume the fourth.
                reader.Read();
                //And we have to append one single quote to our string.
                content.Append('\'');
                
                //Check for a 5th.
                if(reader.TryPeek(out var potentialFifthQuote) && !potentialFifthQuote.IsSingleQuote())
                    //Four in total, so we bail out here.
                    break;
                
                //We have a 5th. Consume it.
                reader.Read();
                //And append to output
                content.Append('\'');
                
                //Check for sixth
                if(reader.TryPeek(out var potentialSixthQuote) && !potentialSixthQuote.IsSingleQuote())
                    //Five in total, so we bail out here.
                    break;
                
                //We have a sixth. This is a syntax error.
                throw new TripleQuoteInTomlMultilineLiteralException(_lineNumber);
            }

            return new TomlString(content.ToString());
        }

        private TomlValue ReadMultiLineBasicString(StringReader reader)
        {
            var content = new StringBuilder();
            
            var escapeMode = false;
            var fourDigitUnicodeMode = false;
            var eightDigitUnicodeMode = false;

            var unicodeStringBuilder = new StringBuilder();
            while (reader.TryPeek(out _))
            {
                var nextChar = reader.Read();
                
                if(nextChar == '"' && !escapeMode)
                    break;

                if (nextChar == '\\' && !escapeMode)
                {
                    escapeMode = true;
                    continue; //Don't append
                }

                if (escapeMode)
                {
                    escapeMode = false;
                    var toAppend = HandleEscapedChar(nextChar, out fourDigitUnicodeMode, out eightDigitUnicodeMode, true);

                    if(toAppend.HasValue)
                        content.Append(toAppend.Value);
                    else if (nextChar.IsNewline())
                    {
                        //Ensure we've fully consumed the newline
                        if (nextChar == '\r' && !reader.ExpectAndConsume('\n'))
                            throw new Exception($"Found a CR without an LF on line {_lineNumber}");
                        
                        //Increment line number
                        _lineNumber++;
                        
                        //Escaped newline indicates we skip any whitespace at the start of the next line
                        reader.SkipWhitespace();
                    }
                    continue;
                }

                if (fourDigitUnicodeMode || eightDigitUnicodeMode)
                {
                    //Handle \u1234 and \U12345678
                    unicodeStringBuilder.Append(nextChar);

                    if (fourDigitUnicodeMode && unicodeStringBuilder.Length == 4 || eightDigitUnicodeMode &&  unicodeStringBuilder.Length == 8)
                    {
                        var unicodeString = unicodeStringBuilder.ToString();

                        content.Append(DecipherUnicodeEscapeSequence(unicodeString, fourDigitUnicodeMode));

                        fourDigitUnicodeMode = false;
                        eightDigitUnicodeMode = false;
                        unicodeStringBuilder.Clear();
                        continue;
                    }
                }

                if (!nextChar.IsDoubleQuote())
                {
                    content.Append((char) nextChar);
                    continue;
                }
                
                //Like above, check for up to 6 quotes.

                //We have a double quote.
                //Is it alone? if so, just continue.
                if(reader.TryPeek(out var potentialSecondQuote) && !potentialSecondQuote.IsDoubleQuote())
                    continue;
                
                //We have two quotes in a row. Consume the second one
                reader.Read();
                
                //Do we have three?
                if(reader.TryPeek(out var potentialThirdQuote) && !potentialThirdQuote.IsDoubleQuote())
                    continue;
                
                //Ok we have at least three quotes. Consume the third.
                reader.Read();
                
                if(reader.TryPeek(out var afterThirdQuote) && !afterThirdQuote.IsDoubleQuote())
                    //And ONLY three quotes. End of literal.
                    break;
                
                //Like above, just going to bruteforce this out instead of writing a loop.
                //Consume the fourth.
                reader.Read();
                //And we have to append one double quote to our string.
                content.Append('"');
                
                //Check for a 5th.
                if(reader.TryPeek(out var potentialFifthQuote) && !potentialFifthQuote.IsDoubleQuote())
                    //Four in total, so we bail out here.
                    break;
                
                //We have a 5th. Consume it.
                reader.Read();
                //And append to output
                content.Append('"');
                
                //Check for sixth
                if(reader.TryPeek(out var potentialSixthQuote) && !potentialSixthQuote.IsDoubleQuote())
                    //Five in total, so we bail out here.
                    break;
                
                //We have a sixth. This is a syntax error.
                throw new TripleQuoteInTomlMultilineSimpleStringException(_lineNumber);
            }

            return new TomlString(content.ToString());
        }
    }
}