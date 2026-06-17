### IFileContainer Design

#### Purpose

`IFileContainer` is the uniform file-access abstraction used by all asserters in FileAssert.
It decouples asserters from the filesystem by hiding whether a "file" resides on disk inside a
directory or as an entry inside a zip archive. Asserters call `GetEntries()`, `OpenEntry()`,
`GetEntrySize()`, and `GetDisplayPath()` without needing to know how the container is backed.

Two implementations are provided:

- `DirectoryFileContainer` — backed by a local filesystem directory.
- `ZipFileContainer` — backed by a `ZipArchive` opened from a `Stream`.

#### Interface Members

```csharp
internal interface IFileContainer
{
    IReadOnlyList<string> GetEntries();
    Stream OpenEntry(string entryPath);
    long GetEntrySize(string entryPath);
    string GetDisplayPath(string entryPath);
}
```

| Member                               | Description                                                                          |
| :----------------------------------- | :----------------------------------------------------------------------------------- |
| `GetEntries()`                       | Returns all relative entry paths, using forward slashes as the path separator.       |
| `OpenEntry(string entryPath)`        | Opens the named entry for sequential reading; throws `IOException` on failure.       |
| `GetEntrySize(string entryPath)`     | Returns the uncompressed byte length of the named entry.                             |
| `GetDisplayPath(string entryPath)`   | Returns a human-readable path for the entry, used in error messages.                 |

#### Design Rationale

- **Single abstraction for both directories and archives**: Asserters can be written once and used
  against both on-disk files and zip archive entries without any conditional logic.
- **Forward-slash normalization in `GetEntries()`**: All returned paths use `/` as the separator
  regardless of platform, ensuring glob patterns work consistently across Windows, Linux, and macOS.
- **`IOException` for missing entries**: Both implementations throw `IOException` (or a subclass) when
  an entry cannot be opened, allowing asserters to catch a single exception type for all I/O failures.
- **`IDisposable` on implementations**: Both `DirectoryFileContainer` and `ZipFileContainer`
  implement `IDisposable`. `DirectoryFileContainer.Dispose()` is a no-op; `ZipFileContainer.Dispose()`
  closes the underlying `ZipArchive` and stream. Using `IDisposable` on both allows callers to use
  `using` statements uniformly.

#### Data Model

`IFileContainer` itself is stateless. Each implementation holds its own state — see
`DirectoryFileContainer` and `ZipFileContainer` design documents for details.

#### Key Methods

| Method                                   | Description                                              |
| :--------------------------------------- | :------------------------------------------------------- |
| `GetEntries() → IReadOnlyList<string>`   | Enumerate all entries (forward-slash paths).             |
| `OpenEntry(string) → Stream`             | Open a named entry as a readable stream.                 |
| `GetEntrySize(string) → long`            | Return the uncompressed byte length of an entry.         |
| `GetDisplayPath(string) → string`        | Return a display path for use in error messages.         |

#### Error Handling

| Scenario                            | Handling                                                            |
| :---------------------------------- | :------------------------------------------------------------------ |
| Entry not found on `OpenEntry`      | `IOException` (or subclass) thrown by the implementation.           |
| Null `entryPath` passed             | `ArgumentNullException` thrown by both implementations.             |

#### Dependencies

- `DirectoryFileContainer` — depends on .NET BCL (`File`, `Directory`, `Path`, `FileInfo`).
- `ZipFileContainer` — depends on `System.IO.Compression.ZipArchive`.

#### Callers

- `FileAssertFile.Run` — calls all four members to resolve, open, measure, and display entries.
- `FileAssertZipAssert.Run` — calls `container.OpenEntry(entryPath)` to get the zip entry stream,
  then wraps it in a `ZipFileContainer` for nested assertion.
- All 7 asserters — call `container.OpenEntry(entryPath)` and `container.GetDisplayPath(entryPath)`.
- `FileAssertTest.Run` — creates a `DirectoryFileContainer` from `basePath`.
