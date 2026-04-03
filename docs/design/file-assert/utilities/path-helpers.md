# PathHelpers Design

## Overview

`PathHelpers` is a static utility class that provides a safe path-combination method. It
protects callers against path-traversal attacks by verifying the canonical combined path
stays within the base directory, regardless of the form of the relative path input.

## Class Structure

### SafePathCombine Method

```csharp
internal static string SafePathCombine(string basePath, string relativePath)
```

Combines `basePath` and `relativePath` safely, ensuring the resulting path remains within
the base directory.

**Validation steps:**

1. Reject null inputs via `ArgumentNullException.ThrowIfNull`.
2. Combine the paths with `Path.Combine`.
3. Compute the absolute canonical path of the base directory and derive a form with a trailing
   directory separator appended (e.g. `/home/user/project/`). The trailing separator prevents
   partial-segment false-positives such as `/base/dir` incorrectly matching `/base/dir2/...`.
4. Compute the absolute canonical path of the combined result.
5. Verify the combined path either equals the base directory or starts with the base path prefix
   using a platform-appropriate comparison (case-insensitive on Windows/macOS, case-sensitive on
   Linux); reject if it escapes the base directory.

## Design Decisions

- **Single canonical-path check**: The combined path is resolved to its absolute canonical form
  and verified to start with the base directory prefix (with trailing separator). This single
  check handles all traversal patterns — `../`, embedded `/../`, absolute paths, and edge-case
  platform path formats — without requiring multiple pre-combine inspections.
- **Trailing separator on base path**: Appending `Path.DirectorySeparatorChar` to the base
  before the `StartsWith` check prevents partial-segment false-positives (e.g. base `/a/b`
  incorrectly matching combined `/a/bc/file`).
- **Platform-appropriate comparison**: Case-insensitive on Windows and macOS; case-sensitive on
  Linux, matching each platform's file-system semantics.
- **ArgumentException on invalid input**: Callers receive a specific `ArgumentException`
  identifying `relativePath` as the problematic parameter, making debugging straightforward.
- **No logging or error accumulation**: `SafePathCombine` is a pure utility method that throws
  on invalid input; it does not interact with the `Context` or any output mechanism.
