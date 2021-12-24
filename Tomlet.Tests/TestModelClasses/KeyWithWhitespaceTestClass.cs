using Tomlet.Attributes;

namespace Tomlet.Tests.TestModelClasses;

public class KeyWithWhitespaceTestClass
{
    [TomlProperty("Key With Whitespace")]
    public string KeyWithWhitespace { get; set; }
}