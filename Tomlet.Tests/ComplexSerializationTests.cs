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
                SC2 = new ComplexTestClass.SubClassTwo {
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
    }
}