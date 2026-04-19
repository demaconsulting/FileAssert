# FileAssertPdfAssert Design

## Overview

The `FileAssertPdfAssert` class attempts to parse a matched file as a PDF document using
PdfPig. If parsing fails, an error is reported and no further assertions are evaluated.
Otherwise it applies metadata field assertions, page count constraints, and body text rules.

## Class Structure

### PdfMetadataRule

An inner modeling class that applies a single PDF metadata field assertion.

#### PdfMetadataRule Properties

| Property   | Type      | Description                                        |
| :--------- | :-------- | :------------------------------------------------- |
| `Field`    | `string`  | Metadata field name (e.g. Title, Author, Subject). |
| `Contains` | `string?` | Metadata value must contain this substring.        |
| `Matches`  | `string?` | Metadata value must match this regular expression. |

#### PdfMetadataRule Factory

```csharp
internal static PdfMetadataRule FromData(FileAssertPdfMetadataRuleData data)
```

#### PdfMetadataRule Apply

```csharp
internal void Apply(Context context, string fileName, string? fieldValue)
```

Checks `Contains` substring presence (ordinal) and `Matches` regex against `fieldValue`.

#### PdfMetadataRule Error Messages

```text
File '<fileName>' PDF metadata '<Field>' does not contain '<Contains>'
File '<fileName>' PDF metadata '<Field>' does not match '<Matches>'
```

### PdfPages

An inner modeling class that enforces page count constraints.

#### PdfPages Properties

| Property | Type   | Description              |
| :------- | :----- | :----------------------- |
| `Min`    | `int?` | Minimum number of pages. |
| `Max`    | `int?` | Maximum number of pages. |

#### PdfPages Factory

```csharp
internal static PdfPages FromData(FileAssertPdfPagesData data)
```

#### PdfPages Apply

```csharp
internal void Apply(Context context, string fileName, int n)
```

Reports an error if `n < Min` or `n > Max`.

#### PdfPages Error Messages

```text
File '<fileName>' PDF has <n> page(s) which is below the minimum of <Min>
File '<fileName>' PDF has <n> page(s) which exceeds the maximum of <Max>
```

### FileAssertPdfAssert

The main class coordinating metadata, page count, and body text assertions for a PDF file.

#### FileAssertPdfAssert Properties

| Field       | Type                              | Description                                    |
| :---------- | :-------------------------------- | :--------------------------------------------- |
| `_metadata` | `IReadOnlyList<PdfMetadataRule>`  | Metadata field assertions.                     |
| `_pages`    | `PdfPages?`                       | Page count constraints (null if not declared). |
| `_text`     | `IReadOnlyList<FileAssertRule>`   | Body text rules.                               |

#### FileAssertPdfAssert Factory

```csharp
internal static FileAssertPdfAssert Create(FileAssertPdfData data)
```

Creates metadata rules, page constraints, and text rules from the DTO.

#### FileAssertPdfAssert Run

```csharp
internal void Run(Context context, string fileName)
```

Execution proceeds in the following steps:

1. Attempts to open the file as a PDF using `PdfDocument.Open`.
2. If an exception is thrown, writes the error below and returns immediately.
3. Applies each metadata rule for each declared field.
4. If `Pages` is defined, applies page count assertions.
5. If `Text` rules are defined, extracts page text via PdfPig, concatenates, and applies
   each rule.

#### FileAssertPdfAssert Parse Error Message

```text
File '<fileName>' could not be parsed as a PDF document
```

#### FileAssertPdfAssert GetMetadataField

```csharp
private static string? GetMetadataField(PdfDocument document, string field)
```

Maps a field name string to the corresponding `DocumentInformation` property on `PdfDocument`.
Returns `null` for unrecognized field names or when the property value is not set.

Recognized field names:

| Field Name  | DocumentInformation Property          |
| :---------- | :------------------------------------ |
| `Title`     | `document.Information.Title`          |
| `Author`    | `document.Information.Author`         |
| `Subject`   | `document.Information.Subject`        |
| `Keywords`  | `document.Information.Keywords`       |
| `Creator`   | `document.Information.Creator`        |
| `Producer`  | `document.Information.Producer`       |

Any other field name returns `null`.

## YAML Configuration

```yaml
files:
  - pattern: "**/*.pdf"
    pdf:
      metadata:
        - field: Title
          contains: "Annual Report"
        - field: Author
          matches: "DEMA Consulting.*"
      pages:
        min: 5
        max: 100
      text:
        - contains: "Executive Summary"
        - does-not-contain: "DRAFT"
```

## Design Decisions

- **Immediate failure on parse error**: Attempting to apply metadata, page, or text
  assertions against a file that is not a valid PDF would produce meaningless partial
  results. Reporting the parse failure immediately and stopping gives users a clear,
  actionable error message.
- **PdfPig chosen**: PdfPig provides a managed .NET API for reading metadata, page counts,
  and extractable text without native dependencies, making it suitable for cross-platform
  CI/CD environments.
- **Shared text rule hierarchy**: Body text assertions delegate to the same `FileAssertRule`
  hierarchy used by `FileAssertTextAssert`, ensuring consistent rule behavior across all
  assertion types.
