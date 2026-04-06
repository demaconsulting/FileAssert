# FileAssertHtmlAssert Design

## Overview

The `FileAssertHtmlAssert` class attempts to parse a matched file as an HTML document using
`HtmlAgilityPack`. If parsing produces critical errors, an error is reported and no further
assertions are evaluated. Otherwise it evaluates each XPath query against the document and
applies min, max, and exact count constraints to the number of matching nodes.

## Class Structure

### FileAssertHtmlAssert

The main class coordinating XPath-based node count assertions for an HTML file.

#### FileAssertHtmlAssert Properties

| Property  | Type                                 | Description             |
| :-------- | :----------------------------------- | :---------------------- |
| `Queries` | `IReadOnlyList<FileAssertHtmlQuery>` | XPath query assertions. |

Each `FileAssertHtmlQuery` entry holds:

| Property | Type     | Description                      |
| :------- | :------- | :------------------------------- |
| `Query`  | `string` | XPath expression to evaluate.    |
| `Count`  | `int?`   | Exact number of matched nodes.   |
| `Min`    | `int?`   | Minimum number of matched nodes. |
| `Max`    | `int?`   | Maximum number of matched nodes. |

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
3. For each query entry: selects nodes via
   `HtmlDocument.DocumentNode.SelectNodes(xpathQuery)`, counts the result, and applies
   `Count`, `Min`, and `Max` constraints against the match count.

#### FileAssertHtmlAssert Parse Error Message

```text
File '<fileName>' could not be parsed as an HTML document
```

#### FileAssertHtmlAssert Query Error Messages

```text
File '<fileName>' query '<query>' returned <n> result(s) which is below the minimum of <Min>
File '<fileName>' query '<query>' returned <n> result(s) which exceeds the maximum of <Max>
File '<fileName>' query '<query>' returned <n> result(s) but expected exactly <Count>
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
- **Independent query model**: `FileAssertHtmlQuery` is private to this unit so that HTML
  assertion behaviour can evolve independently of the other structured-document assert units.
