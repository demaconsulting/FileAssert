## SelfTest Subsystem Design

### Overview

The SelfTest subsystem provides built-in self-validation functionality that verifies the core
behavior of the tool at run time. It is invoked via the `--validate` command-line flag and
produces structured test results that can be written to a TRX or JUnit XML file.

### Subsystem Contents

| Unit         | File            | Responsibility                                           |
| :----------- | :-------------- | :------------------------------------------------------- |
| `Validation` | `Validation.cs` | Runs built-in self-validation tests and reports results. |

### Subsystem Responsibilities

- Execute a set of built-in test cases that exercise core tool functionality.
- Collect and summarize test outcomes (passed, failed).
- Optionally serialize results to TRX or JUnit XML format.
- Report a system information header before running tests.

### Interactions with Other Subsystems

| Dependency  | Usage                                                              |
| :---------- | :----------------------------------------------------------------- |
| Cli         | Receives a `Context` to write output and collect errors.           |
| Utilities   | Uses `PathHelpers.SafePathCombine` to create temporary file paths. |
| Program     | References `Program.Version` for the system information header.    |
|             | Calls `Program.Run` and `Context.Create` to exercise the tool.     |

### Design Decisions

- **Self-contained tests**: Each built-in test creates its own `Context` with a temporary log
  file, runs `Program.Run`, and inspects the output. No external test framework is required at
  run time.
- **Results serialization**: TRX format is used when the results file has a `.trx` extension;
  JUnit XML is used for `.xml`. Unsupported extensions are reported as errors.
- **Temporary directory cleanup**: Each test uses a disposable `TemporaryDirectory` helper that
  deletes its directory on disposal, preventing accumulation of temporary files.
