using System;
using Tomlet.Tests.TestModelClasses;
using Xunit;
using Xunit.Abstractions;

namespace Tomlet.Tests
{
    public class ComplexSerializationTests
    {
        
        private readonly ITestOutputHelper _testOutputHelper;

        public ComplexSerializationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        
        [Fact]
        public void ComplexSerializationWorks()
        {
            var testClass = new ComplexTestClass
            {
                TestString = "Hello world, how are you?",
                ClassOnes =
                {
                    new ComplexTestClass.SubClassOne {SubKeyOne = "Hello"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "World"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "How"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "Are"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "You"},
                },
                SubClass2 = new ComplexTestClass.SubClassTwo {
                    SubKeyOne = "Hello world, how are you?",
                    SubKeyTwo = DateTimeOffset.Now,
                    SubKeyThree = 17,
                    SubKeyFour = 2.34f,
                }
            };

            var tomlString = TomletMain.TomlStringFrom(testClass);
            
            _testOutputHelper.WriteLine("Got TOML string:\n" + tomlString);
            
            var deserializedAgain = TomletMain.To<ComplexTestClass>(tomlString);
            
            Assert.Equal(testClass, deserializedAgain);
        }

        [Fact]
        public void SerializingNullFieldsExcludesThem()
        {
            var testClass = new ComplexTestClass
            {
                TestString = null,
                ClassOnes =
                {
                    new ComplexTestClass.SubClassOne {SubKeyOne = "Hello"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "World"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "How"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "Are"},
                    new ComplexTestClass.SubClassOne {SubKeyOne = "You"},
                },
                SubClass2 = null
            };

            var tomlString = TomletMain.TomlStringFrom(testClass);
            
            _testOutputHelper.WriteLine("Got TOML string:\n" + tomlString);

            var doc = new TomlParser().Parse(tomlString);
            
            Assert.False(doc.ContainsKey("SC2"));
            Assert.False(doc.ContainsKey("TestString"));
            
            var deserializedAgain = TomletMain.To<ComplexTestClass>(tomlString);
            
            Assert.Equal(testClass, deserializedAgain);
        }

        [Fact]
        public void SerializingNullPropertiesExcludesThem()
        {
            var testClass = new SimplePropertyTestClass
            {
                MyString = null,
                MyBool = true,
            };

            var tomlString = TomletMain.TomlStringFrom(testClass);

            _testOutputHelper.WriteLine("Got TOML string:\n" + tomlString);

            var doc = new TomlParser().Parse(tomlString);

            Assert.False(doc.ContainsKey("MyString"));

            var deserializedAgain = TomletMain.To<SimplePropertyTestClass>(tomlString);

            Assert.Equal(testClass, deserializedAgain);
        }

        [Fact]
        public void ComplexRecordSerializationWorks()
        {
            var testRecord = new ComplexTestRecord
            {
                MyString = "Test",
                MyWidget = new Widget
                {
                    MyInt = 1,
                },
            };

            var tomlString = TomletMain.TomlStringFrom(testRecord);

            _testOutputHelper.WriteLine("Got TOML string:\n" + tomlString);

            var deserializedAgain = TomletMain.To<ComplexTestRecord>(tomlString);

            Assert.Equal(testRecord, deserializedAgain);
        }

        [Fact]
        public void DeserializingArraysWorks()
        {
            //Test string taken from a query on discord, that's where the unusual email addresses come from
            var deserialized = TomletMain.To<ExampleMailboxConfigClass>(TestResources.ExampleMailboxConfigurationTestInput);
            
            Assert.Equal("whatev@gmail.com", deserialized.mailbox);
            Assert.Equal("no", deserialized.username);
            Assert.Equal("secret", deserialized.password);
            
            Assert.Equal(2, deserialized.rules.Length);
            
            Assert.Equal(2, deserialized.rules[0].allowed.Length);
            Assert.Equal("yeet@gmail.com", deserialized.rules[0].address);
            
            Assert.Equal("urmum@gmail.com", deserialized.rules[1].address);
        }

        [Fact]
        public void SerializationChoosesInlineTablesForSimpleObjects()
        {
            var anObject = new {value = 1, str = "hello"};

            var anObjectWithTheObject = new {obj = anObject};

            var tomlString = TomletMain.TomlStringFrom(anObjectWithTheObject);
            
            Assert.Equal("obj = { value = 1, str = \"hello\" }", tomlString.Trim());
            
        }
    }
}