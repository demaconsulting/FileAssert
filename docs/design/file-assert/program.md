# Program Design

## Overview

`Program` is the entry point for the FileAssert tool. It owns the `Main` method, constructs a
`Context` from command-line arguments, dispatches to the appropriate handler based on context
flags, and returns the final exit code. It also exposes a `Version` property used by both the
version display path and the self-validation header.

## Class Structure

### Version Property

```csharp
public static string Version { get; }
```

Reads the informational version from the executing assembly's
`AssemblyInformationalVersionAttribute`. Falls back to the assembly version, then to `"0.0.0"`.

### Main Method

```csharp
private static int Main(string[] args)
```

Creates a `Context`, calls `Run`, and returns `context.ExitCode`. Catches `ArgumentException`
and `InvalidOperationException` to print expected error messages and return exit code `1`.
Unexpected exceptions are re-thrown after printing the message so that the runtime generates
an event-log entry.

### Run Method

```csharp
public static void Run(Context context)
```

Inspects context flags in the following priority order:

| Priority | Condition             | Action                                      |
| :------- | :-------------------- | :------------------------------------------ |
| 1        | `context.Version`     | Print version string; return.               |
| 2        | `context.Help`        | Print banner and usage; return.             |
| 3        | `context.Validate`    | Print banner; delegate to `Validation.Run`. |
| 4        | Default               | Print banner; delegate to `RunToolLogic`.   |

### RunToolLogic Method

```csharp
private static void RunToolLogic(Context context)
```

Resolves the configuration file from `context.ConfigFile` (defaulting to `.fileassert.yaml`;
overridden by `--config`). When the default configuration file is absent, it prints guidance
without setting an error. When an explicitly specified file is absent, it calls
`context.WriteError` to signal failure. When the file exists, it calls
`FileAssertConfig.ReadFromFile` and then passes `context.Filters` (the positional name-or-tag
arguments) to `config.Run` so that only matching tests are executed.

## Interactions with Other Units

| Dependency          | Usage                                                         |
| :------------------ | :------------------------------------------------------------ |
| `Context`           | Created by `Context.Create`; owns all I/O and exit code.      |
| `Validation`        | Invoked by `Run` when `--validate` is set.                    |
| `FileAssertConfig`  | Loaded from file and executed by `RunToolLogic`.              |

## Design Decisions

- **Public `Run` method**: `Run` is `public` so that unit tests and the self-validation tests
  can invoke it directly without starting a new process.
- **Exception hierarchy**: `ArgumentException` and `InvalidOperationException` are caught as
  expected error conditions; all other exceptions propagate to generate crash reports.
- **Version from assembly attribute**: Using `AssemblyInformationalVersionAttribute` allows the
  CI pipeline to inject the exact package version (including pre-release labels) at build time.
