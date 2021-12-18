using System.Linq;
using Tomlet.Models;
using Xunit;

namespace Tomlet.Tests
{
    public class StringTests
    {
        private TomlDocument GetDocument(string resource)
        {
            var parser = new TomlParser();
            return parser.Parse(resource);
        }

        [Fact]
        public void EscapedValuesAreAllowedInDoubleQuotedStrings()
        {
            var document = GetDocument(TestResources.EscapedDoubleQuotedStringTestInput);

            Assert.Single(document.Entries);
            Assert.Equal("str", document.Entries.Keys.First());
            Assert.Equal("I'm a string. \"You can quote me\". Name\tJosé\nLocation\tSF.", Assert.IsType<TomlString>(document.Entries.Values.First()).Value);
        }

        [Fact]
        public void MultiLineBasicStringsAreParsedCorrectly()
        {
            var document = GetDocument(TestResources.MultiLineSimpleStringTestInput);

            Assert.Single(document.Entries);
            Assert.Equal("str1", document.Entries.Keys.First());
            Assert.Equal($"Roses are red\nViolets are blue", Assert.IsType<TomlString>(document.Entries.Values.First()).Value);
        }

        [Fact]
        public void WhitespaceInDoubleQuotedMultilineStringsIsStrippedIfEscaped()
        {
            //This test checks if \ followed by a newline correctly strips out whitespace after the newline.
            var document = GetDocument(TestResources.WhitespaceRemovalTestInput);

            Assert.Equal(3, document.Entries.Count);

            //Check keys
            Assert.Collection(document.Entries.Keys,
                key1 => Assert.Equal("str1", key1),
                key2 => Assert.Equal("str2", key2),
                key3 => Assert.Equal("str3", key3)
            );

            Assert.Equal(
                Assert.IsType<TomlString>(document.Entries["str1"]).Value,
                Assert.IsType<TomlString>(document.Entries["str2"]).Value
            );

            Assert.Equal(
                Assert.IsType<TomlString>(document.Entries["str2"]).Value,
                Assert.IsType<TomlString>(document.Entries["str3"]).Value
            );
        }

        [Fact]
        public void DoubleQuotesCanBePresentWithinSimpleMultilineStrings()
        {
            var document = GetDocument(TestResources.DoubleQuotesInMultilineBasicTestInput);

            Assert.Equal(4, document.Entries.Count);

            //Check keys
            Assert.Collection(document.Entries.Keys,
                key1 => Assert.Equal("str4", key1),
                key2 => Assert.Equal("str5", key2),
                key3 => Assert.Equal("str6", key3),
                key4 => Assert.Equal("str7", key4)
            );

            //Check values
            Assert.Collection(document.Entries.Values,
                entry => Assert.Equal("Here are two quotation marks: \"\". Simple enough.", Assert.IsType<TomlString>(entry).Value),
                entry => Assert.Equal("Here are three quotation marks: \"\"\".", Assert.IsType<TomlString>(entry).Value),
                entry => Assert.Equal("Here are fifteen quotation marks: \"\"\"\"\"\"\"\"\"\"\"\"\"\"\".", Assert.IsType<TomlString>(entry).Value),
                entry => Assert.Equal("\"This,\" she said, \"is just a pointless statement.\"", Assert.IsType<TomlString>(entry).Value)
            );
        }

        [Fact]
        public void LiteralStringsArentEscaped()
        {
            var document = GetDocument(TestResources.LiteralStringTestInput);
            
            Assert.Equal(5, document.Entries.Count);

            //Check keys
            Assert.Collection(document.Entries.Keys,
                key1 => Assert.Equal("winpath", key1),
                key2 => Assert.Equal("winpath2", key2),
                key3 => Assert.Equal("quoted", key3),
                key4 => Assert.Equal("regex", key4),
                key5 => Assert.Equal("empty", key5)
            );

            //Check values
            Assert.Collection(document.Entries.Values,
                entry => Assert.Equal(@"C:\Users\nodejs\templates", Assert.IsType<TomlString>(entry).Value),
                entry => Assert.Equal(@"\\ServerX\admin$\system32\", Assert.IsType<TomlString>(entry).Value),
                entry => Assert.Equal("Tom \"Dubs\" Preston-Werner", Assert.IsType<TomlString>(entry).Value),
                entry => Assert.Equal(@"<\i\c*\s*>", Assert.IsType<TomlString>(entry).Value),
                entry => Assert.Empty(Assert.IsType<TomlString>(entry).Value)
            );
        }
        
        [Fact]
        public void MultilineLiteralStringsFunctionAsIntended()
        {
            var document = GetDocument(TestResources.MultiLineLiteralStringTestInput);
            
            Assert.Equal(2, document.Entries.Count);

            //Check keys
            Assert.Collection(document.Entries.Keys,
                key1 => Assert.Equal("regex2", key1),
                key2 => Assert.Equal("lines", key2)
            );

            //Check values
            Assert.Collection(document.Entries.Values,
                entry => Assert.Equal(@"I [dw]on't need \d{2} apples", Assert.IsType<TomlString>(entry).Value),
                entry => Assert.Equal($"The first newline is\ntrimmed in raw strings.\n   All other whitespace\n   is preserved.\n", Assert.IsType<TomlString>(entry).Value)
            );
        }
        
        [Fact]
        public void MultilineLiteralStringsCanContainQuotations()
        {
            var document = GetDocument(TestResources.SingleQuotesInMultilineLiteralTestInput);
            
            Assert.Equal(3, document.Entries.Count);

            //Check keys
            Assert.Collection(document.Entries.Keys,
                key1 => Assert.Equal("quot15", key1),
                key2 => Assert.Equal("apos15", key2),
                key3 => Assert.Equal("str", key3)
            );

            //Check values
            Assert.Collection(document.Entries.Values,
                entry => Assert.Equal("Here are fifteen quotation marks: \"\"\"\"\"\"\"\"\"\"\"\"\"\"\"", Assert.IsType<TomlString>(entry).Value),
                entry => Assert.Equal("Here are fifteen apostrophes: '''''''''''''''", Assert.IsType<TomlString>(entry).Value),
                entry => Assert.Equal("'That,' she said, 'is still pointless.'", Assert.IsType<TomlString>(entry).Value)
            );
        }

        [Fact]
        public void EscapedQuotesInAKeyAreValid()
        {
            var document = GetDocument(TestResources.KeyWithEscapedQuotesTestInput);
            
            Assert.Equal("hello", document.GetString("\"a.b\""));
        }

        [Fact]
        public void EscapedStringsCanBeSerializedAndDeserializedToTheSameValue()
        {
            //Args = '"C:\\Something"'
            var document = GetDocument(TestResources.LiteralQuotedPathWithBackslashesTestInput);
            
            Assert.Equal(@"""C:\\Something""", document.GetString("Args"));

            Assert.Equal(TestResources.LiteralQuotedPathWithBackslashesTestInput, document.SerializedValue.Trim());
        }
    }
}