# Tomlet
## A TOML library for .NET

[![NuGet](https://img.shields.io/nuget/v/Samboy063.Tomlet)](https://www.nuget.org/packages/Samboy063.Tomlet/)
![GitHub Workflow Status](https://img.shields.io/github/workflow/status/SamboyCoding/Tomlet/.NET)
![Toml Version](https://img.shields.io/badge/TOML%20Version-1.0.0-blue)
[![Coverage Status](https://coveralls.io/repos/github/SamboyCoding/Tomlet/badge.svg?branch=master)](https://coveralls.io/github/SamboyCoding/Tomlet?branch=master)

### I have a [discord server](https://discord.gg/CfPSP5GMMv) for support

Tomlet is a zero-dependency library for the [TOML](https://toml.io/) configuration file format. It's targeting [TOML v1.0.0](https://toml.io/en/v1.0.0).

The entire 1.0.0 specification as described [here](https://toml.io/en/v1.0.0) is implemented.  

## A word on dotted keys

The TOML specification allows for dotted keys (e.g. `a.b.c = "d"`, which creates a table `a`, containing a table `b`, containing the key `c` with the string value `d`), 
as well as quoted dotted keys (e.g. `a."b.c" = "d"`, which creates a table `a`, containing the key `b.c`, with the string value `d`).

Tomlet correctly handles both of these cases, but there is room for ambiguity in calls to `TomlTable#GetValue` and its sibling methods.

For ease of internal use, `GetValue` and `ContainsKey` will interpret keys as-above, with key names containing dots requiring the key name to be quoted. So a call to `ContainsKey("a.b")` will look for a table `a` containing a key `b`.
Note that, if you mistakenly do this, and there is a value mapped to the key `a` which is NOT a table, a `TomlContainsDottedKeyNonTableException` will be thrown.

However, for a more convenient API, calls to specific typed variants of `GetValue` (`GetString`, `GetInteger`, `GetDouble`, etc.) will assume keys are supposed to be quoted. That is, a call to
`GetString("a.b")` will look for a single key `a.b`, not a table `a` containing key `b`.

## Usage

### Serialize a runtime object to TOML

```c#
var myClass = new MyClass("hello world", 1, 3);
TomlDocument tomlDoc = TomletMain.DocumentFrom(myClass); //TOML document representation. Can be serialized using the SerializedValue property.
string tomlString = TomletMain.TomlStringFrom(myClass); //Formatted TOML string. Equivalent to TomletMain.DocumentFrom(myClass).SerializedValue
```

### Deserialize TOML to a runtime object

```c#
string myString = GetTomlStringSomehow(); //Web request, read file, etc.
var myClass = TomletMain.To<MyClass>(myString); //Requires a public, zero-argument constructor on MyClass.
Console.WriteLine(myClass.configurationFileVersion); //Or whatever properties you define.
```

### Change what name Tomlet uses to de/serialize a field

Given that you had the above setup and were serializing a class using `TomletMain.TomlStringFrom(myClass)`, you could override TOML key names like so:

```c#
class MyClass {
    [TomlProperty("name")] //Tell Tomlet to use "name" as the key, instead of "Username", when serializing and deserializing this type.
    public string Username { get; set; }
    [TomlProperty("password")] //Tell tomlet to use the lowercase "password" rather than "Password"
    public string Password { get; set; }
}
```

### Comments

Comments are parsed and stored alongside their corresponding values, where possible. Every instance of `TomlValue`
has a `Comments` property, which contains both the "inline" and "preceding" comments. Precending comments are
the comments that appear before the value (and therefore can span multiple lines), and inline comments are
the comments that appear on the same line as the value (and thus must be a single line).

Any preceding comment which is not associated with a value (i.e. it is placed after the last value) will be
stored in the `TrailingComment` property of the TOML document itself, and will be re-serialized from there.

If you're using Tomlet's reflective serialization feature (i.e. `TomletMain.____From`), you can use the `TomlInlineComment` and `TomlPrecedingComment`
attributes on fields or properties to specify the respective comments.

### Parse a TOML File

`TomlParser.ParseFile` is a utility method to parse an on-disk toml file. This just uses File.ReadAllText, so I/O errors will be thrown up to your calling code.

```c#
TomlDocument document = TomlParser.ParseFile(@"C:\MyFile.toml");

//You can then convert this document to a runtime object, if you so desire.
var myClass = TomletMain.To<MyClass>(document);
```

### Parse Arbitrary TOML input
Useful for parsing e.g. the response of a web request.
```c#
TomlParser parser = new TomlParser();
TomlDocument document = parser.Parse(myTomlString);
```

### Creating your own mappers.

By default, serialization will call ToString on an object, and deserialization will piece together an object field-by-field using reflection, excluding fields marked as 
`[NonSerialized]`, and using a parameterless constructor to instantiate the object. 

This approach should work for most model classes, but should something more complex be used, such as storing a colour as an integer/hex string, or if you have a more compact/proprietary
method of serializing your classes, then you can override this default using code such as this:

```c#
// Example: UnityEngine.Color stored as an integer in TOML. There is no differentiation between 32-bit and 64-bit integers, so we use TomlLong.
TomletMain.RegisterMapper<Color>(
        //Serializer (toml value from class) 
        color => new TomlLong(color.a << 24 | color.r << 16 | color.g << 8 | color.b),
        //Deserializler (class from toml value)
        tomlValue => {
            if(!(tomlValue is TomlLong tomlLong)) 
                throw new TomlTypeMismatchException(typeof(TomlLong), tomlValue.GetType(), typeof(Color))); //Expected type, actual type, context (type being deserialized)
            
            int a = tomlLong.Value >> 24 & 0xFF;
            int r = tomlLong.Value >> 16 & 0xFF;
            int g = tomlLong.Value >> 8 & 0xFF;
            int b = tomlLong.Value & 0xFF;
            
            return new Color(r, g, b, a); 
        }
);
```

Calls to `TomletMain.RegisterMapper` can specify either the serializer or deserializer as `null`, in which case the default (de)serializer will be used.

### Retrieve data from a TomlDocument

```c#
TomlDocument document; // See above for how to obtain one.
int someInt = document.GetInteger("myInt");
string someString = document.GetString("myString");

// TomlArray implements IEnumerable<TomlValue>, so you can iterate over it or use LINQ.
foreach(var value in document.GetArray("myArray")) {
    Console.WriteLine(value.StringValue);
}

//It also has an index operator, so you can do this
Console.WriteLine(document.GetArray("myArray")[0]);

List<string> strings = document.GetArray("stringArray").Select(v => (TomlString) v).ToList();

//Retrieving sub-tables. TomlDocument is just a special TomlTable, so you can 
//call GetSubTable on the resulting TomlTable too.
string subTableString = document.GetSubTable("myTable").GetString("aString");

//Date-Time objects. There's no API for these (yet)
var dateValue = document.GetValue("myDateTime");
if(dateValue is TomlOffsetDateTime tomlODT) {
    DateTimeOffset date = tomlODT.Value;
    Console.WriteLine(date); //27/05/1979 00:32:00 -07:00
} else if(dateValue is TomlLocalDateTime tomlLDT) {
    DateTime date = tomlLDT.Value;
    Console.WriteLine(date.ToUniversalTime()); //27/05/1979 07:32:00
} else if(dateValue is TomlLocalDate tomlLD) {
    DateTime date = tomlLD.Value;
    Console.WriteLine(date.ToUniversalTime()); //27/05/1979 00:00:00
} else if(dateValue is TomlLocalTime tomlLT) {
    TimeSpan time = tomlLT.Value;
    Console.WriteLine(time); //07:32:00.9999990
} 
```