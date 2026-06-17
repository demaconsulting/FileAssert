### FileAssertTest Design

#### Overview

The `FileAssertTest` class represents a named, tagged test within a FileAssert
configuration. It groups a collection of `FileAssertFile` assertions and supports
filter-based selection so that users can run targeted subsets of the test suite
from the command line.

#### Class Structure

##### Properties

| Property | Type                            | Description                                |
| :------- | :------------------------------ | :----------------------------------------- |
| `Name`   | `string`                        | Unique human-readable name for the test.   |
| `Tags`   | `IReadOnlyList<string>`         | Tags used for filter-based test selection. |
| `Files`  | `IReadOnlyList<FileAssertFile>` | File assertions belonging to this test.    |

##### Factory Method

```csharp
internal static FileAssertTest Create(FileAssertTestData data)
```

The factory validates that `Name` is not null or whitespace. An empty or null
`Tags` or `Files` list is treated as an empty collection rather than an error.
Passing a null `data` argument throws `ArgumentNullException`.

##### Filter Method

```csharp
internal bool MatchesFilter(IEnumerable<string> filters)
```

Returns `true` when:

- The `filters` collection is empty (run-all default), or
- Any filter string matches `Name` using `OrdinalIgnoreCase` comparison, or
- Any filter string matches any element of `Tags` using `OrdinalIgnoreCase` comparison.

##### Execution Method

```csharp
internal void Run(IContext context, string basePath)
```

Creates a `DirectoryFileContainer(basePath)` and then iterates `Files`, calling
`Run(context, container)` on each entry. Errors reported by individual file assertions
accumulate in the context and do not stop subsequent assertions from running. Passing a null
`context` or null `basePath` throws `ArgumentNullException`.

#### YAML Configuration

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

#### Design Decisions

- **Non-empty name required**: A name is required so that errors and run logs
  identify which test failed without ambiguity.
- **Case-insensitive filter matching**: Users should not need to know the exact
  casing of test names or tags when running from the command line.
- **Empty filter runs all tests**: Following the principle of least surprise, omitting
  filters from the command line executes the full suite rather than nothing.

#### Purpose

`FileAssertTest` represents a single named, tagged test within a FileAssert configuration.
Its single responsibility is to group a set of `FileAssertFile` assertions, evaluate
filter criteria for selective execution, and drive execution of its assertions.

#### Data Model

| Property | Type                            | Description                                     |
| :------- | :------------------------------ | :---------------------------------------------- |
| `Name`   | `string`                        | Required human-readable test identifier.        |
| `Tags`   | `IReadOnlyList<string>`         | Tags used for command-line filter selection.    |
| `Files`  | `IReadOnlyList<FileAssertFile>` | Ordered file assertions belonging to this test. |

#### Key Methods

| Method                                       | Purpose                                                         |
| :------------------------------------------- | :-------------------------------------------------------------- |
| `Create(FileAssertTestData data)`            | Validates `Name`; builds `FileAssertFile` list.                 |
| `MatchesFilter(IEnumerable<string> filters)` | Returns `true` if filters empty or any matches name or tag.     |
| `Run(IContext context, string basePath)`     | Wraps basePath in `DirectoryFileContainer`; runs each file.     |

#### Error Handling

| Scenario                                 | Handling                                             |
| :--------------------------------------- | :--------------------------------------------------- |
| Null `data` passed to `Create`           | `ArgumentNullException` thrown.                      |
| Null or whitespace `Name` in data        | `InvalidOperationException` thrown by `Create`.      |
| Null `filters` passed to `MatchesFilter` | `ArgumentNullException` thrown.                      |
| Null `context` or `basePath` in `Run`    | `ArgumentNullException` thrown.                      |
| Individual file assertion failures       | Accumulated in `context`; subsequent files continue. |

#### Interactions

- **Created by**: `FileAssertConfig.ReadFromFile` via `FileAssertTest.Create` for each
  `FileAssertTestData` entry.
- **Called by**: `FileAssertConfig.Run` — calls `MatchesFilter(filterList)` then
  `Run(context, basePath)` for each qualifying test.
- **Creates and owns**: `FileAssertFile` instances via `FileAssertFile.Create`.
- **Calls**: `FileAssertFile.Run(context, container)` for each file assertion, where `container`
  is a `DirectoryFileContainer` wrapping `basePath`.
- **OTS dependency**: `DirectoryFileContainer` (Utilities subsystem) for wrapping the base path.
