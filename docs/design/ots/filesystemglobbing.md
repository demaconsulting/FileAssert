## FileSystemGlobbing OTS Design

Microsoft.Extensions.FileSystemGlobbing is the glob pattern-matching library used by FileAssert.

### Purpose

FileSystemGlobbing is chosen to resolve file-assertion patterns (for example `**/*.dll`) against the
set of candidate files in a directory or container. It provides the standard .NET implementation of
include/exclude glob matching, removing the need for a custom wildcard matcher.

### Features Used

- `Matcher` configured with include patterns to match candidate file paths.
- Recursive-wildcard (`**`), single-segment wildcard (`*`), and exact-path matching.
- Evaluation of patterns against an in-memory list of entry paths supplied by the file container.

### Integration Pattern

FileSystemGlobbing is referenced as a NuGet package by the main `DemaConsulting.FileAssert` project.
The file assertion unit builds a `Matcher` from the configured pattern and runs it against the entry
paths exposed by the active `IFileContainer` (directory or zip archive). The matched set drives the
count constraints (min/max/exact) for the assertion.
