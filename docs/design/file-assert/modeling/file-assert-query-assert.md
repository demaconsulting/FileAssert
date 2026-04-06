# FileAssertQueryAssert Design

## Overview

`FileAssertQueryAssert` is a standalone internal class in the Modeling namespace that holds
the query string and count constraints for a single structured-document query assertion. It is
shared by `FileAssertXmlAssert`, `FileAssertHtmlAssert`, `FileAssertYamlAssert`, and
`FileAssertJsonAssert`, ensuring consistent error messages and constraint logic across all
structured-document assertion formats.

## Class Structure

### Properties

| Property | Type     | Description                            |
| :------- | :------- | :------------------------------------- |
| `Query`  | `string` | XPath expression or dot-notation path. |
| `Count`  | `int?`   | Exact number of matched nodes.         |
| `Min`    | `int?`   | Minimum number of matched nodes.       |
| `Max`    | `int?`   | Maximum number of matched nodes.       |

### Factory Method

```csharp
internal static FileAssertQueryAssert Create(FileAssertQueryData data)
```

The factory validates that `Query` is not null or whitespace. At least one of `Count`, `Min`,
or `Max` must be set; if none is provided the factory throws `InvalidOperationException`.

### Apply Method

```csharp
internal void Apply(Context context, string fileName, string query, int matchCount)
```

Applies the count constraints to the number of matched nodes reported by the caller:

- If `Min` is set and `matchCount < Min`, an error is written.
- If `Max` is set and `matchCount > Max`, an error is written.
- If `Count` is set and `matchCount != Count`, an error is written.

### Error Messages

```text
File '<fileName>' query '<query>' returned <n> result(s) which is below the minimum of <Min>
File '<fileName>' query '<query>' returned <n> result(s) which exceeds the maximum of <Max>
File '<fileName>' query '<query>' returned <n> result(s) but expected exactly <Count>
```

## Design Decisions

- **Standalone class**: `FileAssertQueryAssert` is a standalone internal class rather than a
  nested type so that it can be referenced directly by all four structured-document assert
  units without cross-type nesting dependencies.
- **Caller provides match count**: The `Apply` method receives the pre-computed match count
  from the calling assert unit rather than performing the query itself. This keeps the query
  evaluation logic in the format-specific assert classes and the constraint logic in this
  shared class, following the single-responsibility principle.
- **At-least-one constraint enforced at creation**: Requiring at least one of `Count`, `Min`,
  or `Max` prevents silent no-op query assertions that would pass vacuously without checking
  anything.
