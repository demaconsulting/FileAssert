# FileAssertFile Design

## Overview

The `FileAssertFile` class locates files on disk using a glob pattern, enforces
optional minimum and maximum count constraints, and applies a collection of
`FileAssertRule` instances to the text content of every matched file.

## Class Structure

### Properties

| Property     | Type                             | Description                                       |
| :----------- | :------------------------------- | :------------------------------------------------ |
| `Pattern`    | `string`                         | Glob pattern used to locate files.                |
| `Min`        | `int?`                           | Optional minimum number of matching files.        |
| `Max`        | `int?`                           | Optional maximum number of matching files.        |
| `Count`      | `int?`                           | Optional exact number of matching files.          |
| `MinSize`    | `long?`                          | Optional minimum file size in bytes per file.     |
| `MaxSize`    | `long?`                          | Optional maximum file size in bytes per file.     |
| `Text`       | `IReadOnlyList<FileAssertRule>`  | Text content rules applied to every matched file. |
| `PdfAssert`  | `FileAssertPdfAssert?`           | PDF document assertions (null if not declared).   |
| `XmlAssert`  | `FileAssertXmlAssert?`           | XML node assertions (null if not declared).       |
| `HtmlAssert` | `FileAssertHtmlAssert?`          | HTML node assertions (null if not declared).      |
| `YamlAssert` | `FileAssertYamlAssert?`          | YAML node assertions (null if not declared).      |
| `JsonAssert` | `FileAssertJsonAssert?`          | JSON node assertions (null if not declared).      |

### Factory Method

```csharp
internal static FileAssertFile Create(FileAssertFileData data)
```

The factory validates that `Pattern` is not null or whitespace before constructing
the instance. Rules are created via `FileAssertRule.Create` for each entry in the
data's rule list.

### Execution Method

```csharp
internal void Run(Context context, string basePath)
```

Execution proceeds in five phases:

1. **File discovery** — `Microsoft.Extensions.FileSystemGlobbing.Matcher` evaluates
   `Pattern` relative to `basePath` and returns the list of matched file paths.

2. **Minimum count validation** — If `Min` is set and the match count is below it,
   an error is written and execution returns immediately.

3. **Maximum count validation** — If `Max` is set and the match count exceeds it,
   an error is written and execution returns immediately.

4. **Exact count validation** — If `Count` is set and the match count does not equal
   it, an error is written and execution returns immediately. Early return prevents
   misleading per-file errors when the count constraint already signals a failure.

5. **Per-file validation** — Each matched file is inspected individually:
   a. Validates size constraints (`MinSize`, `MaxSize`) using `FileInfo.Length`.
   b. If `Text` rules are defined, reads the file as text and applies each
      `FileAssertRule`.
   c. If `PdfAssert` is defined, attempts to parse the file using PdfPig; reports
      an immediate error if parsing fails, otherwise applies metadata, page, and
      body text assertions.
   d. If `XmlAssert` is defined, attempts to parse the file using `System.Xml.Linq`;
      reports an immediate error if parsing fails, otherwise applies XPath node count
      assertions.
   e. If `HtmlAssert` is defined, attempts to parse the file using HtmlAgilityPack;
      reports an immediate error if parsing fails, otherwise applies XPath node count
      assertions.
   f. If `YamlAssert` is defined, attempts to parse the file using YamlDotNet; reports
      an immediate error if parsing fails, otherwise applies dot-notation path count
      assertions.
   g. If `JsonAssert` is defined, attempts to parse the file using `System.Text.Json`;
      reports an immediate error if parsing fails, otherwise applies dot-notation path
      count assertions.

## YAML Configuration

```yaml
files:
  - pattern: "**/*.cs"
    min: 1
    max: 100
    count: 5
    min-size: 10
    max-size: 1048576
    text:
      - contains: "Copyright (c) DEMA Consulting"
```

All properties except `pattern` are optional.

## Design Decisions

- **Glob via FileSystemGlobbing**: The `Microsoft.Extensions.FileSystemGlobbing`
  library is already a project dependency and provides cross-platform glob support
  consistent with the rest of the .NET ecosystem.
- **Early return on count failure**: Reporting a count mismatch and stopping avoids
  cascading content-rule errors for files that should not exist at all.
- **Size checked before content**: File size is inspected before reading content to
  avoid unnecessary I/O when a size violation is already present.
- **Lazy content and document loading**: File content is read as text only when at
  least one text rule is defined. File-type parsing (PDF, XML, HTML, YAML, JSON) is
  attempted only when the corresponding assertion block is declared, avoiding
  unnecessary I/O and third-party library invocations.
