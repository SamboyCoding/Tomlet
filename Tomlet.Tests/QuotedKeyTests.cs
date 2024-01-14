using System;
using System.Collections.Generic;
using System.Linq;
using Tomlet.Exceptions;
using Xunit;

namespace Tomlet.Tests
{
    public class QuotedKeysTests
    {
        [Theory]
        [InlineData("\"a.'b\"", "a.'b")] // a.'b
        [InlineData("\"a.\\\"b\"", "a.\"b")] // a."b
        [InlineData("\"\"", "")] // 
        [InlineData("\"\\\"\"", "\"")] // "
        [InlineData("\"a.🐱b\"", "a.🐱b")] // a.🐱b
        [InlineData("'a.\"b'", "a.\"b")] // a."b
        [InlineData("'a.\\\"b'", "a.\\\"b")] // a.\"b
        [InlineData("''", "")] // 
        [InlineData("'\"'", "\"")] // \"
        [InlineData("'\\\"'", "\\\"")] // \"
        [InlineData("'a.🐱b'", "a.🐱b")] // a.🐱b
        [InlineData("\"a.b\\\".c\"", "a.b\".c")] // a.b".c
        public void NonDottedKeysWork(string inputKey, string expectedKey)
        {
            var inputString = $"{inputKey} = \"value\"";
            var dict = TomletMain.To<Dictionary<string, string>>(inputString);
            Assert.Contains(expectedKey, (IDictionary<string, string>)dict);
        }

        [Theory]
        [InlineData("\"a\"b\"")]
        [InlineData("'a'b'")]
        [InlineData("'a\\'b'")]
        //[InlineData("a\"b")] // Illegal in specs, but no harm in reading it
        //[InlineData("a'b")]  // Illegal in specs, but no harm in reading it
        //[InlineData("a🐱b")] // Illegal in specs, but no harm in reading it
        [InlineData("'ab\"")]
        public void IllegalNonDottedKeysThrow(string inputKey)
        {
            var inputString = $"{inputKey} = \"value\"";
            Assert.ThrowsAny<TomlException>(() => _ = TomletMain.To<Dictionary<string, string>>(inputString));
        }

        [Theory]
        [InlineData("'a.b'.c", "a.b", "c")]
        [InlineData("'a.b'.\"c\"", "a.b", "c")]
        [InlineData("a.'b.c'", "a", "b.c")]
        [InlineData("\"a\".'b.c'", "a", "b.c")]
        [InlineData("\"a\\\".b.c", "a", "b.c")]
        [InlineData("'a.\"b'.c", "a.\"b", "c")]
        [InlineData("\"a.b\\\"c\".d", "a.b\"c", "d")]
        public void DottedKeysWork(string inputKey, string expectedKey, string expectedSubkey)
        {
            var inputString = $"{inputKey} = \"value\"";
            var dict = TomletMain.To<Dictionary<string, Dictionary<string, string>>>(inputString);
            var subDict = Assert.Contains(expectedKey, (IDictionary<string, Dictionary<string, string>>)dict);
            Assert.Contains(expectedSubkey, (IDictionary<string, string>)subDict);
        }

        [Theory]
        [InlineData("'a.\"b'.c\"")]
        [InlineData("\"a.bc\".d\"")]
        [InlineData("\"a.b\"c\".d\"")]
        [InlineData("\"a.b\"c\".d")]
        [InlineData("\"a.b\\\"c\".d\"")]
        [InlineData("'a.b'c'.d")]
        [InlineData("'a.b\\'c'.d")]
        [InlineData("'a.bc'.d'")]
        public void IllegalDottedKeysThrow(string inputKey)
        {
            var inputString = $"{inputKey} = \"value\"";
            Assert.ThrowsAny<TomlException>(() => _ = TomletMain.To<Dictionary<string, string>>(inputString));
        }


        [Theory]
        [InlineData("\"a\"b\"", @"(?:'""a""b""')|(?:""\\""a\\""b\\"""")")] // Simple or Literal
        [InlineData("'a'b'", @"""'a'b'""")] // Simple only
        [InlineData("'a\\'b'", @"""'a\\'b'""")] // Simple only
        [InlineData("a\"b", @"(?:'a""b')|(?:""a\\""b"")")] // Simple or Literal
        [InlineData("a'b", @"""a'b""")] // Simple only
        [InlineData("a🐱b", @"(?:'a🐱b')|(?:""a🐱b"")")] // Simple or Literal
        [InlineData("'ab\"", @"""'ab\\""""")] // Simple only
        public void SerializingIllegalKeysWorks(string inputKey, string expectedOutput)
        {
            var dict = new Dictionary<string, string>
            {
                { inputKey, "a" },
            };
            var document = TomletMain.DocumentFrom(dict);
            Assert.NotEmpty(document.Keys);
            var parsedKey = document.Keys.First();
            Assert.Matches(expectedOutput, parsedKey);
        }
    }
}