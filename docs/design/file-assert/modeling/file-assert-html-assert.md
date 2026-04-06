# FileAssertHtmlAssert Design

## Overview

The `FileAssertHtmlAssert` class attempts to parse a matched file as an HTML document using
`HtmlAgilityPack`. If parsing produces critical errors, an error is reported and no further
assertions are evaluated. Otherwise it evaluates each XPath query against the document and
applies min, max, and exact count constraints to the number of matching nodes.

## Class Structure

### FileAssertQueryAssert

`FileAssertQueryAssert` is the shared inner modeling class documented in
[FileAssertXmlAssert](file-assert-xml-assert.md). It holds the query string and count
constraints for a single structured-document query assertion and is shared across all four
structured-document assert units (XML, HTML, YAML, JSON).

### FileAssertHtmlAssert

The main class coordinating XPath-based node count assertions for an HTML file.

#### FileAssertHtmlAssert Properties

| Property  | Type                                   | Description             |
| :-------- | :------------------------------------- | :---------------------- |
| `Queries` | `IReadOnlyList<FileAssertQueryAssert>` | XPath query assertions. |

#### FileAssertHtmlAssert Factory

```csharp
internal static FileAssertHtmlAssert Create(IEnumerable<FileAssertQueryData> data)
```

#### FileAssertHtmlAssert Run

```csharp
internal void Run(Context context, string fileName)
```

Execution proceeds in the following steps:

1. Loads the file using `HtmlDocument.Load`.
2. If `ParseErrors` contains critical errors, writes the error below and returns
   immediately.
3. For each query assertion: selects nodes via
   `HtmlDocument.DocumentNode.SelectNodes(xpathQuery)`, counts the result, and calls
   `queryAssert.Apply`.

#### FileAssertHtmlAssert Parse Error Message

```text
File '<fileName>' could not be parsed as an HTML document
```

## YAML Configuration

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

## Design Decisions

- **HtmlAgilityPack chosen**: HtmlAgilityPack is the de-facto standard for lenient HTML
  parsing in .NET. It handles malformed HTML gracefully, making it appropriate for
  asserting generated documentation and static site outputs that may not be strict XHTML.
- **Immediate failure on critical parse errors**: When HtmlAgilityPack reports critical
  parse errors, applying XPath assertions would produce meaningless results. Reporting the
  parse failure immediately gives users a clear, actionable error message.
- **Shared `FileAssertQueryAssert`**: The inner query-assert class is shared across all four
  structured-document assert units (XML, HTML, YAML, JSON), ensuring consistent error
  messages and constraint logic across formats.
