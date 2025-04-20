using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tomlet;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace TomlTestEncoder;

class Program
{
    static int Main(string[] args)
    {
        //Read TOML data from stdin
        using var reader = new StreamReader(Console.OpenStandardInput());
        var tomlString = reader.ReadToEnd();

        TomlDocument doc;
        try
        {
            doc = new TomlParser().Parse(tomlString);
        }
        catch (TomlException e)
        {
            Console.Error.WriteLine(e);
            return 1;
        }
        
        //Convert to JSON
        var jsonRoot = new JsonObject();
        
        AppendTableEntriesToJson(jsonRoot, doc);
        
        //Write JSON to stdout
        Console.WriteLine(jsonRoot.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        }));
        
        return 0;
    }
    
    private static void AppendTableEntriesToJson(JsonObject json, TomlTable table)
    {
        foreach (var entry in table)
        {
            if (entry.Value is TomlTable subTable)
            {
                var subJson = new JsonObject();
                AppendTableEntriesToJson(subJson, subTable);
                json[entry.Key] = subJson;
            } else if (entry.Value is TomlArray array)
            {
                var jsonArray = new JsonArray();
                AppendArrayEntriesToJson(jsonArray, array);
                json[entry.Key] = jsonArray;
            }
            else
            {
                json[entry.Key] = MakeJsonObjectForTomlPrimitive(entry.Value);
            }
        }
    }

    private static void AppendArrayEntriesToJson(JsonArray json, TomlArray array)
    {
        foreach (var entry in array)
        {
            if (entry is TomlTable subTable)
            {
                var subJson = new JsonObject();
                AppendTableEntriesToJson(subJson, subTable);
                json.Add(subJson);
            }
            else if (entry is TomlArray subArray)
            {
                var jsonArray = new JsonArray();
                AppendArrayEntriesToJson(jsonArray, subArray);
                json.Add(jsonArray);
            }
            else
            {
                json.Add(MakeJsonObjectForTomlPrimitive(entry));
            }
        }
    }

    private static JsonObject MakeJsonObjectForTomlPrimitive(TomlValue value)
    {
        if(value is TomlTable or TomlArray)
            throw new ArgumentException("Cannot convert TomlTable or TomlArray to JsonObject directly.");
        
        var json = new JsonObject();
        json["type"] = value switch
        {
            TomlLong => "integer",
            TomlDouble => "float",
            TomlString => "string",
            TomlBoolean => "bool",
            TomlOffsetDateTime => "datetime",
            TomlLocalDateTime => "datetime-local",
            TomlLocalDate => "date-local",
            TomlLocalTime => "time-local",
            _ => throw new ArgumentException($"Unsupported TomlValue type: {value.GetType()}")
        };

        json["value"] = value switch
        {
            TomlLong longValue => longValue.Value.ToString(),
            TomlDouble doubleValue => FormatDouble(doubleValue.Value),
            TomlString stringValue => stringValue.Value,
            TomlBoolean boolValue => boolValue.Value.ToString().ToLowerInvariant(),
            TomlOffsetDateTime dateTimeValue => ToRfc3339(dateTimeValue),
            TomlLocalDateTime localDateTimeValue => ToRfc3339(localDateTimeValue),
            TomlLocalDate localDateValue => ToRfc3339(localDateValue),
            TomlLocalTime localTimeValue => ToRfc3339(localTimeValue),
            _ => throw new ArgumentException($"Unsupported TomlValue type: {value.GetType()}")
        };
        
        return json;
    }

    private static string FormatDouble(double d) => d switch
    {
        double.PositiveInfinity => "inf",
        double.NegativeInfinity => "-inf",
        double.NaN => "nan",
        _ => d.ToString(CultureInfo.InvariantCulture)
    };
    
    private static string ToRfc3339(TomlOffsetDateTime dateTime) => dateTime.Value.ToString("yyyy-MM-ddTHH:mm:ssK");
    
    private static string ToRfc3339(TomlLocalDateTime dateTime) => dateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss");
    
    private static string ToRfc3339(TomlLocalDate date) => date.Value.ToString("yyyy-MM-dd");
    
    private static string ToRfc3339(TomlLocalTime time) => time.Value.ToString("hh\\:mm\\:ss");
}