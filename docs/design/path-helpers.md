# PathHelpers Design

## Overview

`PathHelpers` is a static utility class that provides a safe path-combination method. It
protects callers against path-traversal attacks by rejecting relative paths that contain `..`
or that are rooted (absolute) paths.

## Class Structure

### SafePathCombine Method

```csharp
internal static string SafePathCombine(string basePath, string relativePath)
```

Combines `basePath` and `relativePath` safely, ensuring the resulting path remains within
the base directory.

**Validation steps:**

1. Reject null inputs via `ArgumentNullException.ThrowIfNull`.
2. Reject `relativePath` values that contain `..` (path traversal).
3. Reject `relativePath` values that are rooted (absolute paths).
4. Combine the paths with `Path.Combine`.
5. Compute the full (canonical) paths of both base and combined paths.
6. Use `Path.GetRelativePath` to verify the combined path is still under the base; reject if
   it escapes the base directory.

## Design Decisions

- **Two-phase validation**: The pre-combine check (steps 2–3) catches obvious traversal
  attempts. The post-combine check (steps 5–6) adds defense-in-depth against edge cases that
  bypass the initial checks on exotic file systems or path formats.
- **ArgumentException on invalid input**: Callers receive a specific `ArgumentException`
  identifying `relativePath` as the problematic parameter, making debugging straightforward.
- **No logging or error accumulation**: `SafePathCombine` is a pure utility method that throws
  on invalid input; it does not interact with the `Context` or any output mechanism.
