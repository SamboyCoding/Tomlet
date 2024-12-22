# Tomlet Changelog

## Contents

- [Tomlet Changelog](#tomlet-changelog)
  - [Contents](#contents)
  - [6.0.0](#600)
  - [5.4.0](#540)
  - [5.3.1](#531)
  - [5.3.0](#530)
  - [5.2.0](#520)
  - [5.1.3](#513)
  - [5.1.2](#512)
  - [5.1.1](#511)
  - [5.1.0](#510)
  - [5.0.1](#501)
  - [5.0.0](#500)
  - [4.0.0](#400)
  - [3.2.2](#322)
  - [3.2.1](#321)
  - [3.2.0](#320)
  - [3.1.4](#314)
  - [3.1.3](#313)
  - [3.1.2](#312)
  - [3.1.1](#311)
  - [3.1.0](#310)
  - [3.0.1](#301)
  - [3.0.0](#300)
  - [2.2.0](#220)
  - [2.1.0](#210)
  - [2.0.2](#202)
  - [2.0.1](#201)
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

## 6.0.0

- Adds .NET trimming compatibility and annotations to hopefully prevent classes that are passed to Tomlet from having their members stripped.
- Makes behavior of passing `null` to methods in `TomletMain` more consistent - they will all now return null if you pass null, rather than some throwing, some returning null, and some returning an empty table/doc.
  - This is a breaking change, hence the bump to 6.0.0. 
- Implements deserialization of dictionaries using primitives as keys. Thanks to [@Sprixitite](https://github.com/Sprixitite) in [#44](https://github.com/SamboyCoding/Tomlet/pull/44)
- Adds an option to ignore invalid enum values (i.e. an invalid name) when deserializing. 

## 5.4.0

- Adds an option to ignore non-public members when de/serializing. Thanks to [@Sprixitite](https://github.com/Sprixitite) in [#43](https://github.com/SamboyCoding/Tomlet/pull/43)

## 5.3.1

- Fixes an issue introduced by [#33](https://github.com/SamboyCoding/Tomlet/pull/33) which caused some keys (e.g. in table array names) to be erroneously quoted when they shouldn't have been, resulting in breaking round-trip functionality. Reported as [#40](https://github.com/SamboyCoding/Tomlet/issues/40)

## 5.3.0

- Added `TomletMain.To(Type, string, TomlSerializerOptions)` as a wrapper for parse-then-deserialize to fill an API hole.
- Added `TomlSerializationMethods.GetDefaultSerializerForType` and `TomlSerializationMethods.GetDefaultDeserializerForType` to expose the default reflection-based de/serializers for use in custom serializers.
- Added support for de/serializing fields and properties of type `IEnumerable` - they will be treated as arrays. Based on work by [@tcortega](https://github.com/tcortega) in [#35](https://github.com/SamboyCoding/Tomlet/pull/32)

## 5.2.0

- Fixed keys with symbols not being quoted when they should be (issue [#30](https://github.com/SamboyCoding/Tomlet/issues/30)). Thanks to [@tcortega](https://github.com/tcortega) in [#33](https://github.com/SamboyCoding/Tomlet/pull/33)
- Fixed records not being supported due to them having constructors with params - those are now supported. Thanks to [@tcortega](https://github.com/tcortega) in [#32](https://github.com/SamboyCoding/Tomlet/pull/32)
- Tomlet is now compiled for .NET 7 in addition to the previous targets, and the tests are run against .NET 7.

## 5.1.3

- Improvements for ToString on TOML Values, and iterating over TOML Tables. Thanks to [johnkiddjr](https://github.com/johnkiddjr) in [#28](https://github.com/SamboyCoding/Tomlet/pull/28)

## 5.1.2

- Fixed an issue where the decimal separator `,` was used instead of the correct `.` in some cases. Thanks to [GeleArthur](https://github.com/GeleArthur) for the fix in [#22](https://github.com/SamboyCoding/Tomlet/pull/22)

## 5.1.1

- Fixed an issue where `[TomlDoNotInlineObject]` on properties didn't actually work. Thanks to [Windows10CE](https://github.com/Windows10CE) for the fix in [#20](https://github.com/SamboyCoding/Tomlet/pull/20)

## 5.1.0

- Enabled usage of `[TomlDoNotInlineObject]` on fields and properties, which will prevent the field/prop from serialized as an inline table.

## 5.0.1

- Fixes an exception when deserializing empty comments. Thanks to [BruceChenQAQ](https://github.com/BruceChenQAQ) for the fix in [#19](https://github.com/SamboyCoding/Tomlet/pull/19).

## 5.0.0

- Sorry for another breaking change so soon!
- This release adds proper support for `Nullable<T>` to Tomlet (see #2), meaning that Nullables without a value won't be serialized, and those with a value will be serialized as the value itself.
  - Technically this is breaking because it means any existing nullable values in a TOML file won't be deserialized properly.
  - As part of this, you can now return null from custom serializers, in which case the value will not be written to the TOML document.
- It also improves string serialization so that values that contain backslashes but no single quotes (e.g. Windows paths!) will be serialized as literal strings instead of basic strings.
  - Strings which contain match this criteria and additionally contain newlines will be serialized as multiline literals.

## 4.0.0

- Fundamentally rewrote the way properties are de/serialized, preventing weirdness with inheritance (see #17)
  - This introduces a **minor breaking change** for anonymous types (i.e. `new { Foo = "bar" }`), in that the property names are now correctly serialized (`Foo` instead of `<Foo>i__Field`)
  - It may also break if you were making assumptions about the way properties were serialized, but I don't have any specific examples.
  - As before, the `TomlPropertyAttribute` can be used to override the property name.
  - Also, deserializing an incorrect type to a property now throws a `TomlPropertyTypeMismatchException` instead of a `TomlFieldTypeMismatchException`.
- General code cleanup

## 3.2.2

- Fixed possible exception when serializing `Dictionary` objects.

## 3.2.1

- Fixed missing blank line when serializing a table-array inside a table.

## 3.2.0

- Support serialization of `Dictionary<T, U>` and deserialization of `Dictionary<string, T>`.

## 3.1.4

- Updated nuget package metadata to enable source link and deterministic builds, allowing you to verify the integrity of the build and see the source with comments in-ide.

## 3.1.3

- Fixed spacing issues with TomlTable. Thanks to [HerpDerpinstine](https://github.com/HerpDerpinstine) for PRing this fix in [#16](https://github.com/SamboyCoding/Tomlet/pull/16)!

## 3.1.2

- Fixed serialization of floating-point not-a-number (`nan`) and Infinity (`inf`) values and added a test for them.

## 3.1.1

- Added `Tomlet.Exceptions.TomlUnescapedUnicodeControlCharException` for if one of the forbidden unicode
  control characters is used without being escaped. See the TOML spec sections on comments and strings for
  full information.

## 3.1.0

- Added `Tomlet.Attributes.TomlDoNotInlineObjectAttribute` and `TomlTable#ForceNoInline`
  to disable inlining of simple tables
- Made TomlBoolean no longer a singleton class so that comments do not get duplicated to all true/false
  Toml values.
- Adjusted some formatting around table headers when serializing

## 3.0.1
- Hotfix for an issue where the new, preferred, inline serialization would create invalid TOML documents
  due to not quoting keys.

## 3.0.0
- BREAKING CHANGE: Moved TomlPropertyAttribute to the `Tomlet.Attributes` namespace.
- BREAKING CHANGE: Removed several exceptions that weren't ever actually thrown
  - `TomlTableArrayIntermediateNonTableException`
  - `UnterminatedTomlArrayException`
  - `UnterminatedTomlInlineObjectException`
- Added support for comments.
  - Added `Tomlet.Models.TomlCommentData`, exposed via the `Comments` field on any `TomlValue`.
  - Added `Tomlet.Attributes.TomlInlineCommentAttribute` and `Tomlet.Attributes.TomlPrecedingCommentAttribute` to allow specifying inline comments using reflection-based serialization.
  - Added `TomlNewlineInInlineCommentException`
  - Please see the readme for more details.
- Made inline serialization much more likely to be chosen for Toml Tables, assuming they have no comments, and that
  they contain only primitive values.
- Fixed handling of 8-digit unicode escape sequences.
- Generally cleaned up the codebase.
- TomlArray#Add and TomlTable#Put now handle you providing them an already-serialized TOML value instead of double-serializing.
- Finished adding tests for invalid input. Some very minor output changes could be possible, including:
  - Arrays now output a trailing comma. This is valid TOML and was a choice made to keep the code cleaner.

## 2.2.0

- Wrote some tests to test behavior around invalid input. As a result of this, some behavior has changed to bring it in line with the TOML spec. Notably:
  - Floating-point values with a decimal point followed immediately by an exponent (e.g. `3.e20`) are now correctly identified as invalid as per the TOML spec.
  - Handling around keys with whitespace has been clarified, with a new `TomlWhitespaceInKeyException`.
- Fixed a bug where hexadecimal integers containing an `e` were parsed as if the `e` indicated an exponent. Thanks to [packnslash](https://github.com/packnslash) for contributing this in [#15](https://github.com/SamboyCoding/Tomlet/pull/15)!
- Rewrote the way the parser parses keys in a TOML document to properly handle whitespace in dotted keys. I've tested this on a variety of TOML documents and it seems to work well, and all the unit tests pass, but it's possible some issues may occur.
- Added `GetLong` to `TomlTable` (and thus this is also available on `TomlDocument`).

## 2.1.0

- Added the ability to use `[TomlProperty("keyName")]` to override the name Tomlet uses to de/serialize a field to and from a Toml document.
  - Thanks to [Wulfheart](https://github.com/Wulfheart) for contributing this in [#13](https://github.com/SamboyCoding/Tomlet/pull/13)!
- Rewrote some of how class de/serialization works, so that `List<T>` and arrays of classes are better supported. Thank you to `@Violite` on discord for reporting this.

## 2.0.2

- Fixed string serializing still escaping characters when the string serializes to a literal (single-quoted) string.
- Added the ability for strings to serialize to multiline literals for readability under certain circumstances.
- Thanks to [ITR13](https://github.com/ITR13) for reporting the issues on my discord.

## 2.0.1

- Fixed support for multiple subtables. Thanks to [ITR13](https://github.com/ITR13) for the PR!

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
