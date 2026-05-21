### TemporaryDirectory Design

#### Overview

`TemporaryDirectory` is a disposable utility class that creates a uniquely-named temporary
directory under `Environment.CurrentDirectory` and deletes it automatically on disposal.
It uses `PathHelpers.SafePathCombine` internally to prevent path-traversal attacks on any
relative paths supplied by callers.

The directory is rooted at `Environment.CurrentDirectory` rather than `Path.GetTempPath()`
to avoid the macOS symlink issue where `/tmp` resolves to `/private/tmp`, which can cause
path-comparison failures when the OS returns the resolved (real) path instead of the symlink
path used to construct it.

#### Class Structure

##### Constructor

```csharp
public TemporaryDirectory()
```

Creates a uniquely-named subdirectory under `Environment.CurrentDirectory` using the pattern
`tmp-{Guid:N}`. The directory name is derived from a fresh `Guid` formatted with the `N`
specifier (32 lowercase hex digits, no hyphens).

**Steps:**

1. Capture `Environment.CurrentDirectory` as the effective base directory.
2. Call `PathHelpers.SafePathCombine(effectiveBase, $"tmp-{Guid.NewGuid():N}")` to produce
   the directory path.
3. Call `Directory.CreateDirectory(DirectoryPath)`.
4. Wrap any `IOException`, `UnauthorizedAccessException`, or `ArgumentException` in an
   `InvalidOperationException` with a descriptive message.

**Exceptions thrown:**

| Exception                  | Condition                                                   |
| :------------------------- | :---------------------------------------------------------- |
| `InvalidOperationException`| Directory could not be created (wraps the underlying cause) |

##### GetFilePath Method

```csharp
public string GetFilePath(string relativePath)
```

Returns the full path to a file within the temporary directory, creating any required
intermediate subdirectories.

**Steps:**

1. Call `PathHelpers.SafePathCombine(DirectoryPath, relativePath)` to produce the full path
   and guard against traversal attacks.
2. Obtain the parent directory with `Path.GetDirectoryName(path)`.
3. Call `Directory.CreateDirectory(directory)` if the parent directory is non-null.
4. Return the full path.

**Exceptions thrown:**

| Exception              | Condition                                                    |
| :--------------------- | :----------------------------------------------------------- |
| `ArgumentNullException`| `relativePath` is `null`                                     |
| `ArgumentException`    | `relativePath` would escape the temporary directory          |

##### Dispose Method

```csharp
public void Dispose()
```

Deletes the temporary directory and all its contents recursively. Cleanup failures are
treated as non-fatal: `IOException` and `UnauthorizedAccessException` are caught and
silently suppressed so that a failed cleanup does not propagate as an exception out of a
`using` block.

#### Design Decisions

- **`Environment.CurrentDirectory` over `Path.GetTempPath()`**: On macOS, `/tmp` is a
  symlink to `/private/tmp`. When paths are constructed with `/tmp` as a base but the OS
  later returns the real path `/private/tmp`, string-equality comparisons fail. Using
  `Environment.CurrentDirectory`, which is already fully resolved, avoids this class of
  failure entirely.
- **`tmp-{Guid:N}` name pattern**: The `N` format specifier produces 32 lowercase hex
  digits with no hyphens, which is a valid directory name on all supported platforms and
  is highly unlikely to collide with existing directories.
- **`InvalidOperationException` on construction failure**: Wrapping the underlying I/O
  exception in `InvalidOperationException` gives callers a single catch target for setup
  failures without exposing the low-level exception hierarchy as part of the public
  contract.
- **Suppressed exceptions in `Dispose`**: Temporary-directory cleanup is best-effort.
  Propagating cleanup errors out of a `using` block would mask the real exception that
  caused the `using` block to exit, so `IOException` and `UnauthorizedAccessException`
  are silently swallowed.

#### Purpose

`TemporaryDirectory` provides a safe, self-cleaning workspace for built-in tests and any
other caller that needs a short-lived directory isolated to the current working directory.
Its single responsibility is to own the lifecycle — creation and deletion — of that
directory.

#### Data Model

| Field           | Type     | Description                              |
| :-------------- | :------- | :--------------------------------------- |
| `DirectoryPath` | `string` | Full path to the temporary directory     |

#### Key Methods

| Method                                  |
| :-------------------------------------- |
| `TemporaryDirectory()` *(constructor)*  |
| `GetFilePath(string relativePath)`      |
| `Dispose()`                             |

#### Error Handling

| Scenario                                          |
| :------------------------------------------------ |
| Temporary directory creation failure              |
| Null or traversal-escaping `relativePath`         |
| Temporary directory deletion failure (suppressed) |

#### Interactions

- **Uses**: `PathHelpers.SafePathCombine` for all path construction, ensuring traversal
  safety on both the directory name and every relative path passed to `GetFilePath`.
- **Callers**:
  - `Validation` built-in tests — each test creates a `TemporaryDirectory` instance,
    writes fixture files via `GetFilePath`, and disposes it at the end of the test body.
  - Test project files (`*Tests.cs`) — use `TemporaryDirectory` for isolated file
    system fixtures in unit and integration tests.
