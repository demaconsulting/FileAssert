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
