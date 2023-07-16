using System.Collections.Generic;
using Tomlet.Models;
using Tomlet.Tests.TestModelClasses;
using Xunit;

namespace Tomlet.Tests;

public class DictionaryTests
{
    private TomlDocument GetDocument(string resource)
    {
        var parser = new TomlParser();
        return parser.Parse(resource);
    }
    
    [Fact]
    public void DictionariesAsFieldsWork()
    {
        var doc = GetDocument(TestResources.DictionaryAsFieldTestInput);

        var obj = TomletMain.To<ClassWithDictionary>(doc);
        
        Assert.Equal(2, obj.name.Count);
        Assert.True(obj.name.ContainsKey("subname1"));
        Assert.True(obj.name.ContainsKey("subname2"));

        //Just make sure this doesn't throw
        var serialized = TomletMain.TomlStringFrom(obj); 
    }

    [Fact]
    public void DictionaryKeysShouldBeProperlyEscaped()
    {
        var dictionary = new Dictionary<string, string>
        {
            {"normal-key", "normal-key"},
            {"normal_key", "normal_key"},
            {"normalkey", "normalkey"},
            {"key with space", "\"key with spaces\""},
            {"key!with{}(*%&)random[other+symbols", "\"key!with{}(*%&)random[other+symbols\""},
            {"key/with/slashes", "\"key/with/slashes\""},
            {"Nam\\e", "\"Nam\\\\e\""}
        };

        var obj = new ClassWithDictionary
        {
            GenericDictionary = dictionary
        };
        
        var serialized = TomletMain.TomlStringFrom(obj);
        foreach(var (_, expectedValue) in dictionary)
        {
            Assert.Contains(expectedValue, serialized);
        }
    }
}