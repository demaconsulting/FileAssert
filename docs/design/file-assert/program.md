## Program Design

### Purpose

`Program` is the static entry-point class for the FileAssert tool. It constructs the execution
`Context` from command-line arguments, dispatches to the appropriate handler (version display,
help, self-validation, or main tool logic), and returns the final process exit code. It also
exposes a `Version` property used by both the version display path and the self-validation header.

### Data Model

N/A – `Program` is a static entry-point class with no instance state.

### Key Methods

#### Version Property

```csharp
public static string Version { get; }
```

**Purpose**: Provides the application version string for display and self-test headers.

**Algorithm**: Reads the informational version from the executing assembly's
`AssemblyInformationalVersionAttribute`. Falls back to the assembly version string if the
attribute is absent, then to `"0.0.0"` if neither is available.

**Preconditions**: None.

**Post-conditions**: Returns a non-null, non-empty string representing the application version.

#### Main Method

```csharp
private static int Main(string[] args)
```

**Purpose**: Operating-system entry point; creates execution context, delegates to `Run`, and
returns the process exit code.

**Algorithm**: Creates a `Context` via `Context.Create(args)`, calls `Run(context)`, and returns
`context.ExitCode`. Catches `ArgumentException` and `InvalidOperationException` to print their
messages to standard error and return exit code `1`. Any other exception is printed to standard
error and re-thrown so that the runtime generates a crash-report event-log entry.

**Preconditions**: `args` is the raw command-line argument array supplied by the runtime (may be
empty; must not be null).

**Post-conditions**: Returns `0` for success or a non-zero value for failure.

#### Run Method

```csharp
public static void Run(Context context)
```

**Purpose**: Dispatches to the appropriate execution path based on the parsed context flags.
Declared `public` so that unit tests and the self-validation suite can exercise it without
spawning a child process.

**Algorithm**: Inspects context flags in the following priority order:

| Priority | Condition          | Action                           |
| :------- | :----------------- | :------------------------------- |
| 1        | `context.Version`  | Write `Version` and return.      |
| 2        | `context.Help`     | Print banner, print help, return.|
| 3        | `context.Validate` | Print banner, run validation.    |
| 4        | Otherwise          | Print banner, run tool logic.    |

**Preconditions**: `context` must not be null.

**Post-conditions**: Appropriate output has been written to `context`; `context.ExitCode` reflects
the outcome of the dispatched path.

#### PrintBanner Method

```csharp
private static void PrintBanner(Context context)
```

**Purpose**: Writes the application name, version, and copyright notice to the context output.

**Algorithm**: Writes three lines: the versioned tool name, the copyright notice, and a blank
separator line.

**Preconditions**: `context` must not be null.

**Post-conditions**: Banner lines have been written to `context`.

#### PrintHelp Method

```csharp
private static void PrintHelp(Context context)
```

**Purpose**: Writes usage information and available options to the context output.

**Algorithm**: Writes the usage line followed by a formatted table of all supported options.

**Preconditions**: `context` must not be null.

**Post-conditions**: Help text has been written to `context`.

#### RunToolLogic Method

```csharp
private static void RunToolLogic(Context context)
```

**Purpose**: Loads the YAML configuration file and runs all matching assertions.

**Algorithm**: Resolves the configuration file from `context.ConfigFile` (defaulting to
`.fileassert.yaml`; overridden by `--config`). When the default configuration file is absent,
prints guidance without setting an error and returns. When an explicitly specified file is absent,
calls `context.WriteError` to signal failure. When the file exists, calls
`FileAssertConfig.ReadFromFile` and then passes `context.Filters` to `config.Run` so that only
matching tests are executed.

**Preconditions**: `context` must not be null.

**Post-conditions**: Either an error has been reported via `context.WriteError`, guidance has been
written for a missing default config, or all matching assertions have been executed and their
outcomes reflected in `context.ExitCode`.

### Error Handling

| Exception                   | Detection                                                   | Handling                                                               |
| :-------------------------- | :---------------------------------------------------------- | :--------------------------------------------------------------------- |
| `ArgumentException`         | Invalid arguments detected by `Context.Create` or callee    | Caught in `Main`; message printed to standard error; exit code `1`     |
| `InvalidOperationException` | Invalid state detected by downstream code                   | Caught in `Main`; message printed to standard error; exit code `1`     |
| All other exceptions        | Unexpected runtime failure (programming error or I/O fault) | Printed to standard error and re-thrown; runtime generates crash log   |

### Dependencies

- `Context` provides the parsed flags, config path, filters, and output methods used by every
  execution path.
- `Validation` runs the built-in self-validation flow when `context.Validate` is set.
- `FileAssertConfig` loads the YAML configuration and executes matching assertions during normal
  tool runs.

### Callers

- None.

### Design Decisions

- **Public `Run` method**: `Run` is `public` so that unit tests and the self-validation tests
  can invoke it directly without starting a new process.
- **Exception hierarchy**: `ArgumentException` and `InvalidOperationException` are caught as
  expected error conditions; all other exceptions propagate to generate crash reports.
- **Version from assembly attribute**: Using `AssemblyInformationalVersionAttribute` allows the
  CI pipeline to inject the exact package version (including pre-release labels) at build time.
