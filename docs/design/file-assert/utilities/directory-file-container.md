### DirectoryFileContainer Design

#### Overview

`DirectoryFileContainer` is the filesystem implementation of `IFileContainer`. It exposes a local
directory as a container of file entries, enumerating all files recursively, opening them via their
absolute paths, and returning their on-disk sizes and full paths for error messages.

It is used by `FileAssertTest.Run`, which wraps the user-supplied `basePath` in a
`DirectoryFileContainer` before passing it down to `FileAssertFile.Run` and the asserters.

#### Class Structure

##### Constructor

```csharp
internal DirectoryFileContainer(string basePath)
```

Stores the base directory path. Validates that `basePath` is not null. Does not verify that the
directory exists at construction time — a non-existent directory is handled gracefully in
`GetEntries()`.

##### GetEntries Method

```csharp
public IReadOnlyList<string> GetEntries()
```

Returns all files found recursively under `BasePath`, each as a path relative to `BasePath` with
forward slashes as the separator.

**Steps:**

1. If `BasePath` does not exist (`Directory.Exists` returns `false`), return `Array.Empty<string>()`.
2. Call `Directory.EnumerateFiles(BasePath, "*", SearchOption.AllDirectories)`.
3. Convert each absolute path to a relative path via `Path.GetRelativePath(BasePath, f)`.
4. Replace back-slashes with forward slashes via `.Replace('\\', '/')`.
5. Return the list as a read-only collection.

**Rationale for empty-list on missing directory:** Glob-based count constraints treat zero matches
as a valid count. Returning an empty list rather than throwing allows `Min = 0` constraints to pass
against directories that may legitimately not exist yet.

##### OpenEntry Method

```csharp
public Stream OpenEntry(string entryPath)
```

Opens the file at `Path.Combine(BasePath, entryPath)` for reading using `File.OpenRead`. Throws
`FileNotFoundException` (a subclass of `IOException`) when the file does not exist.

##### GetEntrySize Method

```csharp
public long GetEntrySize(string entryPath)
```

Returns the byte length of the file at `Path.Combine(BasePath, entryPath)` via `new FileInfo(fullPath).Length`.

##### GetDisplayPath Method

```csharp
public string GetDisplayPath(string entryPath)
```

Returns `Path.Combine(BasePath, entryPath)` — the full on-disk path of the entry. This is used in
error messages so users can identify the exact file that failed an assertion.

##### Dispose Method

```csharp
public void Dispose()
```

No-op. `DirectoryFileContainer` holds no disposable resources. The method is provided for
symmetry with `ZipFileContainer` so both can be used in `using` statements.

#### Design Decisions

- **Empty list on missing directory**: Returning an empty list rather than throwing when the
  directory does not exist is intentional. Zero-match count constraints are valid, and some callers
  construct the container before the directory is guaranteed to exist.
- **Full path as display path**: Error messages should identify files by their full path so users
  can navigate to them. The relative entry path alone is insufficient context.
- **No-op Dispose**: `DirectoryFileContainer` does not open any resources; all streams returned by
  `OpenEntry` are the caller's responsibility. Implementing `IDisposable` as a no-op provides
  symmetry with `ZipFileContainer` and allows callers to use `using var container = ...` uniformly.

#### Data Model

| Field      | Type     | Description                              |
| :--------- | :------- | :--------------------------------------- |
| `BasePath` | `string` | The absolute path of the root directory. |

#### Key Methods

| Method                                     | Description                                            |
| :----------------------------------------- | :----------------------------------------------------- |
| `DirectoryFileContainer(string basePath)`  | Constructor: stores the base path.                     |
| `GetEntries() → IReadOnlyList<string>`     | Enumerate all files recursively with forward slashes.  |
| `OpenEntry(string) → Stream`               | Open a file by relative path for reading.              |
| `GetEntrySize(string) → long`              | Return the file size in bytes.                         |
| `GetDisplayPath(string) → string`          | Return the full file-system path for error messages.   |
| `Dispose()`                                | No-op for IDisposable symmetry.                        |

#### Error Handling

| Scenario                              | Handling                                                           |
| :------------------------------------ | :----------------------------------------------------------------- |
| Non-existent base directory           | `GetEntries()` returns empty list; no exception.                   |
| Missing file on `OpenEntry`           | `FileNotFoundException` thrown by `File.OpenRead`.                 |
| Null `entryPath`                      | `ArgumentNullException` thrown before path combination.            |
| Null `basePath`                       | `ArgumentNullException` thrown in constructor.                     |

#### Dependencies

- .NET BCL: `File`, `Directory`, `Path`, `FileInfo`, `SearchOption`

#### Callers

- `FileAssertTest.Run` — creates a `DirectoryFileContainer(basePath)` at the top of the assertion
  chain.
- `FileAssertFile.Run` — calls all four interface methods to enumerate, open, measure, and display
  entries.
- All 7 asserters — receive the container via `FileAssertFile` delegation.
