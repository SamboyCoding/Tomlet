using System;
using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests
{
    public class ObjectToStringTests
    {
        [Fact]
        public void SimpleObjectToTomlStringWorks()
        {
            var testObject = new SimplePrimitiveTestClass
            {
                MyBool = true,
                MyFloat = 420.69f,
                MyString = "Hello, world!",
                MyDateTime = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc)
            };

            var serializedForm = Tomlet.TomlStringFrom(testObject);

            Assert.Equal("MyString = \"Hello, world!\"\nMyFloat = 420.69000244140625\nMyBool = true\nMyDateTime = 1970-01-01T07:00:00", serializedForm.Trim());
        }

        [Fact]
        public void SerializingSimpleObjectAndDeserializingAgainGivesEquivalentObject()
        {
            var testObject = new SimplePrimitiveTestClass
            {
                MyBool = true,
                MyFloat = 420.69f,
                MyString = "Hello, world!",
                MyDateTime = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc)
            };
            
            var serializedForm = Tomlet.TomlStringFrom(testObject);

            var deserializedAgain = Tomlet.To<SimplePrimitiveTestClass>(serializedForm);

            Assert.Equal(testObject, deserializedAgain);
        }
    }
}