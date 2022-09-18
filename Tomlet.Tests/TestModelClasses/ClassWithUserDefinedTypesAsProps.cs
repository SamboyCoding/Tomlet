using Tomlet.Attributes;

namespace Tomlet.Tests.TestModelClasses;

public class TableA
{
    [TomlProperty("IntA")]
    public int IntA { get; set; }
    [TomlProperty("StringA")]
    public string StringA { get; set; }
}

public class TableB
{
    [TomlProperty("IntB")]
    public int IntB { get; set; }
    
    [TomlProperty("StringB")]
    public string StringB { get; set; }
}

public abstract class Base
{
    [TomlProperty("A")]
    public TableA A { get; set; }
    
    [TomlProperty("B")]
    public TableB B { get; set; }
    public string Junk;
}

public class TableC
{
    [TomlProperty("IntC")]
    public int IntC { get; set; }
    [TomlProperty("StringC")]
    public string StringC { get; set; }
}

public class Derived : Base
{
    [TomlProperty("C")]
    public TableC C { get; set; }
}