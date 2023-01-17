using System;
using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests;

public class EnumSerializationTests
{
    [Fact]
    public void CanSerializeEnum()
    {
        var toml = TomletMain.TomlStringFrom(new {EnumValue = TestEnum.Value1}).Trim();
        Assert.Equal("EnumValue = \"Value1\"", toml);
    }

    [Fact]
    public void SerializingAnUndefinedEnumValueThrows()
    {
        var testObj = new { EnumValue = (TestEnum)4 };
        Assert.Throws<ArgumentException>(() => TomletMain.TomlStringFrom(testObj));
    }
}