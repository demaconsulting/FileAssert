# FileAssertXmlAssert Design

## Overview

The `FileAssertXmlAssert` class attempts to parse a matched file as an XML document using
`System.Xml.Linq` (`XDocument.Load`). If parsing fails, an error is reported and no further
assertions are evaluated. Otherwise it evaluates each XPath query against the document and
applies min, max, and exact count constraints to the number of matching nodes.

## Class Structure

### FileAssertQueryAssert

An inner modeling class shared across `FileAssertXmlAssert`, `FileAssertHtmlAssert`,
`FileAssertYamlAssert`, and `FileAssertJsonAssert`. It holds the query string and count
constraints for a single structured-document query assertion.

#### FileAssertQueryAssert Properties

| Property | Type     | Description                            |
| :------- | :------- | :------------------------------------- |
| `Query`  | `string` | XPath expression or dot-notation path. |
| `Count`  | `int?`   | Exact number of matched nodes.         |
| `Min`    | `int?`   | Minimum number of matched nodes.       |
| `Max`    | `int?`   | Maximum number of matched nodes.       |

#### FileAssertQueryAssert Factory

```csharp
internal static FileAssertQueryAssert Create(FileAssertQueryData data)
```

#### FileAssertQueryAssert Apply

```csharp
internal void Apply(Context context, string fileName, string query, int matchCount)
```

Reports an error if `matchCount < Min`, `matchCount > Max`, or `matchCount != Count`.

#### FileAssertQueryAssert Error Messages

```text
File '<fileName>' query '<query>' returned <n> result(s) which is below the minimum of <Min>
File '<fileName>' query '<query>' returned <n> result(s) which exceeds the maximum of <Max>
File '<fileName>' query '<query>' returned <n> result(s) but expected exactly <Count>
```

### FileAssertXmlAssert

The main class coordinating XPath-based node count assertions for an XML file.

#### FileAssertXmlAssert Properties

| Property  | Type                                   | Description             |
| :-------- | :------------------------------------- | :---------------------- |
| `Queries` | `IReadOnlyList<FileAssertQueryAssert>` | XPath query assertions. |

#### FileAssertXmlAssert Factory

```csharp
internal static FileAssertXmlAssert Create(IEnumerable<FileAssertQueryData> data)
```

#### FileAssertXmlAssert Run

```csharp
internal void Run(Context context, string fileName)
```

Execution proceeds in the following steps:

1. Attempts to load the file using `XDocument.Load(fileName)`.
2. If an exception is thrown, writes the error below and returns immediately.
3. For each query assertion: evaluates the XPath expression against the document using
   `System.Xml.XPath` extension methods, counts the matching nodes, and calls
   `queryAssert.Apply`.

#### FileAssertXmlAssert Parse Error Message

```text
File '<fileName>' could not be parsed as an XML document
```

## YAML Configuration

```yaml
files:
  - pattern: "**/*.xml"
    xml:
      - query: "//dependency"
        min: 1
      - query: "//plugin[artifactId='maven-compiler-plugin']"
        count: 1
```

## Design Decisions

- **No additional dependencies**: `System.Xml.Linq` and `System.Xml.XPath` are part of the
  .NET BCL, so XML assertions require no additional NuGet packages.
- **Immediate failure on parse error**: Attempting to evaluate XPath queries against a file
  that is not valid XML would produce meaningless or misleading results. Reporting the parse
  failure immediately gives users a clear, actionable error message.
- **Shared `FileAssertQueryAssert`**: The inner query-assert class is shared by all four
  structured-document assert units (XML, HTML, YAML, JSON), ensuring consistent error
  messages and constraint logic across formats.
