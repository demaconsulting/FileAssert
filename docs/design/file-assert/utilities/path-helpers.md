### PathHelpers Design

#### Overview

`PathHelpers` is a static utility class that provides a safe path-combination method. It
protects callers against path-traversal attacks by verifying the resolved combined path stays
within the base directory. Note that `Path.GetFullPath` normalizes `.`/`..` segments but does
not resolve symlinks or reparse points, so this check guards against string-level traversal
only.

#### Class Structure

##### SafePathCombine Method

```csharp
internal static string SafePathCombine(string basePath, string relativePath)
```

Combines `basePath` and `relativePath` safely, ensuring the resulting path remains within
the base directory.

**Validation steps:**

1. Reject null inputs via `ArgumentNullException.ThrowIfNull`.
2. Combine the paths with `Path.Combine` to produce the candidate path (preserving the
   caller's relative/absolute style).
3. Resolve both `basePath` and the candidate to absolute form with `Path.GetFullPath`.
4. Compute `Path.GetRelativePath(absoluteBase, absoluteCombined)` and reject the input if
   the result is exactly `".."`, starts with `".."` followed by `Path.DirectorySeparatorChar`
   or `Path.AltDirectorySeparatorChar`, or is itself rooted (absolute), which would indicate
   the combined path escapes the base directory.

#### Design Decisions

- **`Path.GetRelativePath` for containment check**: Using `GetRelativePath` to verify
  containment handles root paths (e.g. `/`, `C:\`), platform case-sensitivity, and
  directory-separator normalization natively. The containment test should treat `..` as an
  escaping segment only when it is the entire relative result or is followed by a directory
  separator, avoiding false positives for valid in-base names such as `..data`.
- **Post-combine canonical-path check**: Resolving paths after combining handles all traversal
  patterns — `../`, embedded `/../`, absolute-path overrides, and platform edge cases —
  without fragile pre-combine string inspection of `relativePath`.
- **ArgumentException on invalid input**: Callers receive a specific `ArgumentException`
  identifying `relativePath` as the problematic parameter, making debugging straightforward.
- **No logging or error accumulation**: `SafePathCombine` is a pure utility method that throws
  on invalid input; it does not interact with the `Context` or any output mechanism.

#### Purpose

`PathHelpers` provides a single safe path-combination utility. Its responsibility is to
prevent path-traversal attacks by verifying that the resolved combined path remains within
the specified base directory before returning it.

#### Data Model

N/A — `PathHelpers` is a `static` class with no instance state or fields.

#### Key Methods

| Method                                                  |
| :------------------------------------------------------ |
| `SafePathCombine(string basePath, string relativePath)` |

**Algorithm:**

1. Reject null inputs via `ArgumentNullException.ThrowIfNull`.
2. Produce `combinedPath = Path.Combine(basePath, relativePath)`.
3. Resolve both `basePath` and `combinedPath` to absolute form with `Path.GetFullPath`.
4. Compute `Path.GetRelativePath(absoluteBase, absoluteCombined)`.
5. Throw `ArgumentException` if the relative result equals `".."`, starts with `"../"` or
   `"..\\"`, or is itself rooted.

#### Error Handling

- **Null `basePath` or `relativePath`**: `ArgumentNullException` thrown immediately by
  `ArgumentNullException.ThrowIfNull`; not propagated further.
- **Combined path escapes base directory via `../` traversal**: `ArgumentException` thrown
  with message `"Invalid path component: {relativePath}"`; `relativePath` is named as the
  offending parameter so callers can identify the cause.
- **Path contains unsupported format**: `NotSupportedException` propagated from
  `Path.GetFullPath` or `Path.Combine`; not caught by this method.
- **Combined or resolved path exceeds system maximum length**: `PathTooLongException`
  propagated from `Path.GetFullPath`; not caught by this method.

#### Interactions

- **Callers**:
  - `TemporaryDirectory` — uses `SafePathCombine(Environment.CurrentDirectory, guid-name)` to
    create a temp directory path, and `SafePathCombine(DirectoryPath, relativePath)` inside
    `GetFilePath`.
  - `Validation` built-in tests — uses `tempDir.GetFilePath(fileName)` (which internally calls
    `SafePathCombine`) to build fixture file paths.
- **No internal FileAssert dependencies**: `PathHelpers` is a self-contained utility with no
  references to other units in the system.
