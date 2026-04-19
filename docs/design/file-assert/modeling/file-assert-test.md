# FileAssertTest Design

## Overview

The `FileAssertTest` class represents a named, tagged test within a FileAssert
configuration. It groups a collection of `FileAssertFile` assertions and supports
filter-based selection so that users can run targeted subsets of the test suite
from the command line.

## Class Structure

### Properties

| Property | Type                            | Description                                |
| :------- | :------------------------------ | :----------------------------------------- |
| `Name`   | `string`                        | Unique human-readable name for the test.   |
| `Tags`   | `IReadOnlyList<string>`         | Tags used for filter-based test selection. |
| `Files`  | `IReadOnlyList<FileAssertFile>` | File assertions belonging to this test.    |

### Factory Method

```csharp
internal static FileAssertTest Create(FileAssertTestData data)
```

The factory validates that `Name` is not null or whitespace. An empty or null
`Tags` or `Files` list is treated as an empty collection rather than an error.
Passing a null `data` argument throws `ArgumentNullException`.

### Filter Method

```csharp
internal bool MatchesFilter(IEnumerable<string> filters)
```

Returns `true` when:

- The `filters` collection is empty (run-all default), or
- Any filter string matches `Name` using `OrdinalIgnoreCase` comparison, or
- Any filter string matches any element of `Tags` using `OrdinalIgnoreCase` comparison.

### Execution Method

```csharp
internal void Run(Context context, string basePath)
```

Iterates `Files` and calls `Run(context, basePath)` on each entry. Errors reported
by individual file assertions accumulate in the context and do not stop subsequent
assertions from running. Passing a null `context` or null `basePath` throws
`ArgumentNullException`.

## YAML Configuration

```yaml
tests:
  - name: "License Headers"
    tags:
      - license
      - smoke
    files:
      - pattern: "**/*.cs"
        min: 1
        text:
          - contains: "Copyright (c) DEMA Consulting"
```

## Design Decisions

- **Non-empty name required**: A name is required so that errors and run logs
  identify which test failed without ambiguity.
- **Case-insensitive filter matching**: Users should not need to know the exact
  casing of test names or tags when running from the command line.
- **Empty filter runs all tests**: Following the principle of least surprise, omitting
  filters from the command line executes the full suite rather than nothing.
