using System;
using System.Collections.Generic;
using Tomlet.Exceptions;
using Tomlet.Models;
using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests
{
    public class ClassDeserializationTests
    {
        [Fact]
        public void DictionaryDeserializationWorks()
        {
            var dict = TomletMain.To<Dictionary<string, string>>(TestResources.SimplePrimitiveDeserializationTestInput);
            
            Assert.Equal(4, dict.Count);
            Assert.Equal("Hello, world!", dict["MyString"]);
            Assert.Equal("690.42", dict["MyFloat"]);
            Assert.Equal("true", dict["MyBool"]);
            Assert.Equal("1970-01-01T07:00:00", dict["MyDateTime"]);
        }
        
        [Fact]
        public void SimpleCompositeDeserializationWorks()
        {
            var type = TomletMain.To<SimplePrimitiveTestClass>(TestResources.SimplePrimitiveDeserializationTestInput);
            
            Assert.Equal("Hello, world!", type.MyString);
            Assert.True(Math.Abs(690.42 - type.MyFloat) < 0.01);
            Assert.True(type.MyBool);
            Assert.Equal(new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc), type.MyDateTime);
        }

        [Fact]
        public void SimplePropertyDeserializationWorks()
        {
            var type = TomletMain.To<SimplePropertyTestClass>(TestResources.SimplePrimitiveDeserializationTestInput);

            Assert.Equal("Hello, world!", type.MyString);
            Assert.True(Math.Abs(690.42 - type.MyFloat) < 0.01);
            Assert.True(type.MyBool);
            Assert.Equal(new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc), type.MyDateTime);
        }

        [Fact]
        public void SimpleRecordDeserializationWorks()
        {
            var type = TomletMain.To<SimpleTestRecord>(TestResources.SimplePrimitiveDeserializationTestInput);

            Assert.Equal("Hello, world!", type.MyString);
            Assert.True(Math.Abs(690.42 - type.MyFloat) < 0.01);
            Assert.True(type.MyBool);
            Assert.Equal(new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc), type.MyDateTime);
        }

        [Fact]
        public void ClassWithParameterlessConstructorDeserializationWorks()
        {
            var type = TomletMain.To<ClassWithParameterlessConstructor>(TestResources.SimplePrimitiveDeserializationTestInput);
            
            Assert.Equal("Hello, world!", type.MyString);
        }
        
        [Fact]
        public void AnArrayOfEmptyStringsCanBeDeserialized()
        {
            var wrapper = TomletMain.To<StringArrayWrapper>(TestResources.ArrayOfEmptyStringTestInput);
            var array = wrapper.array;
            
            Assert.Equal(5, array.Length);
            Assert.All(array, s => Assert.Equal(string.Empty, s));
        }

        [Fact]
        public void AttemptingToDeserializeADocumentWithAnIncorrectlyTypedFieldThrows()
        {
            var document = TomlDocument.CreateEmpty();
            document.Put("MyFloat", "Not a float");

            var ex = Assert.Throws<TomlFieldTypeMismatchException>(() => TomletMain.To<SimplePrimitiveTestClass>(document));

            var msg = $"While deserializing an object of type {typeof(SimplePrimitiveTestClass).FullName}, found field MyFloat expecting a type of Double, but value in TOML was of type String";
            Assert.Equal(msg, ex.Message);
        }

        [Fact]
        public void ShouldOverrideDefaultConstructorsValues()
        {
            var options = new TomlSerializerOptions { OverrideConstructorValues = true };
            var type = TomletMain.To<ClassWithValuesSetOnConstructor>(TestResources.SimplePrimitiveDeserializationTestInput, options);
            
            Assert.Equal("Hello, world!", type.MyString);
        }
        
        [Fact]
        public void ShouldNotOverrideDefaultConstructorsValues()
        {
            var options = new TomlSerializerOptions { OverrideConstructorValues = false };
            var type = TomletMain.To<ClassWithValuesSetOnConstructor>(TestResources.SimplePrimitiveDeserializationTestInput, options);
            
            Assert.Equal("Modified on constructor!", type.MyString);
        }
    }
}