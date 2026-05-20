## Cli Subsystem Design

### Overview

The Cli subsystem is responsible for translating the raw command-line argument array into a
structured, immutable context object that the rest of the tool uses for output, configuration,
and execution decisions.

### Subsystem Contents

| Unit      | File          | Responsibility                                            |
| :-------- | :------------ | :-------------------------------------------------------- |
| `Context` | `Context.cs`  | Parses arguments and owns all I/O operations.             |

### Subsystem Responsibilities

- Parse all supported flags (`--version`/`-v`, `--help`/`-h`/`-?`, `--silent`, `--validate`, `--log`,
  `--results`/`--result`, `--config`, `--depth`) and positional filter arguments.
- Reject unknown or malformed arguments with a descriptive `ArgumentException`.
- Open and manage a log file when `--log` is specified.
- Write output to stdout and the log file; write errors to stderr and the log file.
- Expose an exit code that reflects whether any errors have been reported.

### Interfaces

The `Context` unit exposes the following public interface:

#### Properties

| Property       | Type                    | Description                                                     |
| :------------- | :---------------------- | :-------------------------------------------------------------- |
| `Silent`       | `bool`                  | `true` when `--silent` was given; suppresses console output.    |
| `Validate`     | `bool`                  | `true` when `--validate` was specified.                         |
| `Version`      | `bool`                  | `true` when `--version`/`-v` was specified.                     |
| `Help`         | `bool`                  | `true` when `--help`/`-h`/`-?` was specified.                   |
| `Depth`        | `int`                   | Heading depth for validation output (1–6, default 1).           |
| `ConfigFile`   | `string`                | Configuration file path (default `.fileassert.yaml`).           |
| `ResultsFile`  | `string?`               | Results file path; `null` if `--results` was not specified.     |
| `Filters`      | `IReadOnlyList<string>` | Positional name-or-tag filter arguments.                        |
| `ExitCode`     | `int`                   | `0` when no errors have been reported; `1` otherwise.           |

#### Methods

| Method       | Signature                              | Description                                                  |
| :----------- | :------------------------------------- | :----------------------------------------------------------- |
| `Create`     | `static Context Create(string[] args)` | Parses args; opens log file when `--log` is specified.       |
| `WriteLine`  | `void WriteLine(string message)`       | Writes to stdout (unless silent) and to the log file.        |
| `WriteError` | `void WriteError(string message)`      | Sets `_hasErrors`; writes to stderr (unless silent) and log. |
| `Dispose`    | `void Dispose()`                       | Closes the log file writer.                                  |

#### Environmental Resources

| Resource              | Direction | Description                                                           |
| :-------------------- | :-------- | :-------------------------------------------------------------------- |
| Standard output       | Output    | Receives all `WriteLine` messages when `--silent` is not set.         |
| Standard error        | Output    | Receives all `WriteError` messages when `--silent` is not set.        |
| Command-line arguments| Input     | Parsed by the internal `ArgumentParser` nested class.                 |
| File system (log)     | Output    | Log file opened for writing when `--log <path>` is specified.         |
| File system (config)  | Input     | Configuration file path resolved from `--config` or default.          |
| File system (results) | Output    | Results file path; written by the Configuration subsystem.            |

### Design

The `Context` class uses the following collaboration flow:

1. `Context.Create` is called with the raw `string[]` argument array.
2. An internal `ArgumentParser` instance iterates the array, setting boolean flags
   (`Silent`, `Validate`, `Version`, `Help`) and collecting string values (`ConfigFile`,
   `ResultsFile`, `LogFile`) and positional filter arguments.
3. All recognized values are transferred to the immutable `Context` instance via
   `private init` property accessors; unrecognized flags starting with `-` throw
   an `ArgumentException`.
4. If a log file path was provided, `OpenLogFile` opens a `StreamWriter` with
   `AutoFlush = true` on the specified path.
5. All subsequent output dispatches through `WriteLine` and `WriteError`:
   - If `Silent` is `false`, messages are written to `Console.Out` / `Console.Error`.
   - If a log writer is open, messages are always written to it regardless of `Silent`.
6. `WriteError` additionally sets the internal `_hasErrors` flag, causing `ExitCode`
   to return `1` for the remainder of the context's lifetime.

### Interactions with Other Subsystems

| Consumer          | Usage                                                                |
| :---------------- | :------------------------------------------------------------------- |
| Program           | Creates a `Context` and passes it to all downstream operations.      |
| Configuration     | Receives a `Context` to report errors and write progress output.     |
| Modeling          | Receives a `Context` to write error messages for assertion failures. |
| SelfTest          | Receives a `Context` to write validation results and errors.         |

### Design Decisions

- **Immutable context object**: Properties are set once via `private init` accessors, preventing
  accidental mutation after the context is created.
- **Internal ArgumentParser helper**: Argument parsing is encapsulated in a private nested
  class, keeping the public `Context` interface focused on output and state rather than parsing.
- **AutoFlush log writer**: The log file stream is opened with `AutoFlush = true` so that log
  entries are written to disk immediately, even if the process terminates unexpectedly.
