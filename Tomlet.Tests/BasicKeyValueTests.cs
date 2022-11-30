using System;
using System.Linq;
using Tomlet.Exceptions;
using Tomlet.Models;
using Xunit;
using Xunit.Abstractions;

namespace Tomlet.Tests
{
    public class BasicKeyValueTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public BasicKeyValueTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private TomlDocument GetDocument(string resource)
        {
            var parser = new TomlParser();
            return parser.Parse(resource);
        }

        [Fact(Timeout = 5_000)]
        public void ASingleKeyValuePairCanBeParsed()
        {
            //This should yield one key, 'key', mapped to one string value, 'value'
            var doc = GetDocument(TestResources.BasicKVPTestInput);

            Assert.True(doc.Entries.Count == 1, "Document contains one element.");
            Assert.True(doc.Entries.ContainsKey("key"), "Document's only key is called 'key'");
            Assert.True(doc.Entries["key"] is TomlString, "The value associated with 'key' is a TomlString");
            Assert.Equal("value", ((TomlString) doc.Entries["key"]).Value);
        }

        [Fact]
        public void MultipleKeysCanBeParsedIgnoringComments()
        {
            var doc = GetDocument(TestResources.CommentTestInput);
            
            //Check keys
            Assert.Collection(doc.Entries.Keys,
                key1 => Assert.Equal("key1", key1),
                key2 => Assert.Equal("key2", key2),
                key3 => Assert.Equal("another", key3)
            );
            
            //Check values
            Assert.Collection(doc.Entries.Values,
                value1 => Assert.Equal("value1", Assert.IsType<TomlString>(value1).Value),
                value2 => Assert.Equal("value2", Assert.IsType<TomlString>(value2).Value),
                value3 => Assert.Equal("# This is not a comment", Assert.IsType<TomlString>(value3).Value)
            );
        }

        [Fact]
        public void AKeyWithNoValueShouldThrowAnException()
        {
            Assert.Throws<TomlInvalidValueException>(() =>
            {
                try
                {
                    return GetDocument(TestResources.UnspecifiedValueTestInput);
                }
                catch (Exception e)
                {
                    _testOutputHelper.WriteLine(e.ToString());
                    throw;
                }
            });
        }

        [Fact]
        public void MultiplePairsOnOneLineThrowsAnException()
        {
            Assert.Throws<TomlMissingNewlineException>(() =>
            {
                try
                {
                    return GetDocument(TestResources.MultiplePairsOnOneLineTestInput);
                }
                catch (Exception e)
                {
                    _testOutputHelper.WriteLine(e.ToString());
                    throw;
                }
            });
        }
        
        [Fact]
        public void KeysCanOptionallyContainUnderscoresAndDashesOrBeEntirelyNumeric()
        {
            var doc = GetDocument(TestResources.NonSimpleKeysTestInput);
            
            //Check keys
            Assert.Collection(doc.Entries.Keys,
                key1 => Assert.Equal("key", key1),
                key2 => Assert.Equal("bare_key", key2),
                key3 => Assert.Equal("bare-key", key3),
                key4 => Assert.Equal("1234", key4)
            );
            
            //Check values
            foreach (var entry in doc.Entries.Values)
            {
                Assert.Equal("value", Assert.IsType<TomlString>(entry).Value);
            }
        }
        
        [Fact]
        public void QuotedKeysAreSupportedAndCanContainDotsAndWhitespace()
        {
            var doc = GetDocument(TestResources.QuotedKeysTestInput);
            
            //Check keys
            Assert.Collection(doc.Entries.Keys,
                key1 => Assert.Equal("127.0.0.1", key1),
                key2 => Assert.Equal("character encoding", key2),
                key3 => Assert.Equal("ʎǝʞ", key3),
                key4 => Assert.Equal("key2", key4),
                key5 => Assert.Equal("quoted \"value\"", key5)
            );
            
            //Check values
            foreach (var entry in doc.Entries.Values)
            {
                Assert.Equal("value", Assert.IsType<TomlString>(entry).Value);
            }
        }
        
        [Fact]
        public void MissingAKeyNameThrowsAnException()
        {
            Assert.Throws<NoTomlKeyException>(() =>
            {
                try
                {
                    return GetDocument(TestResources.EmptyKeyNameTestInput);
                }
                catch (Exception e)
                {
                    _testOutputHelper.WriteLine(e.ToString());
                    throw;
                }
            });
        }
        
        [Fact]
        public void EmptyKeyNamesAreLegalIfQuoted()
        {
            var doc = GetDocument(TestResources.BlankKeysAreAcceptedTestInput);
            
            Assert.Single(doc.Entries);
            Assert.Equal("", doc.Entries.Keys.First());
            Assert.Equal("blank", Assert.IsType<TomlString>(doc.Entries[""]).Value);
        }
    }
}