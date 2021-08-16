# Tomlet Changelog

## Contents

- [Tomlet Changelog](#tomlet-changelog)
  - [Contents](#contents)
  - [2.0.0](#200)
  - [1.3.5](#135)
  - [1.3.4](#134)
  - [1.3.3](#133)
  - [1.3.2](#132)
  - [1.3.1](#131)
  - [1.3.0](#130)
  - [1.2.0](#120)
  - [1.1.2](#112)
  - [1.1.1](#111)
  - [1.1.0](#110)
  - [1.0.2](#102)
  - [1.0.1](#101)
  - [1.0.0](#100)

## 2.0.0

- Fix deserialization of empty strings in inline arrays (#9)
- Fix deserialization of escape sequences in double-quoted keys (#10)
- Updated TomlArray to serialize Table Arrays more intelligently, and update the exception message to be more useful to developers using Tomlet.
- Internal cleanup to remove compiler warnings, resulting in some breaking changes where member names did not meet the standard (hence the bump to 2.0):
  - `TomlEOFException` is now `TomlEndOfFileException`
  - `TomlBoolean.TRUE` and `TomlBoolean.FALSE` are now `TomlBoolean.True` and `TomlBoolean.False` 
  - `TomlString.EMPTY` is now `TomlString.Empty`
  - `TomlValueWithDateTime` is now `ITomlValueWithDateTime`
  - `TomletMain.ValueFrom<T>` and `TomletMain.DocumentFrom<T>` will now explicitly throw an `ArgumentNullException` if their argument is null.
  - `TomlNumberStyle` is now internal. This shouldn't matter because all its fields were internal anyway, but for completeness it is included here.

## 1.3.5

- Respect `new`-annotated fields. That is to say, use the most-subclass value, not any parent value, when serializing.

## 1.3.4

- Fix serialization of floats and doubles in cultures which use ',' as the decimal separator (much of Western Europe, at least) (#6)

## 1.3.3

- Fix deserialization of empty inline tables (https://github.com/SamboyCoding/Tomlet/pull/5). Thanks to https://github.com/ITR13 for the PR!

## 1.3.2

- Fix inability to serialize non-inline tables with spaces in their name.

## 1.3.1

- Bugfix for strings with newlines.
  
## 1.3.0

- Fixed key names for sub-tables in table arrays
- Add default serializers for bytes and shorts, and unsigned variants of all number types.

## 1.2.0

- Adds `TomlArray.Add<T>` and `TomlTable.Put<T>`
- Serializes arrays as multi-line if they're complicated.
- Make deserialization to floats and doubles accept long or int values in the TOML file.
- Fixed an issue with serializing table-arrays.
- Fixes support for newline characters or quotes in key names.

## 1.1.2

- Fix a regression in the ability to deserialize arrays.

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
