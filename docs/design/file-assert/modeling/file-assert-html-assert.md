### FileAssertHtmlAssert Design

#### Overview

The `FileAssertHtmlAssert` class attempts to parse a matched file as an HTML document using
`HtmlAgilityPack`. If loading the file fails due to I/O or permission errors, an error is
reported and no further assertions are evaluated. Otherwise it evaluates each XPath query
against the document and applies min, max, and exact count constraints to the number of
matching nodes.

#### Class Structure

##### FileAssertHtmlAssert

The main class coordinating XPath-based node count assertions for an HTML file.

###### FileAssertHtmlAssert Properties

| Property  |
| :-------- |
| `Queries` |

Each `HtmlQuery` entry holds:

| Property |
| :------- |
| `Query`  |
| `Count`  |
| `Min`    |
| `Max`    |

###### FileAssertHtmlAssert Factory

```csharp
internal static FileAssertHtmlAssert Create(IEnumerable<FileAssertQueryData> data)
```

###### FileAssertHtmlAssert Run

```csharp
internal void Run(Context context, string fileName)
```

Execution proceeds in the following steps:

1. Loads the file using `HtmlDocument.Load`.
2. If `Load` throws an `IOException` or `UnauthorizedAccessException`, writes the
   error below and returns immediately.
3. For each query entry: selects nodes via
   `HtmlDocument.DocumentNode.SelectNodes(xpathQuery)`, counts the result, and applies
   `Count`, `Min`, and `Max` constraints against the match count.

###### FileAssertHtmlAssert Parse Error Message

```text
File '<fileName>' could not be parsed as an HTML document
```

###### FileAssertHtmlAssert Query Error Messages

```text
File '<fileName>' query '<query>' returned <n> result(s) which is below the minimum of <Min>
File '<fileName>' query '<query>' returned <n> result(s) which exceeds the maximum of <Max>
File '<fileName>' query '<query>' returned <n> result(s) but expected exactly <Count>
```

#### YAML Configuration

```yaml
files:
  - pattern: "docs/**/*.html"
    html:
      - query: "//title"
        count: 1
      - query: "//h1"
        min: 1
      - query: "//a[@href]"
        min: 1
```

#### Design Decisions

- **HtmlAgilityPack chosen**: HtmlAgilityPack is the de-facto standard for lenient HTML
  parsing in .NET. It handles malformed HTML gracefully, making it appropriate for
  asserting generated documentation and static site outputs that may not be strict XHTML.
- **Immediate failure on I/O errors**: HtmlAgilityPack is lenient and parses malformed
  HTML without throwing exceptions. The primary error condition handled is a missing or
  inaccessible file; `IOException` and `UnauthorizedAccessException` from `Load` are
  caught and reported immediately so users receive a clear, actionable error message
  rather than silent XPath results against an empty document.
- **Independent query model**: `HtmlQuery` is a private nested record in this unit so that HTML
  assertion behavior can evolve independently of the other structured-document assert units.

#### Purpose

`FileAssertHtmlAssert` is responsible for validating one HTML file against a list of XPath
queries. It parses the file with HtmlAgilityPack and enforces min, max, and exact node-count
constraints per query.

#### Data Model

| Field / Property |
| :--------------- |
| `Queries`        |

Each `HtmlQuery` (private nested record) holds:

| Property |
| :------- |
| `Query`  |
| `Count`  |
| `Min`    |
| `Max`    |

#### Key Methods

| Method                                          |
| :---------------------------------------------- |
| `Create(IEnumerable<FileAssertQueryData> data)` |
| `Run(Context context, string fileName)`         |

#### Error Handling

| Scenario                                                                 |
| :----------------------------------------------------------------------- |
| `IOException` or `UnauthorizedAccessException` while loading the file    |
| Query XPath expression is invalid (`XPathException`)                     |
| Query result below `Min`                                                 |
| Query result above `Max`                                                 |
| Query result not equal to `Count`                                        |

#### Interactions

- **Caller**: `FileAssertFile.Run` calls `HtmlAssert.Run(context, fileName)` when the `html:`
  assertion block is declared.
- **Created by**: `FileAssertFile.Create` via `FileAssertHtmlAssert.Create`.
- **OTS dependency**: `HtmlAgilityPack.HtmlDocument` for lenient HTML parsing and
  `HtmlDocument.DocumentNode.SelectNodes` for XPath evaluation.
- **Configuration dependency**: `FileAssertQueryData` DTOs from the Configuration subsystem.
