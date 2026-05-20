### Validation Design

#### Overview

`Validation` is a static class that implements the self-validation test runner for FileAssert.
It runs a series of built-in tests that exercise the tool's core functionality, prints a
structured report to the context output, and optionally writes results to a TRX or JUnit XML
file.

#### Class Structure

##### Run Method

```csharp
public static void Run(Context context)
```

Entry point for self-validation. Executes the following steps:

1. Prints a system information table (tool version, machine name, OS, .NET runtime, timestamp)
   with a markdown heading at the depth specified by `context.Depth`.
2. Creates a `TestResults` collection.
3. Runs each built-in test, adding results to the collection.
4. Prints a pass/fail summary.
5. Writes the results file if `context.ResultsFile` is set.

##### Built-in Tests

| Test Name                   |
| :-------------------------- |
| `FileAssert_VersionDisplay` |
| `FileAssert_HelpDisplay`    |
| `FileAssert_Results`        |
| `FileAssert_Exists`         |
| `FileAssert_Contains`       |

Each test is dispatched via `RunValidationTest`, which handles the common boilerplate:

1. Creates a `TestResult` and records the start time.
2. Invokes the test body (`Func<string?>`).
3. Interprets a `null` return as pass and a non-null string as a failure message.
4. Records the outcome in the `TestResults` collection.
5. Catches any unhandled exception and records it as a test failure.

Each test body:

1. Creates a temporary directory via `TemporaryDirectory`.
2. Writes any required fixture files and a `.fileassert.yaml` configuration.
3. Builds a `Context` with `--silent` and `--config` (and optionally `--log` or `--results`).
4. Calls `Program.Run` and checks the exit code and/or output files.
5. Returns `null` on success or an error message string on failure.

##### RunValidationTest Helper

```csharp
private static void RunValidationTest(
    Context context,
    TestResults testResults,
    string testName,
    Func<string?> testBody)
```

Central dispatcher for all built-in tests. Executes `testBody`, maps its return value to
a pass/fail outcome, logs the result to the context, handles unhandled exceptions, and adds
the `TestResult` to the collection.

##### Results Serialization

```csharp
private static void WriteResultsFile(Context context, TestResults testResults)
```

Writes the collected results to the file specified by `context.ResultsFile`:

- `.trx` extension → TRX format via `TrxSerializer.Serialize`.
- `.xml` extension → JUnit XML format via `JUnitSerializer.Serialize`.
- Other extensions → error written to context.
- Any I/O or other exception is caught and an error message is written to context.

#### Design Decisions

- **`RunValidationTest` dispatcher**: All built-in tests share a single helper that owns the
  try/catch, pass/fail recording, logging, and result finalization. Each test body only needs to
  return `null` (pass) or an error message string (fail), eliminating repeated boilerplate.
- **Separation of pass/fail summary**: The pass/fail counts are printed only after all tests
  complete, so the summary reflects the full run.
- **`Program.Run` as the test target**: Using the public `Run` method rather than the private
  `Main` method allows tests to capture the log output without spawning a subprocess.

#### Purpose

`Validation` is the self-validation test runner for FileAssert. Its single responsibility is
to exercise the tool's core functionality using built-in test scenarios, print a structured
pass/fail report, and optionally serialize the results to a TRX or JUnit XML file.

#### Data Model

N/A — `Validation` is a `static` class with no instance fields. All state is local to the
`Run` method call. Temporary directory management is delegated to the standalone
`TemporaryDirectory` utility class.

#### Key Methods

| Method                                                                       |
| :--------------------------------------------------------------------------- |
| `Run(Context context)` *(public)*                                            |
| `RunVersionTest(Context, TestResults)` *(private)*                           |
| `RunHelpTest(Context, TestResults)` *(private)*                              |
| `RunResultsTest(Context, TestResults)` *(private)*                           |
| `RunExistsTest(Context, TestResults)` *(private)*                            |
| `RunContainsTest(Context, TestResults)` *(private)*                          |
| `RunValidationTest(Context, TestResults, string, Func<string?>)` *(private)* |
| `WriteResultsFile(Context, TestResults)` *(private)*                         |

#### Error Handling

| Scenario                                |
| :-------------------------------------- |
| Null `context` passed to `Run`          |
| Test body throws an unhandled exception |
| Unsupported results file extension      |
| Results file write failure              |
| Temporary directory creation failure    |
| Temporary directory deletion failure    |

#### Interactions

- **Caller**: `Program.Run` calls `Validation.Run(context)` when `context.Validate` is `true`.
- **Calls internally**:
  - `Program.Run(Context)` to execute each built-in test scenario in-process.
  - `Context.Create(string[])` to construct per-test contexts with `--silent` and `--config`.
  - `TemporaryDirectory.GetFilePath` to build all fixture and log file paths safely.
  - `DemaConsulting.TestResults.IO.TrxSerializer.Serialize` and `JUnitSerializer.Serialize` for
    results serialization.
- **OTS dependencies**: `System.Runtime.InteropServices.RuntimeInformation` for system info
  output; `System.Text.RegularExpressions.Regex` (source-generated) for version string matching.
