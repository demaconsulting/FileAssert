### Context Verification

This document describes the unit-level verification design for the `Context` unit. It defines the
test scenarios, dependency usage, and requirement coverage for `Cli/Context.cs`.

#### Verification Approach

`Context` is verified with unit tests defined in `ContextTests.cs` and
`ContextNewPropertiesTests.cs`. Because `Context` depends only on .NET base class library types
(`Console`, `StreamWriter`, `Path`), no mocking or test doubles are required. Tests call
`Context.Create` with controlled argument arrays, inspect the resulting properties and exit codes,
and verify output written to captured streams.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Every public method on
`Context` exercised by the listed scenarios returns the documented value or sets `ExitCode`
to `1` when an error is reported.

#### Dependencies

`Context` has no dependencies on other tool units. All dependencies are real .NET BCL types;
no mocking is needed at this level.

#### Test Scenarios

##### Context_Create_NoArguments_ReturnsDefaultContext

**Scenario**: `Context.Create` is called with an empty argument array.

**Expected**: All boolean flags are false; `ResultsFile` is null; exit code is 0.

##### Context_Create_VersionFlag_SetsVersionTrue

**Scenario**: `Context.Create` is called with `["--version"]`.

**Expected**: `Version` property is true.

##### Context_Create_ShortVersionFlag_SetsVersionTrue

**Scenario**: `Context.Create` is called with `["-v"]`.

**Expected**: `Version` property is true.

##### Context_Create_HelpFlag_SetsHelpTrue

**Scenario**: `Context.Create` is called with `["--help"]`.

**Expected**: `Help` property is true.

##### Context_Create_ShortHelpFlag_H_SetsHelpTrue

**Scenario**: `Context.Create` is called with `["-h"]`.

**Expected**: `Help` property is true.

##### Context_Create_ShortHelpFlag_Question_SetsHelpTrue

**Scenario**: `Context.Create` is called with `["-?"]`.

**Expected**: `Help` property is true.

##### Context_Create_SilentFlag_SetsSilentTrue

**Scenario**: `Context.Create` is called with `["--silent"]`.

**Expected**: `Silent` property is true.

##### Context_Create_ValidateFlag_SetsValidateTrue

**Scenario**: `Context.Create` is called with `["--validate"]`.

**Expected**: `Validate` property is true.

##### Context_Create_ResultsFlag_SetsResultsFile

**Scenario**: `Context.Create` is called with `["--results", "output.trx"]`.

**Expected**: `ResultsFile` property equals `"output.trx"`.

##### Context_Create_ResultAliasFlag_SetsResultsFile

**Scenario**: `Context.Create` is called with `["--result", "output.trx"]` (legacy alias).

**Expected**: `ResultsFile` property equals `"output.trx"`, identical to the `--results` flag.

##### Context_Create_LogFlag_OpensLogFile

**Scenario**: `Context.Create` is called with `["--log", "<tmp>.log"]`; `WriteLine` is then called
with a test message.

**Expected**: The log file is created; the test message is written to it.

##### Context_Create_UnknownArgument_ThrowsArgumentException

**Scenario**: `Context.Create` is called with an unrecognized argument (e.g., `["--unknown"]`).

**Expected**: An `ArgumentException` is thrown containing the text "Unsupported argument".

##### Context_Create_LogFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--log"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

##### Context_Create_ResultsFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--results"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

##### Context_WriteLine_NotSilent_WritesToConsole

**Scenario**: A non-silent `Context` is created and `WriteLine` is called with a test message.

**Expected**: The test message appears on standard output.

##### Context_WriteLine_Silent_DoesNotWriteToConsole

**Scenario**: A silent `Context` (created with `["--silent"]`) calls `WriteLine`.

**Expected**: Standard output receives nothing.

##### Context_WriteError_Silent_DoesNotWriteToConsole

**Scenario**: A silent `Context` calls `WriteError`.

**Expected**: Standard error receives nothing.

##### Context_WriteError_SetsErrorExitCode

**Scenario**: A `Context` calls `WriteError`.

**Expected**: `ExitCode` is 1 after the call.

##### Context_WriteError_NotSilent_WritesToConsole

**Scenario**: A non-silent `Context` calls `WriteError` with a test message.

**Expected**: The test message appears on standard error.

##### Context_WriteError_WritesToLogFile

**Scenario**: A `Context` created with `["--silent", "--log", "<tmp>.log"]` calls `WriteError`
with a test message.

**Expected**: The test message appears in the log file.

##### Context_ErrorCount_IncrementsOnEachWriteError

**Scenario**: `WriteError` is called multiple times on the same `Context`.

**Expected**: `ErrorCount` increments by one for each call.

##### Context_Create_DepthFlag_SetsDepth

**Scenario**: `Context.Create` is called with `["--depth", "3"]`.

**Expected**: `Depth` property equals 3.

##### Context_Create_NoArguments_DepthDefaultsToOne

**Scenario**: `Context.Create` is called with an empty argument array.

**Expected**: `Depth` property equals 1 (the default).

##### Context_Create_DepthFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

##### Context_Create_DepthFlag_NonNumeric_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "abc"]`.

**Expected**: An `ArgumentException` is thrown.

##### Context_Create_DepthFlag_Zero_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "0"]` (below minimum of 1).

**Expected**: An `ArgumentException` is thrown.

##### Context_Create_DepthFlag_AboveSix_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "7"]` (above maximum of 6).

**Expected**: An `ArgumentException` is thrown.

##### Context_Create_NoArguments_ConfigFileHasDefaultValue

**Scenario**: `Context.Create` is called with an empty argument array.

**Expected**: `ConfigFile` property has the default value.

##### Context_Create_NoArguments_FiltersIsEmpty

**Scenario**: `Context.Create` is called with an empty argument array.

**Expected**: `Filters` collection is empty.

##### Context_Create_ConfigFlag_SetsConfigFile

**Scenario**: `Context.Create` is called with `["--config", "my.yaml"]`.

**Expected**: `ConfigFile` property equals `"my.yaml"`.

##### Context_Create_PositionalArguments_AddedToFilters

**Scenario**: `Context.Create` is called with positional arguments (e.g., `["TestA", "TestB"]`).

**Expected**: `Filters` contains `["TestA", "TestB"]`.

##### Context_Create_MixedArguments_ParsesCorrectly

**Scenario**: `Context.Create` is called with a mix of flags and positional arguments.

**Expected**: All flags and positional arguments are correctly parsed.

##### Context_Create_UnknownFlagWithDash_ThrowsArgumentException

**Scenario**: `Context.Create` is called with an unrecognized flag starting with `--`.

**Expected**: An `ArgumentException` is thrown.

##### Context_Create_ConfigFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--config"]` (value missing).

**Expected**: An `ArgumentException` is thrown.
