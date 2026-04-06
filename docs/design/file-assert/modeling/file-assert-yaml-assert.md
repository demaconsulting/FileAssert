# FileAssertYamlAssert Design

## Overview

The `FileAssertYamlAssert` class attempts to parse a matched file as a YAML document using
`YamlDotNet`. If parsing fails, an error is reported and no further assertions are evaluated.
Otherwise it evaluates each dot-notation path against the YAML document tree and applies min,
max, and exact count constraints to the number of matching nodes.

## Class Structure

### FileAssertQueryAssert

`FileAssertQueryAssert` is the shared inner modeling class documented in
[FileAssertXmlAssert](file-assert-xml-assert.md). It holds the query string and count
constraints for a single structured-document query assertion and is shared across all four
structured-document assert units (XML, HTML, YAML, JSON).

### FileAssertYamlAssert

The main class coordinating dot-notation path assertions for a YAML file.

#### FileAssertYamlAssert Properties

| Property  | Type                                   | Description                         |
| :-------- | :------------------------------------- | :---------------------------------- |
| `Queries` | `IReadOnlyList<FileAssertQueryAssert>` | Dot-notation path query assertions. |

#### FileAssertYamlAssert Factory

```csharp
internal static FileAssertYamlAssert Create(IEnumerable<FileAssertQueryData> data)
```

#### FileAssertYamlAssert Run

```csharp
internal void Run(Context context, string fileName)
```

Execution proceeds in the following steps:

1. Parses the file using YamlDotNet's `YamlStream.Load`.
2. If a `YamlException` is thrown, writes the error below and returns immediately.
3. For each query assertion: traverses the YAML document tree following the dot-notation
   path segments, counts the matched nodes, and calls `queryAssert.Apply`.

#### FileAssertYamlAssert Parse Error Message

```text
File '<fileName>' could not be parsed as a YAML document
```

## YAML Configuration

```yaml
files:
  - pattern: ".fileassert.yaml"
    yaml:
      - query: "tests"
        min: 1
      - query: "tests.files"
        min: 1
      - query: "tests.tags"
        count: 3
```

## Design Decisions

- **YamlDotNet reuse**: YamlDotNet is already a project dependency for configuration
  deserialization. Reusing it for YAML assertions avoids adding a new library.
- **Immediate failure on parse error**: Attempting to traverse a YAML document tree against
  a file that is not valid YAML would produce meaningless or misleading results. Reporting
  the parse failure immediately gives users a clear, actionable error message.
- **Dot-notation path traversal**: Segment-by-segment descent through YAML mapping nodes.
  Sequences count as zero or more items at the terminal segment, allowing users to assert
  the presence and cardinality of sequence keys.
- **Shared `FileAssertQueryAssert`**: The inner query-assert class is shared across all four
  structured-document assert units (XML, HTML, YAML, JSON), ensuring consistent error
  messages and constraint logic across formats.
