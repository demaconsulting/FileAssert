### Context Design

#### Overview

`Context` is the command-line argument parser and I/O owner for FileAssert. It translates the
raw `string[]` argument array into named properties, manages the optional log file stream, and
provides a unified interface for writing output and errors throughout the tool's execution.

#### Class Structure

##### Properties

| Property               | Type                    | Description                                                   |
| :--------------------- | :---------------------- | :------------------------------------------------------------ |
| `Version`              | `bool`                  | Set when `--version` or `-v` is present.                      |
| `Help`                 | `bool`                  | Set when `--help`, `-h`, or `-?` is present.                  |
| `Silent`               | `bool`                  | Set when `--silent` is present.                               |
| `Validate`             | `bool`                  | Set when `--validate` is present.                             |
| `Depth`                | `int`                   | Set via `--depth`; defaults to `1`. Range: 1–6.               |
| `ResultsFile`          | `string?`               | Path provided via `--results`, or null.                       |
| `ConfigFile`           | `string`                | Path provided via `--config`; defaults to `.fileassert.yaml`. |
| `IsConfigFileExplicit` | `bool`                  | True when `--config` was explicitly specified.                |
| `Filters`              | `IReadOnlyList<string>` | Positional arguments treated as test name or tag filters.     |
| `ExitCode`             | `int`                   | Returns `1` if any errors have been reported; otherwise `0`.  |
| `ErrorCount`           | `int`                   | Monotonically increasing count of `WriteError` calls.         |

##### Factory Method

```csharp
public static Context Create(string[] args)
```

Delegates argument parsing to the private `ArgumentParser` nested class. Opens a log file if
`--log` was specified. Returns the fully initialized `Context` instance.

##### Output Methods

```csharp
public void WriteLine(string message)
public void WriteError(string message)
public IContext WithPrefix(string prefix)
```

`WriteLine` writes to stdout and the log file (unless `--silent` suppresses console output).
`WriteError` sets the internal error flag, writes to stderr in red (unless silent), and writes
to the log file.
`WithPrefix` creates and returns a new `ScopedContext` that prepends `"{prefix} > "` to every
`WriteError` message before delegating to this context. The prefix must not be null.

##### ScopedContext Nested Class

`ScopedContext` is a private sealed class nested inside `Context`. It implements `IContext` and
holds a reference to its parent `IContext` and a prefix string:

- `WriteLine` — delegated unchanged to the parent context.
- `WriteError` — the message is rewritten as `"{_prefix} > {message}"` before passing to the
  parent, so all errors bubble up through the chain and reach the root context's `_hasErrors`
  and `_errorCount` fields.
- `WithPrefix` — creates a further-nested `ScopedContext(this, prefix)`, enabling arbitrarily
  deep breadcrumb chains (e.g., `"outer.zip > inner.zip > error message"`).

`ScopedContext` is used by `FileAssertZipAssert.Run` to scope every error message with the
archive entry's display path before passing the context to nested `FileAssertFile` assertions.

##### Argument Parsing

The private nested class `ArgumentParser` processes each argument in order:

- Flag arguments (starting with `--` or `-`) are matched by a `switch` statement.
- Arguments requiring a value (`--log`, `--results`, `--config`, `--depth`) consume the next element
  from the argument array and throw `ArgumentException` if no value follows.
- The `--depth` argument additionally validates that the value is an integer between 1 and 6.
- Unknown flag arguments (starting with `-`) throw `ArgumentException`.
- All other arguments are accumulated in the `Filters` list.

#### Design Decisions

- **Implements `IContext`**: `Context` implements the `IContext` interface, which is also
  implemented by `ScopedContext`. This allows all asserters to accept `IContext` rather than the
  concrete `Context`, enabling `FileAssertZipAssert` to supply a scoped context without those
  asserters requiring any knowledge of the scoping mechanism.
- **Sealed with IDisposable**: The class is sealed to prevent inheritance of internal state, and
  implements `IDisposable` to ensure the log file stream is always closed.
- **Factory method**: The `Create` factory method is `public` so tests and the self-validation
  tests can construct a context directly without invoking `Main`.
- **Error flag over exception**: `WriteError` sets a flag rather than throwing, so the tool
  completes all assertions before reporting a final failure via the exit code.
- **ErrorCount for per-test tracking**: The `ErrorCount` property increments monotonically so
  that callers running multiple named tests can snapshot the count before each test and compare
  after to derive a per-test pass/fail outcome without requiring a separate context per test.

#### Purpose

`Context` is the single owner of all command-line argument state, log-file I/O, and
error-count bookkeeping for a FileAssert execution. It provides a unified output interface
so that every assertion unit writes errors through one reporting path rather than directly
to the console.

#### Data Model

| Field / Property       | Type                    | Description                                                   |
| :--------------------- | :---------------------- | :------------------------------------------------------------ |
| `_logWriter`           | `StreamWriter?`         | Open log-file stream; `null` when `--log` was not specified.  |
| `_hasErrors`           | `bool`                  | Set on the first `WriteError` call; drives `ExitCode`.        |
| `_errorCount`          | `int`                   | Monotonically increasing count of `WriteError` calls.         |
| `Version`              | `bool`                  | `true` when `--version` / `-v` is present.                    |
| `Help`                 | `bool`                  | `true` when `--help` / `-h` / `-?` is present.                |
| `Silent`               | `bool`                  | `true` when `--silent` is present.                            |
| `Validate`             | `bool`                  | `true` when `--validate` is present.                          |
| `Depth`                | `int`                   | Markdown heading depth; defaults to `1`.                      |
| `ResultsFile`          | `string?`               | Path for TRX/JUnit results output; `null` if not specified.   |
| `ConfigFile`           | `string`                | Config file path; defaults to `.fileassert.yaml`.             |
| `IsConfigFileExplicit` | `bool`                  | `true` when `--config` was explicitly provided.               |
| `Filters`              | `IReadOnlyList<string>` | Positional arguments treated as test name/tag filters.        |
| `ExitCode`             | `int`                   | `1` if any errors have been reported; otherwise `0`.          |
| `ErrorCount`           | `int`                   | Read-only view of `_errorCount`.                              |

#### Key Methods

| Method                                    | Purpose                                                             |
| :---------------------------------------- | :------------------------------------------------------------------ |
| `Context.Create(string[])`                | Factory: parses args, opens log file, returns initialized instance. |
| `WriteLine(string)`                       | Writes to stdout (unless silent) and log file.                      |
| `WriteError(string)`                      | Sets error flag and counter; writes to stderr/log (unless silent).  |
| `WithPrefix(string) → IContext`           | Returns a new `ScopedContext` prepending the given prefix to errors.|
| `Dispose()`                               | Closes and disposes the log-file stream writer.                     |
| `ArgumentParser.ParseArguments(string[])` | Inner class: translates argument array into named parser state.     |

#### Error Handling

| Scenario                                  | Handling                                                      |
| :---------------------------------------- | :------------------------------------------------------------ |
| Null `args` passed to `Create`            | `ArgumentNullException` thrown before parsing begins.         |
| Unknown flag argument (starts with `-`)   | `ArgumentException` propagated to the caller of `Create`.     |
| Value-requiring flag with no value        | `ArgumentException` propagated to the caller of `Create`.     |
| `--depth` value not in 1–6 range          | `ArgumentException` propagated to the caller of `Create`.     |
| Log file cannot be opened                 | `InvalidOperationException` wrapping the underlying I/O.      |
| Null `prefix` passed to `WithPrefix`      | `ArgumentNullException` thrown before `ScopedContext` created.|
| Assertion or rule failure at runtime      | Handled by `WriteError`; no throw — errors in `_errorCount`.  |

#### Interactions

- **Callers**: `Program.RunToolLogic` constructs a `Context` via `Context.Create` and passes it
  to all execution paths. `Validation.Run` constructs additional `Context` instances to drive
  self-test scenarios.
- **Consumers**: `FileAssertConfig.Run`, `FileAssertTest.Run`, `FileAssertFile.Run`, and every
  assert unit (`FileAssertTextAssert`, `FileAssertPdfAssert`, `FileAssertXmlAssert`,
  `FileAssertHtmlAssert`, `FileAssertYamlAssert`, `FileAssertJsonAssert`, `FileAssertZipAssert`)
  receive an `IContext` reference and call `WriteLine` / `WriteError` to report results.
  `FileAssertZipAssert` additionally calls `context.WithPrefix(displayPath)` to derive a scoped
  context for nested zip entry assertions.
- **Internal dependency**: `ArgumentParser` (private nested class) is used exclusively by
  `Context.Create`. `ScopedContext` (private nested class) is returned by `WithPrefix`.
