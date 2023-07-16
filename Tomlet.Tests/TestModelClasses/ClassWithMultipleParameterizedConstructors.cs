namespace Tomlet.Tests.TestModelClasses;

public class ClassWithMultipleParameterizedConstructors
{
    public ClassWithMultipleParameterizedConstructors(string myString)
    {
        MyString = myString;
    }
    
    public ClassWithMultipleParameterizedConstructors(string myString, int age)
    {
        MyString = myString;
    }
    
    public string MyString { get; set; }
}