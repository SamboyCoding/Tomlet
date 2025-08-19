using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tomlet.Tests.TestModelClasses;
using Xunit;
using Xunit.Abstractions;

namespace Tomlet.Tests;

public class EnumTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public EnumTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CanSerializeEnum()
    {
        var toml = TomletMain.TomlStringFrom(new {EnumValue = TestEnum.Value1}).Trim();
        Assert.Equal("EnumValue = \"Value1\"", toml);
    }

    [Fact]
    public void SerializingAnUndefinedEnumValueThrows()
    {
        var testObj = new {EnumValue = (TestEnum)4};
        Assert.Throws<ArgumentException>(() => TomletMain.TomlStringFrom(testObj));
    }

    [Fact]
    public void CanSerializeDictWithEnumValues()
    {
        var testObj = new Dictionary<TestEnum, int> {{TestEnum.Value1, 1}, {TestEnum.Value2, 2}, {TestEnum.Value3, 3}};
        var toml = TomletMain.TomlStringFrom(testObj);
        Assert.Equal("Value1 = 1\nValue2 = 2\nValue3 = 3\n", toml);
    }

    [Fact]
    public void CanSerializeNonInlinedClass()
    {
        var testObj = new Dictionary<TestEnum, ClassWithDoNotInlineMembers>
        {
            {
                TestEnum.Value1, new ClassWithDoNotInlineMembers
                {
                    ShouldNotBeInlinedField = new Dictionary<string, string> {{"Key", "Value1"}}
                }
            },
        };
        var toml = TomletMain.TomlStringFrom(testObj);
        var expected = "[Value1]\nShouldBeInlined = {  }\n[Value1.ShouldNotBeInlinedField]\nKey = \"Value1\"\n\n[Value1.ShouldNotBeInlinedProp]\n\n\n";
        Assert.Equal(expected, toml);
    }

    [Fact]
    public void CanDeserializeEnumDictionary()
    {
        var toml = "Value1 = 1\nValue2 = 2\nValue3 = 3\n";
        var result = TomletMain.To<Dictionary<TestEnum, int>>(toml);
        var expected = new Dictionary<TestEnum, int> {{TestEnum.Value1, 1}, {TestEnum.Value2, 2}, {TestEnum.Value3, 3}};
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void CanIgnoreInvalidEnumDictionaryKeys()
    {
        var toml = "Value1 = 1\nValue2 = 2\nValue3 = 3\nValue4 = 4\nValue5 = 5\nValue6 = 6\n";
        var result = TomletMain.To<Dictionary<TestEnum, int>>(toml, new TomlSerializerOptions { IgnoreInvalidEnumValues = true });
        var expected = new Dictionary<TestEnum, int> {{TestEnum.Value1, 1}, {TestEnum.Value2, 2}, {TestEnum.Value3, 3}};
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void CanDeserializeEnumDictionaryWithFields()
    {
        var toml = @"
[Value1]
a = 'A'
b = 'B'
";
        var result = TomletMain.To<Dictionary<TestEnum, Subname>>(toml);
        var expected = new Dictionary<TestEnum, Subname>
        {
            {TestEnum.Value1, new Subname {a = "A", b = "B"}},
        };

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CanDeserializeEnumDictionaryInClass()
    {
        var toml = @"
[Subnames]
[Subnames.Value1]
a = 'A'
b = 'B'
";
        var result = TomletMain.To<TomlTestClassWithEnumDict>(toml);
        var expected = new TomlTestClassWithEnumDict
        {
            Subnames = new Dictionary<TestEnum, Subname> {{TestEnum.Value1, new Subname {a = "A", b = "B"}}},
        };

        Assert.Equal(expected.Subnames, result.Subnames);
    }
}