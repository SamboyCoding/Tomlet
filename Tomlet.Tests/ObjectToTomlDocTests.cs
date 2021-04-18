using System;
using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests
{
    public class ObjectToTomlDocTests
    {
        [Fact]
        public static void SimpleObjectToTomlDocWorks()
        {
            var testObject = new SimplePrimitiveTestClass
            {
                MyBool = true,
                MyFloat = 420.69f,
                MyString = "Hello, world!",
                MyDateTime = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc)
            };

            var tomlDoc = TomletMain.DocumentFrom(testObject);
            
            Assert.Equal(4, tomlDoc.Entries.Count);
            Assert.True(tomlDoc.GetBoolean("MyBool"));
            Assert.True(Math.Abs(tomlDoc.GetFloat("MyFloat") - 420.69) < 0.01);
            Assert.Equal("Hello, world!", tomlDoc.GetString("MyString"));
            Assert.Equal("1970-01-01T07:00:00", tomlDoc.GetValue("MyDateTime").StringValue);
        }
    }
}