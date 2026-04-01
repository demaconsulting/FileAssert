# FileAssertFile Design

## Overview

The `FileAssertFile` class locates files on disk using a glob pattern, enforces
optional minimum and maximum count constraints, and applies a collection of
`FileAssertRule` instances to the text content of every matched file.

## Class Structure

### Properties

| Property  | Type                            | Description                                  |
| :-------- | :------------------------------ | :------------------------------------------- |
| `Pattern` | `string`                        | Glob pattern used to locate files.           |
| `Min`     | `int?`                          | Optional minimum number of matching files.   |
| `Max`     | `int?`                          | Optional maximum number of matching files.   |
| `Rules`   | `IReadOnlyList<FileAssertRule>` | Content rules applied to every matched file. |

### Factory Method

```csharp
internal static FileAssertFile Create(FileAssertFileData data)
```

The factory validates that `Pattern` is not null or whitespace before constructing
the instance. Rules are created via `FileAssertRule.Create` for each entry in the
data's rule list.

### Execution Method

```csharp
internal void Run(Context context, string basePath)
```

Execution proceeds in three phases:

1. **File discovery** — `Microsoft.Extensions.FileSystemGlobbing.Matcher` evaluates
   `Pattern` relative to `basePath` and returns the list of matched file paths.

2. **Count validation** — If `Min` is set and the match count is below it, an error
   is written and execution returns immediately. Likewise for `Max`. Early return
   prevents misleading content-rule errors when the count constraint already signals
   a failure.

3. **Content validation** — If any rules are defined, each matched file's text is
   read and every rule is applied in order.

## YAML Configuration

```yaml
files:
  - pattern: "**/*.cs"
    min: 1
    max: 100
    rules:
      - contains: "Copyright (c) DEMA Consulting"
```

All properties except `pattern` are optional.

## Design Decisions

- **Glob via FileSystemGlobbing**: The `Microsoft.Extensions.FileSystemGlobbing`
  library is already a project dependency and provides cross-platform glob support
  consistent with the rest of the .NET ecosystem.
- **Early return on count failure**: Reporting a count mismatch and stopping avoids
  cascading content-rule errors for files that should not exist at all.
- **Content loaded on demand**: File content is only read when at least one rule is
  defined, avoiding unnecessary I/O for count-only checks.
