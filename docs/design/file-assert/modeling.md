## Modeling Subsystem Design

### Overview

The Modeling subsystem contains the domain objects that represent a FileAssert test suite at
runtime. It transforms the data transfer objects produced by the Configuration subsystem into
executable domain objects and drives the assertion logic.

### Subsystem Contents

| Unit                    | File                        | Responsibility                                         |
| :---------------------- | :-------------------------- | :----------------------------------------------------- |
| `FileAssertTest`        | `FileAssertTest.cs`         | Named test with file assertions and filter matching.   |
| `FileAssertFile`        | `FileAssertFile.cs`         | Glob pattern matcher; count constraints and rules.     |
| `FileAssertRule`        | `FileAssertRule.cs`         | Abstract content validation rule hierarchy.            |
| `FileAssertTextAssert`  | `FileAssertTextAssert.cs`   | Applies text content rules to matched file text.       |
| `FileAssertPdfAssert`   | `FileAssertPdfAssert.cs`    | Parses PDF; applies metadata, page, and text rules.    |
| `FileAssertXmlAssert`   | `FileAssertXmlAssert.cs`    | Parses XML; applies XPath node count assertions.       |
| `FileAssertHtmlAssert`  | `FileAssertHtmlAssert.cs`   | Parses HTML; applies XPath node count assertions.      |
| `FileAssertYamlAssert`  | `FileAssertYamlAssert.cs`   | Parses YAML; applies dot-notation path assertions.     |
| `FileAssertJsonAssert`  | `FileAssertJsonAssert.cs`   | Parses JSON; applies dot-notation path assertions.     |
| `FileAssertZipAssert`   | `FileAssertZipAssert.cs`    | Opens zip; applies full assertion suite to entries.    |

### Subsystem Responsibilities

- Construct domain objects from Configuration DTOs via static factory methods.
- Validate required fields (test name, file pattern, rule type) during construction.
- Execute file glob matching using `Microsoft.Extensions.FileSystemGlobbing`.
- Enforce minimum and maximum file count constraints.
- Apply positive (contains/matches) and negative (does-not-contain/does-not-match) content rules to matched file text.
- Parse matched files as PDF, XML, HTML, YAML, or JSON documents when the corresponding assertion block is declared.
- Report an immediate error when a file cannot be parsed as the declared format.
- Apply structured-document query assertions (XPath or dot-notation) to parsed document nodes.
- Open zip archives, wrap them in `ZipFileContainer`, and apply the full assertion suite to their entries.
- Report assertion failures via the `IContext` interface from the Cli subsystem, enabling scoped breadcrumb error messages for nested zip assertions.

### Object Hierarchy

```text
FileAssertTest
└── FileAssertFile (one or more)
    ├── FileAssertTextAssert? (zero or one)
    │   └── FileAssertRule (zero or more)
    │       ├── FileAssertContainsRule
    │       ├── FileAssertDoesNotContainRule
    │       ├── FileAssertMatchesRule
    │       └── FileAssertDoesNotMatchRule
    ├── FileAssertPdfAssert? (zero or one)
    ├── FileAssertXmlAssert? (zero or one)
    ├── FileAssertHtmlAssert? (zero or one)
    ├── FileAssertYamlAssert? (zero or one)
    ├── FileAssertJsonAssert? (zero or one)
    └── FileAssertZipAssert? (zero or one)
```

### Interfaces

#### Exposed

| Member / Class                 | Description                                                                                 |
| :--------------------------    | :------------------------------------------------------------------------------------------ |
| `FileAssertTest.Create`        | Factory method: builds a domain test object from a `FileAssertTestData` DTO.                |
| `FileAssertTest.MatchesFilter` | Returns whether the test name or tags match the provided filter list.                       |
| `FileAssertTest.Run`           | Executes all file assertions within the test and reports results via `Context`.             |
| `FileAssertFile.Create`        | Factory method: builds a domain file-pattern object from a `FileAssertFileData` DTO.        |
| `FileAssertRule.Create`        | Factory method: selects and returns the correct concrete rule subclass.                     |

#### Consumed

| Dependency                                                                                    | Usage                                                                          |
| :-------------------------                                                                    | :----------------------------------------------------------------------------- |
| `IContext` (Cli subsystem)                                                                    | Receives assertion failure messages and error exit code.                       |
| `IFileContainer` (Utilities subsystem)                                                        | Uniform file-access abstraction; enables assertions over directories and zips. |
| `FileAssertData` DTOs                                                                         | Input types for all `Create` factory methods.                                  |
| Microsoft.Extensions.FileSystemGlobbing                                                       | Cross-platform glob evaluation for file discovery.                             |
| YamlDotNet, PdfPig, HtmlAgilityPack, System.Xml.Linq, System.Text.Json, System.IO.Compression | Format-specific parsing libraries.                                             |

### Design

Domain objects are constructed and executed in the following layers:

1. `FileAssertTest.Create` iterates the `FileAssertTestData.Files` list, calling
   `FileAssertFile.Create` for each entry. `FileAssertFile.Create` in turn creates any
   declared assert units (`FileAssertTextAssert`, `FileAssertPdfAssert`, etc.).
2. `FileAssertTextAssert.Create` iterates rule data, calling `FileAssertRule.Create` for
   each entry to produce the correct concrete rule subclass.
3. During execution, `FileAssertConfig.Run` calls `FileAssertTest.Run` → `FileAssertFile.Run`
   → assert unit `Run` methods, threading `IContext` through every layer so all failures are
   reported via a single path. `FileAssertTest.Run` wraps the base path in a
   `DirectoryFileContainer` before passing it down. `FileAssertZipAssert.Run` calls
   `context.WithPrefix(displayPath)` to create a scoped `IContext` that prepends the archive
   path as a breadcrumb to every nested error message.

### Dependencies

| Dependency    | Usage                                                          |
| :------------ | :------------------------------------------------------------- |
| Cli           | Receives `IContext` to report assertion failures.              |
| Utilities     | Accesses files via `IFileContainer` and `ZipFileContainer`.    |
| Configuration | Accepts DTO types for test, file, and rule construction.       |

### Callers

- None.

### Design Decisions

- **`IContext` over `Context`**: All asserters and `FileAssertFile` accept `IContext` rather than
  the concrete `Context` class. This allows `FileAssertZipAssert` to pass a scoped prefix context
  to nested asserters without those asserters requiring any knowledge of the scoping mechanism.
- **Factory methods over constructors**: Each domain class provides an `internal static Create`
  method that validates the DTO and constructs the domain object, keeping constructors private.
- **Error accumulation**: Failures are reported via `context.WriteError` rather than exceptions,
  so all assertions run to completion and all failures are visible in a single pass.
- **Glob via FileSystemGlobbing**: Uses the `Microsoft.Extensions.FileSystemGlobbing` library for
  cross-platform glob pattern evaluation consistent with the rest of the .NET ecosystem.
- **Compiled regex with timeout**: The `FileAssertMatchesRule` compiles its regex at construction
  time and applies a ten-second evaluation timeout to guard against catastrophic backtracking.
- **Lazy file-type parsing**: A file is only parsed as a structured document if the corresponding
  assertion block is declared, avoiding unnecessary I/O and library invocations.
- **Immediate failure on parse error**: If a file-type assertion block is declared and the file
  cannot be parsed, an error is written immediately and no further assertions for that file are
  evaluated.
- **Full assertion suite in zip archives**: `FileAssertZipAssert` wraps the zip entry stream in
  a `ZipFileContainer`, then runs `FileAssertFile` against it. This means every asserter (text,
  xml, html, yaml, json, pdf, and recursive zip) is available for zip entry validation without
  any per-asserter changes.
