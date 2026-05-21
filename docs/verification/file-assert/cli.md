## Cli Subsystem Verification

This document describes the subsystem-level verification design for the `Cli` subsystem. It
defines the integration test approach, subsystem boundary, mocking strategy, and test scenarios
that together verify the `Cli` subsystem requirements.

### Verification Strategy

The `Cli` subsystem boundary at `Program` is verified by integration tests defined in
`CliTests.cs`. Each test exercises `Context.Create` and `Program.Run` together, treating the pair
as the observable subsystem interface. Tests pass controlled argument arrays and assert on captured
console output, file system side-effects, and exit codes.

### Dependencies and Mocking Strategy

At the subsystem boundary, `Validation` (part of the `SelfTest` subsystem) is the only external
collaborator that `Program` calls. In scenarios that exercise the `--validate` path, `Validation`
executes its real logic rather than being stubbed. Scenarios that do not involve `--validate` do
not reach `Validation` at all.

No mocking is applied at this level; all collaborators within and directly adjacent to the
subsystem use their real implementations.

### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner against
the .NET runtime specified by the build matrix. No special hardware, peripherals, or
environment configuration is required beyond the standard build toolchain.

### Acceptance Criteria

The Cli subsystem verification passes when all test scenarios listed in
this document execute and pass in the CI pipeline without any test failures, unexpected
exceptions, or assertion errors. Each named scenario must pass on all supported runtime
and platform combinations.

### Integration Test Scenarios

The following integration test scenarios are defined in `CliTests.cs`.

#### Cli_CreateContext_ParsesSilentValidateAndLogFlags

**Scenario**: Arguments containing `--silent`, `--validate`, and `--log <path>` flags are passed
through `Context.Create`.

**Expected**: All three flags are correctly parsed; exit code is 0.

#### Cli_CreateContext_ParsesVersionHelpConfigResultsFlags

**Scenario**: Arguments containing `--version`, `--help`, `--config <path>`, and
`--results <path>` flags are passed through `Context.Create`.

**Expected**: All four flags are correctly parsed; exit code is 0.

#### Cli_CreateContext_WithFilters_ParsesPositionalArguments

**Scenario**: Positional arguments (test filters) are passed through `Context.Create`.

**Expected**: The filters list contains the expected values; exit code is 0.

#### Cli_CreateContext_UnknownArgument_ThrowsArgumentException

**Scenario**: An unrecognized argument is passed through `Context.Create`.

**Expected**: An `ArgumentException` is thrown.

#### Cli_WriteError_AfterSuccessfulCreate_ChangesExitCodeToOne

**Scenario**: `Context.WriteError` is called with an error message.

**Expected**: `ExitCode` becomes 1 after the call.

#### Cli_OutputPipeline_WithLogPathAndSilentFlag_WritesMessagesToLogFile

**Scenario**: A context with both `--silent` and `--log <path>` flags is created;
`Context.WriteLine` is called with a message.

**Expected**: The message appears in the log file; exit code is 0.

#### Cli_CreateContext_ParsesDepthFlag

**Scenario**: Arguments containing `--depth 3` are passed through `Context.Create`.

**Expected**: The `Depth` property is set to `3`; exit code is 0.

#### Cli_OutputPipeline_WithoutSilentFlag_WritesMessagesToConsole

**Scenario**: A context without the `--silent` flag is created; `Context.WriteLine` is
called with a message.

**Expected**: The message appears on standard output; exit code is 0.

### Requirements Coverage

- **Argument parsing**: Cli_CreateContext_ParsesSilentValidateAndLogFlags,
  Cli_CreateContext_ParsesVersionHelpConfigResultsFlags,
  Cli_CreateContext_WithFilters_ParsesPositionalArguments
- **Unknown argument rejection**: Cli_CreateContext_UnknownArgument_ThrowsArgumentException
- **Typed property exposure (depth)**: Cli_CreateContext_ParsesDepthFlag
- **Error exit code**: Cli_WriteError_AfterSuccessfulCreate_ChangesExitCodeToOne
- **Log file output**: Cli_OutputPipeline_WithLogPathAndSilentFlag_WritesMessagesToLogFile
- **Console output**: Cli_OutputPipeline_WithoutSilentFlag_WritesMessagesToConsole
