# FileAssertYamlAssert Design

## Overview

The `FileAssertYamlAssert` class attempts to parse a matched file as a YAML document using
`YamlDotNet`. If parsing fails, an error is reported and no further assertions are evaluated.
Otherwise it evaluates each dot-notation path against the YAML document tree and applies min,
max, and exact count constraints to the number of matching nodes.

## Class Structure

### FileAssertYamlAssert

The main class coordinating dot-notation path assertions for a YAML file.

#### FileAssertYamlAssert Properties

| Property  | Type                                 | Description                         |
| :-------- | :----------------------------------- | :---------------------------------- |
| `Queries` | `IReadOnlyList<FileAssertYamlQuery>` | Dot-notation path query assertions. |

Each `FileAssertYamlQuery` entry holds:

| Property | Type     | Description                      |
| :------- | :------- | :------------------------------- |
| `Query`  | `string` | Dot-notation path to evaluate.   |
| `Count`  | `int?`   | Exact number of matched nodes.   |
| `Min`    | `int?`   | Minimum number of matched nodes. |
| `Max`    | `int?`   | Maximum number of matched nodes. |

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
3. For each query entry: traverses the YAML document tree following the dot-notation
   path segments, counts the matched nodes, and applies `Count`, `Min`, and `Max`
   constraints against the match count.

#### FileAssertYamlAssert Parse Error Message

```text
File '<fileName>' could not be parsed as a YAML document
```

#### FileAssertYamlAssert Query Error Messages

```text
File '<fileName>' query '<query>' returned <n> result(s) which is below the minimum of <Min>
File '<fileName>' query '<query>' returned <n> result(s) which exceeds the maximum of <Max>
File '<fileName>' query '<query>' returned <n> result(s) but expected exactly <Count>
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
- **Independent query model**: `FileAssertYamlQuery` is private to this unit so that YAML
  assertion behavior can evolve independently of the other structured-document assert units.
