### FileAssertZipAssert Design

#### Overview

The `FileAssertZipAssert` class validates the contents of a zip archive by applying the full
FileAssert assertion suite to its entries. It accepts an `IFileContainer` and an entry path,
opens the entry as a zip archive, wraps it in a `ZipFileContainer`, and runs all configured
`FileAssertFile` assertions against the archive's contents using a scoped `IContext` for
breadcrumb-style error messages.

This design replaces the earlier glob-count-only approach; the full assertion suite (text, xml,
html, yaml, json, pdf, and recursive zip) is now available for zip entry validation.

#### Class Structure

##### Properties

| Property | Type                            | Description                                   |
| :------- | :------------------------------ | :-------------------------------------------- |
| `Files`  | `IReadOnlyList<FileAssertFile>` | The file assertions to apply to zip contents. |

##### Factory Method

```csharp
internal static FileAssertZipAssert Create(FileAssertZipData data)
```

Converts each `FileAssertFileData` DTO from `data.Files` into a
`FileAssertFile` instance after validating that a pattern is specified.

| Parameter | Description                              |
| :-------- | :--------------------------------------- |
| `data`    | The zip assertion DTO; must not be null. |

| Return / Exception          | Condition                                                   |
| :-------------------------- | :---------------------------------------------------------- |
| Returns                     | A new `FileAssertZipAssert` with all file assertions built. |
| `ArgumentNullException`     | `data` is null.                                             |
| `InvalidOperationException` | A file entry has a null or whitespace pattern.              |

##### Run Method

```csharp
internal void Run(IContext context, IFileContainer container, string entryPath)
```

Opens the zip entry via `container.OpenEntry(entryPath)`, wraps it in a `ZipFileContainer`,
and runs all `FileAssertFile` assertions against the archive contents.

Execution proceeds in the following steps:

1. Calls `container.GetDisplayPath(entryPath)` to get the display path for breadcrumb use.
2. Opens the entry stream via `container.OpenEntry(entryPath)`.
3. Wraps the stream in a `ZipFileContainer(stream, displayPath)`.
4. If `InvalidDataException`, `IOException`, or `UnauthorizedAccessException` is thrown constructing
   the `ZipFileContainer`, writes the parse error and returns immediately. The stream is disposed in
   a nested `try` block even when the `ZipFileContainer` constructor throws.
5. Creates a scoped context via `context.WithPrefix(displayPath)`.
6. Runs each `FileAssertFile` in `Files` against the `ZipFileContainer` and scoped context.

###### Run Error Messages

```text
File '<displayPath>' could not be read as a zip archive
```

| Parameter   | Description                                              |
| :---------- | :------------------------------------------------------- |
| `context`   | The `IContext` to report errors through.                 |
| `container` | The `IFileContainer` that owns the zip entry.            |
| `entryPath` | The relative path of the zip entry within the container. |

#### YAML Configuration

Zip entry assertions are declared under the `zip:` key of a file entry, using the same
`files:` structure as top-level test files:

```yaml
files:
  - pattern: "output/package.zip"
    zip:
      files:
        - pattern: 'lib/net8.0/MyLib.dll'
          min: 1
          max: 1
        - pattern: 'lib/**/*.dll'
          min: 1
          text:
            - contains: "Copyright"
```

The file assertions inside a `zip:` block use the same `FileAssertFileData` schema as
top-level file assertions, enabling the full assertion suite (text, xml, html, yaml, json,
pdf, nested zip) against the archive contents.

#### Design Decisions

- **Full assertion suite in archives**: By wrapping the zip entry stream in a `ZipFileContainer`
  and running `FileAssertFile` assertions, every asserter (text, xml, html, yaml, json, pdf, and
  recursive zip) becomes available for archive entry validation without any per-asserter changes.
- **`IFileContainer` parameter**: Accepting `IFileContainer` rather than a file path allows zip
  entries within outer archives to be opened as streams and wrapped directly, enabling zip-in-zip
  assertion scenarios.
- **Stream disposal on `ZipArchive` constructor failure**: A nested `try` block ensures the entry
  stream is disposed even when the `ZipFileContainer` constructor throws `InvalidDataException`.
  Without this guard, the stream from `container.OpenEntry` would remain open, locking the
  underlying file or archive entry.
- **Scoped context for breadcrumbs**: `context.WithPrefix(displayPath)` creates a scoped
  `IContext` that prepends the archive's display path to every error message, giving users
  unambiguous context (`"outer.zip > entry.xml > error"`) without requiring any formatting
  logic in the individual asserters.
- **Forward-slash normalization handled by `ZipFileContainer`**: Entry path normalization is the
  responsibility of `ZipFileContainer.GetEntries`, not `FileAssertZipAssert`. This keeps the
  asserter free of container-specific logic.

#### Purpose

`FileAssertZipAssert` is responsible for opening a zip archive entry from an `IFileContainer`
and applying the full FileAssert assertion suite to its contents using a scoped context.

#### Data Model

| Property | Type                            | Description                                  |
| :------- | :------------------------------ | :------------------------------------------- |
| `Files`  | `IReadOnlyList<FileAssertFile>` | File assertions applied to the zip contents. |

#### Key Methods

| Method                                                              | Purpose                                                    |
| :------------------------------------------------------------------ | :--------------------------------------------------------- |
| `Create(FileAssertZipData data)`                                    | Factory: builds `FileAssertFile` list from DTO.            |
| `Run(IContext context, IFileContainer container, string entryPath)` | Opens zip entry, wraps in container, runs file assertions. |

#### Error Handling

| Scenario                                                                                     | Handling                                               |
| :------------------------------------------------------------------------------------------- | :----------------------------------------------------- |
| Null `data` passed to `Create`                                                               | `ArgumentNullException` thrown.                        |
| File entry with null or whitespace pattern in `Create`                                       | `InvalidOperationException` thrown.                    |
| `IOException`, `InvalidDataException`, or `UnauthorizedAccessException` opening entry as zip | Error written via `context.WriteError`; `Run` returns. |
| Entry match count below `Min`                                                                | Reported by the `FileAssertFile` instance.             |
| Entry match count above `Max`                                                                | Reported by the `FileAssertFile` instance.             |

#### Dependencies

- **Delegates to**: `FileAssertFile.Run` with a `ZipFileContainer` and scoped `IContext`.
- **OTS dependencies**:
  - `System.IO.Compression.ZipArchive` (BCL) via `ZipFileContainer`.
  - `Microsoft.Extensions.FileSystemGlobbing.Matcher` via `FileAssertFile`.
- **Configuration dependency**: `FileAssertZipData` and `FileAssertFileData` DTOs from the
  Configuration subsystem.

#### Callers

- **Caller**: `FileAssertFile.Run` calls `ZipAssert.Run(context, container, entryPath)` when the
  `zip:` assertion block is declared.
- **Created by**: `FileAssertFile.Create` via `FileAssertZipAssert.Create`.
