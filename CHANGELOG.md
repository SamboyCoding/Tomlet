# Tomlet Changelog

## Contents

- [v1.1.1](#111)
- [v1.1.0](#110)
- [v1.0.2](#102)
- [v1.0.1](#101)
- [v1.0.0](#100)

## 1.1.1

- Fix more API holes by adding non-generic variants of TomletMain.ValueFrom, TomletMain.DocumentFrom, and TomletMain.TomlStringFrom.

## 1.1.0

- Support for enums (serialized and deserialized by name).
  - Includes a new Exception, `TomlEnumParseException`, for if an enum name cannot be resolved to a value when deserializing.
- Support for Arrays of complex types.
- Filled an API gap by adding `TomlDocument.CreateEmpty`
- Renamed Tomlet to TomletMain to avoid namespace/type conflict

## 1.0.2

- Add support for .NET Framework 3.5. 

## 1.0.1

- Small changes to structure for NuGet

## 1.0.0

- Initial Release
