## Utilities Subsystem Design

### Overview

The Utilities subsystem provides shared helper functionality used by other subsystems. It
contains security-sensitive or otherwise reusable operations that do not belong to any specific
domain subsystem.

### Subsystem Contents

| Unit                 | File                    | Responsibility                                                         |
| :------------------- | :---------------------- | :--------------------------------------------------------------------- |
| `PathHelpers`        | `PathHelpers.cs`        | Safe path-combination utility with path-traversal protection.          |
| `TemporaryDirectory` | `TemporaryDirectory.cs` | Disposable temporary directory with safe path resolution and clean-up. |

### Subsystem Responsibilities

- Provide path utilities that safely combine paths while preventing path-traversal attacks.
- Reject relative paths containing `..` or absolute paths when a relative path is expected.
- Create uniquely-named temporary directories and delete them automatically on disposal.
- Ensure all file paths within a temporary directory remain within its boundary.

### Interfaces

#### Exposed

| Class / Member                                | Description                                                                              |
| :-------------------------------------------- | :--------------------------------------------------------------------------------------- |
| `PathHelpers.SafePathCombine(base, relative)` | Combines `base` and `relative`; throws `ArgumentException` if the result escapes `base`. |
| `TemporaryDirectory` *(constructor)*          | Creates a uniquely-named subdirectory under `Environment.CurrentDirectory`.              |
| `TemporaryDirectory.DirectoryPath`            | Full path to the temporary directory.                                                    |
| `TemporaryDirectory.GetFilePath(relative)`    | Resolves a relative path within the directory; creates intermediate subdirectories.      |
| `TemporaryDirectory.Dispose()`                | Deletes the temporary directory and all its contents.                                    |

#### Consumed

| Dependency                     | Usage                                                                         |
| :----------------------------  | :---------------------------------------------------------------------------- |
| .NET BCL (`Path`, `Directory`) | All path manipulation and file-system operations within both units.           |

### Design

`PathHelpers` and `TemporaryDirectory` collaborate in a layered pattern:

1. `TemporaryDirectory` delegates all path construction to `PathHelpers.SafePathCombine`, ensuring
   that both the directory name itself and every relative path passed to `GetFilePath` are safe.
2. `PathHelpers` performs validation independently of `TemporaryDirectory`, so it can be used
   directly by other subsystems (such as `SelfTest`) without going through `TemporaryDirectory`.

Neither unit holds references to `Context` or any other subsystem; they are pure utilities with no
awareness of the tool's execution state.

### Interactions with Other Subsystems

| Consumer  | Usage                                                                                     |
| :-------- | :---------------------------------------------------------------------------------------- |
| SelfTest  | Uses `TemporaryDirectory` and `PathHelpers.SafePathCombine` for fixture file management.  |
| Tests     | Uses `TemporaryDirectory` for isolated file-system fixtures in all test projects.         |

### Design Decisions

- **Static class for PathHelpers**: `PathHelpers` is a static utility class with no instance state, suitable
  for use anywhere in the codebase without injection.
- **Defense-in-depth validation**: Path safety is validated both before and after combining
  paths, guarding against edge cases that might bypass the initial checks.
- **`Environment.CurrentDirectory` over `Path.GetTempPath()`**: On macOS, `/tmp` is a symlink
  to `/private/tmp`. Using the current directory avoids path-comparison failures caused by
  symlink resolution. See *TemporaryDirectory Design* for details.
