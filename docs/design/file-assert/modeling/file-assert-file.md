### FileAssertFile Design

#### Overview

The `FileAssertFile` class locates files on disk using a glob pattern, enforces
optional minimum and maximum count constraints, and delegates per-file assertions to
file-type-specific assert units.

#### Class Structure

##### Properties

| Property     | Type                             | Description                                       |
| :----------- | :------------------------------- | :------------------------------------------------ |
| `Pattern`    | `string`                         | Glob pattern used to locate files.                |
| `Min`        | `int?`                           | Optional minimum number of matching files.        |
| `Max`        | `int?`                           | Optional maximum number of matching files.        |
| `Count`      | `int?`                           | Optional exact number of matching files.          |
| `MinSize`    | `long?`                          | Optional minimum file size in bytes per file.     |
| `MaxSize`    | `long?`                          | Optional maximum file size in bytes per file.     |
| `TextAssert` | `FileAssertTextAssert?`          | Text content assertions (null if not declared).   |
| `PdfAssert`  | `FileAssertPdfAssert?`           | PDF document assertions (null if not declared).   |
| `XmlAssert`  | `FileAssertXmlAssert?`           | XML node assertions (null if not declared).       |
| `HtmlAssert` | `FileAssertHtmlAssert?`          | HTML node assertions (null if not declared).      |
| `YamlAssert` | `FileAssertYamlAssert?`          | YAML node assertions (null if not declared).      |
| `JsonAssert` | `FileAssertJsonAssert?`          | JSON node assertions (null if not declared).      |
| `ZipAssert`  | `FileAssertZipAssert?`           | Zip archive entry assertions; `null` if absent.   |

##### Factory Method

```csharp
internal static FileAssertFile Create(FileAssertFileData data)
```

The factory validates that `Pattern` is not null or whitespace before constructing
the instance. Each file-type assert is created from the corresponding data block when
that block is present.

##### Execution Method

```csharp
internal void Run(IContext context, IFileContainer container)
```

Execution proceeds in five phases:

1. **File discovery** — `Microsoft.Extensions.FileSystemGlobbing.Matcher` evaluates
   `Pattern` against the entries returned by `container.GetEntries()` and returns the
   list of matched entry paths.

2. **Minimum count validation** — If `Min` is set and the match count is below it,
   an error is written and execution returns immediately.

3. **Maximum count validation** — If `Max` is set and the match count exceeds it,
   an error is written and execution returns immediately.

4. **Exact count validation** — If `Count` is set and the match count does not equal
   it, an error is written and execution returns immediately. Early return prevents
   misleading per-file errors when the count constraint already signals a failure.

5. **Per-file validation** — Each matched entry is inspected individually:
   a. Validates size constraints (`MinSize`, `MaxSize`) using `container.GetEntrySize(entryPath)`.
      Size violations are recorded via `context.WriteError` but do NOT cause early return;
      the remaining per-file assertions continue to execute.
   b. If `TextAssert` is defined, delegates to `FileAssertTextAssert.Run(context, container, entryPath)`.
   c. If `PdfAssert` is defined, delegates to `FileAssertPdfAssert.Run(context, container, entryPath)`.
   d. If `XmlAssert` is defined, delegates to `FileAssertXmlAssert.Run(context, container, entryPath)`.
   e. If `HtmlAssert` is defined, delegates to `FileAssertHtmlAssert.Run(context, container, entryPath)`.
   f. If `YamlAssert` is defined, delegates to `FileAssertYamlAssert.Run(context, container, entryPath)`.
   g. If `JsonAssert` is defined, delegates to `FileAssertJsonAssert.Run(context, container, entryPath)`.
   h. If `ZipAssert` is defined, delegates to `FileAssertZipAssert.Run(context, container, entryPath)`.

##### Count Constraint Error Messages

```text
Pattern '<Pattern>' matched <n> file(s), but expected at least <Min>
Pattern '<Pattern>' matched <n> file(s), but expected at most <Max>
Pattern '<Pattern>' matched <n> file(s), but expected exactly <Count>
```

##### Size Constraint Error Messages

```text
File '<displayPath>' is <n> byte(s), which is less than the minimum <MinSize> bytes
File '<displayPath>' is <n> byte(s), which exceeds the maximum <MaxSize> bytes
```

#### YAML Configuration

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

#### Design Decisions

- **`IContext` and `IFileContainer` parameters**: Accepting interfaces rather than concrete types
  allows `FileAssertFile` to be called identically for on-disk files (via `DirectoryFileContainer`)
  and for zip archive entries (via `ZipFileContainer`), with no conditional logic in this class.
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

#### Purpose

`FileAssertFile` is responsible for a single file-pattern assertion within a test. It
discovers files on disk via a glob pattern, enforces count and size constraints, and
delegates per-file content validation to file-type-specific assert units.

#### Data Model

| Property     | Type                    | Description                                          |
| :----------- | :---------------------- | :--------------------------------------------------- |
| `Pattern`    | `string`                | Glob pattern used to discover files (required).      |
| `Min`        | `int?`                  | Minimum number of matching files; `null` = no bound. |
| `Max`        | `int?`                  | Maximum number of matching files; `null` = no bound. |
| `Count`      | `int?`                  | Exact number of matching files; `null` = no bound.   |
| `MinSize`    | `long?`                 | Minimum file size in bytes; `null` = no bound.       |
| `MaxSize`    | `long?`                 | Maximum file size in bytes; `null` = no bound.       |
| `TextAssert` | `FileAssertTextAssert?` | Text content assert unit; `null` if not declared.    |
| `PdfAssert`  | `FileAssertPdfAssert?`  | PDF assert unit; `null` if not declared.             |
| `XmlAssert`  | `FileAssertXmlAssert?`  | XML assert unit; `null` if not declared.             |
| `HtmlAssert` | `FileAssertHtmlAssert?` | HTML assert unit; `null` if not declared.            |
| `YamlAssert` | `FileAssertYamlAssert?` | YAML assert unit; `null` if not declared.            |
| `JsonAssert` | `FileAssertJsonAssert?` | JSON assert unit; `null` if not declared.            |
| `ZipAssert`  | `FileAssertZipAssert?`  | Zip archive assert unit; `null` if not declared.     |

#### Key Methods

| Method                                            | Purpose                                                            |
| :-----------------------------------------------  | :----------------------------------------------------------------- |
| `Create(FileAssertFileData data)`                 | Factory: validates pattern, builds assert units, returns instance. |
| `Run(IContext context, IFileContainer container)` | Discovers entries, checks count/size, delegates to assert units.   |

#### Error Handling

| Scenario                               | Handling                                                              |
| :------------------------------------- | :-------------------------------------------------------------------- |
| Null or whitespace `Pattern` in data   | `InvalidOperationException` thrown by `Create`.                       |
| Count below `Min`                      | Error written via `context.WriteError`; `Run` returns immediately.    |
| Count above `Max`                      | Error written via `context.WriteError`; `Run` returns immediately.    |
| Count not equal to `Count` constraint  | Error written via `context.WriteError`; `Run` returns immediately.    |
| File size outside `MinSize`/`MaxSize`  | Error written via `context.WriteError`; per-file assertions continue. |
| Parse errors in assert units           | Assert units catch parse exceptions and call `context.WriteError`.    |

#### Interactions

- **Caller**: `FileAssertTest.Run` iterates the `Files` collection and calls `Run` on each instance.
- **Created by**: `FileAssertTest.Create` via `FileAssertFile.Create`.
- **Delegates to**:
  - `FileAssertTextAssert.Run` for text content rules.
  - `FileAssertPdfAssert.Run` for PDF document rules.
  - `FileAssertXmlAssert.Run` for XML XPath rules.
  - `FileAssertHtmlAssert.Run` for HTML XPath rules.
  - `FileAssertYamlAssert.Run` for YAML path rules.
  - `FileAssertJsonAssert.Run` for JSON path rules.
  - `FileAssertZipAssert.Run` for zip archive entry rules.
- **OTS dependency**: `Microsoft.Extensions.FileSystemGlobbing.Matcher` for file discovery.
