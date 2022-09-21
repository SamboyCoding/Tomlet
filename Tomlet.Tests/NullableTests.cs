using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests;

public class NullableTests
{
    [Fact]
    public void SerializingNullablesSkipsThemIfTheyDontHaveAValue()
    {
        var withValue = new ClassWithNullableValueType() {MyShort = 123};
        var withoutValue = new ClassWithNullableValueType() {MyShort = null};
        
        var withValueToml = TomletMain.TomlStringFrom(withValue).Trim();
        var withoutValueToml = TomletMain.TomlStringFrom(withoutValue).Trim();
        
        Assert.Equal("MyShort = 123", withValueToml);
        Assert.Equal("", withoutValueToml);
    }
    
    [Fact]
    public void DeserializingNullablesWorks()
    {
        var withValueToml = "MyShort = 123";
        var withoutValueToml = "";
        
        var withValue = TomletMain.To<ClassWithNullableValueType>(withValueToml);
        var withoutValue = TomletMain.To<ClassWithNullableValueType>(withoutValueToml);
        
        Assert.Equal((short) 123, withValue.MyShort);
        Assert.Null(withoutValue.MyShort);
    }
}