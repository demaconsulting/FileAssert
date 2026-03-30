# Validation Design

## Overview

`Validation` is a static class that implements the self-validation test runner for FileAssert.
It runs a series of built-in tests that exercise the tool's core functionality, prints a
structured report to the context output, and optionally writes results to a TRX or JUnit XML
file.

## Class Structure

### Run Method

```csharp
public static void Run(Context context)
```

Entry point for self-validation. Executes the following steps:

1. Prints a system information table (tool version, machine name, OS, .NET runtime, timestamp).
2. Creates a `TestResults` collection.
3. Runs each built-in test, adding results to the collection.
4. Prints a pass/fail summary.
5. Writes the results file if `context.ResultsFile` is set.

### Built-in Tests

| Test Name                   | Description                                                       |
| :-------------------------- | :---------------------------------------------------------------- |
| `FileAssert_VersionDisplay` | Runs `--version`; verifies log contains a version string.         |
| `FileAssert_HelpDisplay`    | Runs `--help`; verifies log contains `"Usage:"` and `"Options:"`. |

Each test:

1. Creates a temporary directory.
2. Builds a `Context` with `--silent` and `--log` pointing to a file in that directory.
3. Calls `Program.Run`.
4. Reads the log file and asserts its contents.
5. Records the outcome in the `TestResults` collection.

### Results Serialization

```csharp
private static void WriteResultsFile(Context context, TestResults testResults)
```

Writes the collected results to the file specified by `context.ResultsFile`:

- `.trx` extension → TRX format via `TrxSerializer.Serialize`.
- `.xml` extension → JUnit XML format via `JUnitSerializer.Serialize`.
- Other extensions → error written to context.

### TemporaryDirectory Helper

A private nested `IDisposable` class that creates a unique temporary directory on construction
and deletes it recursively on disposal. Uses `PathHelpers.SafePathCombine` to build the
directory path under `Path.GetTempPath()`.

## Design Decisions

- **Generic exception catch in test methods**: Each built-in test wraps its body in a
  `try/catch (Exception)` to record any unexpected exception as a test failure rather than
  crashing the self-validation run.
- **Separation of pass/fail summary**: The pass/fail counts are printed only after all tests
  complete, so the summary reflects the full run.
- **`Program.Run` as the test target**: Using the public `Run` method rather than the private
  `Main` method allows tests to capture the log output without spawning a subprocess.
