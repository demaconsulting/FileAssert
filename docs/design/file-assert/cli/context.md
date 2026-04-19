# Context Design

## Overview

`Context` is the command-line argument parser and I/O owner for FileAssert. It translates the
raw `string[]` argument array into named properties, manages the optional log file stream, and
provides a unified interface for writing output and errors throughout the tool's execution.

## Class Structure

### Properties

| Property               | Type                    | Description                                                   |
| :--------------------- | :---------------------- | :------------------------------------------------------------ |
| `Version`              | `bool`                  | Set when `--version` or `-v` is present.                      |
| `Help`                 | `bool`                  | Set when `--help`, `-h`, or `-?` is present.                  |
| `Silent`               | `bool`                  | Set when `--silent` is present.                               |
| `Validate`             | `bool`                  | Set when `--validate` is present.                             |
| `Depth`                | `int`                   | Set via `--depth`; defaults to `1`. Range: 1–6.              |
| `ResultsFile`          | `string?`               | Path provided via `--results`, or null.                       |
| `ConfigFile`           | `string`                | Path provided via `--config`; defaults to `.fileassert.yaml`. |
| `IsConfigFileExplicit` | `bool`                  | True when `--config` was explicitly specified.                |
| `Filters`              | `IReadOnlyList<string>` | Positional arguments treated as test name or tag filters.     |
| `ExitCode`             | `int`                   | Returns `1` if any errors have been reported; otherwise `0`.  |
| `ErrorCount`           | `int`                   | Monotonically increasing count of `WriteError` calls.         |

### Factory Method

```csharp
public static Context Create(string[] args)
```

Delegates argument parsing to the private `ArgumentParser` nested class. Opens a log file if
`--log` was specified. Returns the fully initialized `Context` instance.

### Output Methods

```csharp
public void WriteLine(string message)
public void WriteError(string message)
```

`WriteLine` writes to stdout and the log file (unless `--silent` suppresses console output).
`WriteError` sets the internal error flag, writes to stderr in red (unless silent), and writes
to the log file.

### Argument Parsing

The private nested class `ArgumentParser` processes each argument in order:

- Flag arguments (starting with `--` or `-`) are matched by a `switch` statement.
- Arguments requiring a value (`--log`, `--results`, `--config`, `--depth`) consume the next element
  from the argument array and throw `ArgumentException` if no value follows.
- The `--depth` argument additionally validates that the value is an integer between 1 and 6.
- Unknown flag arguments (starting with `-`) throw `ArgumentException`.
- All other arguments are accumulated in the `Filters` list.

## Design Decisions

- **Sealed with IDisposable**: The class is sealed to prevent inheritance of internal state, and
  implements `IDisposable` to ensure the log file stream is always closed.
- **Factory method**: The `Create` factory method is `public` so tests and the self-validation
  tests can construct a context directly without invoking `Main`.
- **Error flag over exception**: `WriteError` sets a flag rather than throwing, so the tool
  completes all assertions before reporting a final failure via the exit code.
- **ErrorCount for per-test tracking**: The `ErrorCount` property increments monotonically so
  that callers running multiple named tests can snapshot the count before each test and compare
  after to derive a per-test pass/fail outcome without requiring a separate context per test.
