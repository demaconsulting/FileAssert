## Utilities Subsystem Design

### Overview

The Utilities subsystem provides shared helper functionality used by other subsystems. It
contains security-sensitive or otherwise reusable operations that do not belong to any specific
domain subsystem.

### Subsystem Contents

| Unit                       | File                         | Responsibility                                                         |
| :------------------------- | :--------------------------- | :--------------------------------------------------------------------- |
| `PathHelpers`              | `PathHelpers.cs`             | Safe path-combination utility with path-traversal protection.          |
| `TemporaryDirectory`       | `TemporaryDirectory.cs`      | Disposable temporary directory with safe path resolution and clean-up. |
| `IFileContainer`           | `IFileContainer.cs`          | Uniform file-access interface over directories and zip archives.       |
| `DirectoryFileContainer`   | `DirectoryFileContainer.cs`  | IFileContainer implementation backed by a local filesystem directory.  |
| `ZipFileContainer`         | `ZipFileContainer.cs`        | IFileContainer implementation backed by a ZipArchive stream.           |

### Subsystem Responsibilities

- Provide path utilities that safely combine paths while preventing path-traversal attacks.
- Reject relative paths containing `..` or absolute paths when a relative path is expected.
- Create uniquely-named temporary directories and delete them automatically on disposal.
- Ensure all file paths within a temporary directory remain within its boundary.
- Provide a uniform `IFileContainer` abstraction for enumerating, opening, and measuring file
  entries regardless of whether they reside on disk or inside a zip archive.
- Expose local filesystem directories as `IFileContainer` instances via `DirectoryFileContainer`.
- Expose zip archive streams as `IFileContainer` instances via `ZipFileContainer`, enabling the
  full assertion suite to be applied to zip entry contents.

### Interfaces

#### Exposed

| Class / Member                                | Description                                                                              |
| :-------------------------------------------- | :--------------------------------------------------------------------------------------- |
| `PathHelpers.SafePathCombine(base, relative)` | Combines `base` and `relative`; throws `ArgumentException` if the result escapes `base`. |
| `TemporaryDirectory` *(constructor)*          | Creates a uniquely-named subdirectory under `Environment.CurrentDirectory`.              |
| `TemporaryDirectory.DirectoryPath`            | Full path to the temporary directory.                                                    |
| `TemporaryDirectory.GetFilePath(relative)`    | Resolves a relative path within the directory; creates intermediate subdirectories.      |
| `TemporaryDirectory.Dispose()`                | Deletes the temporary directory and all its contents.                                    |
| `IFileContainer.GetEntries()`                 | Returns all relative entry paths with forward-slash separators.                          |
| `IFileContainer.OpenEntry(entryPath)`         | Opens the named entry as a readable stream; throws `IOException` on failure.             |
| `IFileContainer.GetEntrySize(entryPath)`      | Returns the uncompressed byte length of the named entry.                                 |
| `IFileContainer.GetDisplayPath(entryPath)`    | Returns a human-readable display path for use in error messages.                         |
| `DirectoryFileContainer(basePath)`            | IFileContainer over a local filesystem directory.                                        |
| `ZipFileContainer(stream, displayName)`       | IFileContainer over a zip archive stream; supports nested zips.                          |

#### Consumed

| Dependency                                 | Usage                                                                         |
| :----------------------------------------- | :---------------------------------------------------------------------------- |
| .NET BCL (`Path`, `Directory`, `File`)     | All path manipulation and file-system operations.                             |
| `System.IO.Compression.ZipArchive`         | Zip archive access in `ZipFileContainer`.                                     |

### Design

`PathHelpers` and `TemporaryDirectory` collaborate in a layered pattern:

1. `TemporaryDirectory` delegates all path construction to `PathHelpers.SafePathCombine`, ensuring
   that both the directory name itself and every relative path passed to `GetFilePath` are safe.
2. `PathHelpers` performs validation independently of `TemporaryDirectory`, so it can be used
   directly by other subsystems (such as `SelfTest`) without going through `TemporaryDirectory`.

`IFileContainer` and its two implementations provide a uniform virtual-file-system API:

1. `DirectoryFileContainer` wraps a local directory path; `GetEntries` enumerates files
   recursively, normalizes separators to forward slashes, and returns empty for non-existent
   directories. `GetDisplayPath` returns the full on-disk path.
2. `ZipFileContainer` wraps a `ZipArchive` opened from a caller-supplied `Stream`; `GetEntries`
   filters out directory marker entries; `GetDisplayPath` returns `"{displayName} > {entryPath}"`,
   enabling breadcrumb-style paths in nested zip scenarios. `OpenEntry` throws `IOException` when
   the requested entry is not present.
3. Both implementations are `IDisposable`; `DirectoryFileContainer.Dispose` is a no-op while
   `ZipFileContainer.Dispose` closes the underlying archive and stream.

Neither `PathHelpers` nor `TemporaryDirectory` holds references to `Context` or any other
subsystem; they are pure utilities with no awareness of the tool's execution state.
`IFileContainer` and its implementations are similarly isolated, depending only on .NET BCL
file-system and compression APIs.

### Dependencies

- None.

### Callers

| Consumer  | Usage                                                                                     |
| :-------- | :---------------------------------------------------------------------------------------- |
| SelfTest  | Uses `TemporaryDirectory` and `PathHelpers.SafePathCombine` for fixture file management.  |
| Modeling  | Uses `IFileContainer`, `DirectoryFileContainer`, and `ZipFileContainer` to abstract file  |
|           | access across asserters, enabling the full assertion suite for both on-disk and zip files.|
| Tests     | Uses `TemporaryDirectory` for isolated file-system fixtures in all test projects.         |

### Design Decisions

- **Static class for PathHelpers**: `PathHelpers` is a static utility class with no instance state, suitable
  for use anywhere in the codebase without injection.
- **Defense-in-depth validation**: Path safety is validated both before and after combining
  paths, guarding against edge cases that might bypass the initial checks.
- **`Environment.CurrentDirectory` over `Path.GetTempPath()`**: On macOS, `/tmp` is a symlink
  to `/private/tmp`. Using the current directory avoids path-comparison failures caused by
  symlink resolution. See *TemporaryDirectory Design* for details.
- **`IFileContainer` over direct file paths in asserters**: Accepting an `IFileContainer`
  interface rather than a file path string allows asserters to be reused unchanged for both
  on-disk files and zip archive entries, without any conditional logic in the asserter.
- **`IOException` from `ZipFileContainer.OpenEntry`**: Using the parent class rather than
  `FileNotFoundException` avoids conflating file-system semantics with archive-entry semantics.
  All asserter catch clauses handle `IOException` uniformly.
