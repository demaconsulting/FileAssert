# FileAssert System Design

## Overview

FileAssert is a .NET command-line tool for asserting file properties using YAML-defined test
suites. It is packaged as a .NET global tool and invoked as `fileassert`. The tool accepts a
configuration file, evaluates glob patterns against the file system, and reports failures when
files do not meet the declared constraints.

## System-Level Responsibilities

| Responsibility          | Description                                                               |
| :---------------------- | :------------------------------------------------------------------------ |
| Argument parsing        | Accept and validate command-line flags and positional filter arguments.   |
| Configuration loading   | Read and deserialize a YAML test-suite configuration file.                |
| Configuration file path | Accept a custom configuration file path via `--config`.                   |
| Test filtering          | Run only the tests whose name or tags match the positional filter args.   |
| Test execution          | Run selected tests, evaluating file patterns and content rules.           |
| Output and logging      | Report results to stdout/stderr and optionally to a log file.             |
| Self-validation         | Verify core functionality at run time via built-in tests.                 |
| Results serialization   | Write test outcome records to TRX or JUnit XML format.                    |

## Software Item Hierarchy

The system is decomposed into one top-level unit (Program) and five subsystems, each containing
one or more units:

| Item          | Level     | Units contained                                           |
| :------------ | :-------- | :-------------------------------------------------------- |
| FileAssert    | System    | —                                                         |
| Program       | Unit      | —                                                         |
| Cli           | Subsystem | Context                                                   |
| Configuration | Subsystem | FileAssertConfig, FileAssertData                          |
| Modeling      | Subsystem | FileAssertTest, FileAssertFile, FileAssertRule            |
| Utilities     | Subsystem | PathHelpers                                               |
| SelfTest      | Subsystem | Validation                                                |

## Execution Flow

The following sequence describes the normal execution path:

1. `Program.Main` creates a `Context` instance by parsing command-line arguments.
2. `Program.Run` inspects context flags in priority order:
   a. `--version` — prints the version string and exits.
   b. `--help` — prints usage information and exits.
   c. `--validate` — delegates to `Validation.Run` for self-validation and exits.
   d. Default — delegates to `Program.RunToolLogic`.
3. `RunToolLogic` resolves the configuration file from `context.ConfigFile` (default:
   `.fileassert.yaml`; overridden by `--config`). If absent, it prints guidance (default
   path) or an error (explicit path) and exits.
4. `FileAssertConfig.ReadFromFile` deserializes the YAML configuration into a hierarchy of
   `FileAssertTest`, `FileAssertFile`, and `FileAssertRule` instances.
5. `FileAssertConfig.Run` filters the test list against `context.Filters` (the positional
   name-or-tag arguments) and executes each matching test. An empty filter list runs all tests.
6. Each `FileAssertTest.Run` iterates its `FileAssertFile` list.
7. Each `FileAssertFile.Run` discovers files via a glob matcher, validates count constraints,
   and applies content rules.
8. Content rules (`FileAssertContainsRule`, `FileAssertMatchesRule`, `FileAssertDoesNotContainRule`,
   `FileAssertDoesNotMatchRule`) call `context.WriteError` on failure.
9. After all tests complete, `context.ExitCode` reflects whether any errors occurred.
10. When `--validate` is used with `--results`, `Validation.Run` writes TRX or JUnit XML
    results to the file specified by `context.ResultsFile`.

## Design Decisions

- **Single-assembly tool**: All logic is compiled into one assembly and published as a .NET
  global tool, simplifying installation and avoiding DLL management.
- **YAML configuration**: YAML is human-readable, widely supported, and natively handled by
  YamlDotNet. The `IgnoreUnmatchedProperties` setting provides forward compatibility.
- **Internal visibility**: All classes except test-facing members are `internal`, limiting the
  public API surface to what is strictly necessary.
- **Error accumulation**: Failures are accumulated via `Context.WriteError` rather than
  exceptions, so all assertions in a run are reported in a single pass.
