### ZipFileContainer Design

#### Purpose

`ZipFileContainer` is the zip archive implementation of `IFileContainer`. It wraps a `ZipArchive`
opened from a caller-supplied `Stream`, exposing the archive's file entries as a virtual container.
It supports nested zip assertion by accepting a stream rather than a file path, enabling
zip-in-zip scenarios where the outer archive's entry stream is directly wrapped as an inner
`ZipFileContainer`.

#### Class Structure

##### Constructor

```csharp
internal ZipFileContainer(Stream stream, string displayName)
```

Opens a `ZipArchive` from `stream` with `ZipArchiveMode.Read` and `leaveOpen: false`. The
`displayName` is stored for use in `GetDisplayPath`. Both parameters are validated non-null.

The `leaveOpen: false` parameter means that when `ZipFileContainer` is disposed, the `ZipArchive`
closes the underlying `stream` automatically.

**Exceptions thrown:**

| Exception              | Condition                                        |
| :--------------------- | :----------------------------------------------- |
| `ArgumentNullException`| `stream` or `displayName` is null.               |
| `InvalidDataException` | Stream does not contain a valid zip archive.     |

##### GetEntries Method

```csharp
public IReadOnlyList<string> GetEntries()
```

Returns the names of all non-directory entries in the zip archive as a read-only list with
forward slashes. Directory entries (whose names end with `/`) are excluded.

**Steps:**

1. Enumerate `_archive.Entries`.
2. Select `e.FullName.Replace('\\', '/')` for each entry.
3. Filter out entries whose name ends with `/` (directory markers).
4. Return as a read-only list.

##### OpenEntry Method

```csharp
public Stream OpenEntry(string entryPath)
```

Finds the zip entry with the given name and opens it for reading via `entry.Open()`. The
supplied `entryPath` is normalized by replacing any `\` with `/` before calling
`_archive.GetEntry(...)`, mirroring the normalization performed by `GetEntries()` so that
callers using either separator can locate entries. Throws `IOException` with a descriptive
message when the entry is not found.

**Why `IOException` rather than `FileNotFoundException`:** `FileNotFoundException` implies a
file-system file. Inside a zip archive the abstraction is a "stream entry", so the more general
`IOException` (which `FileNotFoundException` is a subclass of) is appropriate and consistent
with the asserters' catch clauses.

##### GetEntrySize Method

```csharp
public long GetEntrySize(string entryPath)
```

Returns `entry.Length` (the uncompressed size) for the named entry. The supplied `entryPath`
is normalized by replacing any `\` with `/` before lookup, mirroring `OpenEntry` and
`GetEntries`. Throws `IOException` when the entry is not found.

##### GetDisplayPath Method

```csharp
public string GetDisplayPath(string entryPath)
```

Returns `"{_displayName} > {entryPath}"`. This provides a breadcrumb-style path that includes
the archive name, enabling users to trace nested errors back through the archive hierarchy.

For example, if `displayName` is `"outer.zip"` and `entryPath` is `"lib/inner.dll"`, the
display path is `"outer.zip > lib/inner.dll"`.

For nested archives where the outer `ZipFileContainer` was created with display name
`"outer.zip > inner.zip"`, the display path of a further-nested entry would be
`"outer.zip > inner.zip > entry.txt"`.

##### Dispose Method

```csharp
public void Dispose()
```

Disposes `_archive`. Because the `ZipArchive` was opened with `leaveOpen: false`, disposing
it also closes and disposes the underlying stream.

#### Design Decisions

- **Stream-based constructor**: Accepting a `Stream` rather than a file path allows the same class
  to be used when the zip archive is itself a zip entry in an outer archive. The outer asserter
  opens the entry as a stream and passes it directly.
- **`leaveOpen: false`**: Closing the stream on archive disposal simplifies ownership. The caller
  that opened the entry stream via `DirectoryFileContainer.OpenEntry` or an outer
  `ZipFileContainer.OpenEntry` transfers ownership to `ZipFileContainer`.
- **`IOException` on missing entry**: Using the parent type rather than `FileNotFoundException`
  avoids conflating file-system semantics with archive-entry semantics. All callers catch
  `IOException` for I/O failures.
- **Directory marker exclusion**: Zip archives may contain explicit directory entries whose names
  end with `/`. These markers carry no file content, so `GetEntries` filters them out and exposes
  only real file entries to the asserters.
- **Display name as breadcrumb prefix**: Embedding the archive name in `GetDisplayPath` makes
  error messages self-explanatory without requiring the asserter to format the path itself.

#### Data Model

| Field          | Type          | Description                                              |
| :------------- | :------------ | :------------------------------------------------------- |
| `_archive`     | `ZipArchive`  | The open zip archive wrapping the supplied stream.       |
| `_displayName` | `string`      | The archive's display name for breadcrumb path building. |

#### Key Methods

| Method                                    | Description                                                          |
| :---------------------------------------- | :------------------------------------------------------------------- |
| `ZipFileContainer(Stream, string)`        | Constructor: opens `ZipArchive` from stream.                         |
| `GetEntries() → IReadOnlyList<string>`    | Enumerate all file entries (no directory markers) with `/` paths.    |
| `OpenEntry(string) → Stream`              | Open a named entry; throws `IOException` when not found.             |
| `GetEntrySize(string) → long`             | Return the uncompressed size of a named entry.                       |
| `GetDisplayPath(string) → string`         | Return `"{displayName} > {entryPath}"` for error messages.           |
| `Dispose()`                               | Dispose the `ZipArchive` (also closes the underlying stream).        |

#### Error Handling

| Scenario                         | Handling                                                            |
| :------------------------------- | :------------------------------------------------------------------ |
| Stream is not a valid zip file   | `InvalidDataException` thrown by `ZipArchive` constructor.          |
| Entry not found on `OpenEntry`   | `IOException` thrown with `"Zip entry '{name}' not found"` message. |
| Null `stream` or `displayName`   | `ArgumentNullException` thrown in constructor.                      |
| Null `entryPath`                 | `ArgumentNullException` thrown before archive lookup.               |

#### Dependencies

- `System.IO.Compression.ZipArchive`, `ZipArchiveEntry`

#### Callers

- `FileAssertZipAssert.Run` — creates a `ZipFileContainer` from the zip entry's stream, then
  runs all configured file assertions against it.
- Test project `IFileContainerTests.cs` — verifies entry enumeration, stream opening, size
  reporting, and display path generation.
