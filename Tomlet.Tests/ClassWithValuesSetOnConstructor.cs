namespace Tomlet.Tests;

public class ClassWithValuesSetOnConstructor
{
    public ClassWithValuesSetOnConstructor(string myString)
    {
        MyString = "Modified on constructor!";
    }

    public string MyString { get; set; }
}