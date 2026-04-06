# FileAssertJsonAssert Design

## Overview

The `FileAssertJsonAssert` class attempts to parse a matched file as a JSON document using
`System.Text.Json.JsonDocument.Parse`. If parsing fails, an error is reported and no further
assertions are evaluated. Otherwise it evaluates each dot-notation path against the JSON
element tree and applies min, max, and exact count constraints to the number of matching
elements.

## Class Structure

### FileAssertQueryAssert

`FileAssertQueryAssert` is the shared inner modeling class documented in
[FileAssertXmlAssert](file-assert-xml-assert.md). It holds the query string and count
constraints for a single structured-document query assertion and is shared across all four
structured-document assert units (XML, HTML, YAML, JSON).

### FileAssertJsonAssert

The main class coordinating dot-notation path assertions for a JSON file.

#### FileAssertJsonAssert Properties

| Property  | Type                                   | Description                         |
| :-------- | :------------------------------------- | :---------------------------------- |
| `Queries` | `IReadOnlyList<FileAssertQueryAssert>` | Dot-notation path query assertions. |

#### FileAssertJsonAssert Factory

```csharp
internal static FileAssertJsonAssert Create(IEnumerable<FileAssertQueryData> data)
```

#### FileAssertJsonAssert Run

```csharp
internal void Run(Context context, string fileName)
```

Execution proceeds in the following steps:

1. Reads the file content and calls `JsonDocument.Parse`.
2. If a `JsonException` is thrown, writes the error below and returns immediately.
3. For each query assertion: traverses the JSON element tree following the dot-notation path
   segments, counts the matched properties or array elements, and calls `queryAssert.Apply`.

#### FileAssertJsonAssert Parse Error Message

```text
File '<fileName>' could not be parsed as a JSON document
```

## YAML Configuration

```yaml
files:
  - pattern: "appsettings.json"
    json:
      - query: "ConnectionStrings"
        min: 1
      - query: "Logging.LogLevel"
        count: 1
      - query: "AllowedHosts"
        count: 1
```

## Design Decisions

- **No additional dependencies**: `System.Text.Json` is part of the .NET BCL, so JSON
  assertions require no additional NuGet packages.
- **Immediate failure on parse error**: Attempting to traverse a JSON document tree against
  a file that is not valid JSON would produce meaningless or misleading results. Reporting
  the parse failure immediately gives users a clear, actionable error message.
- **Dot-notation path traversal**: Segment-by-segment descent through JSON object properties.
  Array elements are counted at the terminal segment, allowing users to assert the presence
  and cardinality of array-valued keys.
- **Shared `FileAssertQueryAssert`**: The inner query-assert class is shared across all four
  structured-document assert units (XML, HTML, YAML, JSON), ensuring consistent error
  messages and constraint logic across formats.
