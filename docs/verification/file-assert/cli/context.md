### Context Verification

This document describes the unit-level verification design for the `Context` unit. It defines the
test scenarios, dependency usage, and requirement coverage for `Cli/Context.cs`.

#### Verification Approach

`Context` is verified with unit tests defined in `ContextTests.cs` and
`ContextNewPropertiesTests.cs`. Because `Context` depends only on .NET base class library types
(`Console`, `StreamWriter`, `Path`), no mocking or test doubles are required. Tests call
`Context.Create` with controlled argument arrays, inspect the resulting properties and exit codes,
and verify output written to captured streams.

#### Dependencies

`Context` has no dependencies on other tool units. All dependencies are real .NET BCL types;
no mocking is needed at this level.

#### Test Scenarios

##### Context_Create_NoArguments_ReturnsDefaultContext

**Scenario**: `Context.Create` is called with an empty argument array.

**Expected**: All boolean flags are false; `ResultsFile` is null; exit code is 0.

**Requirement coverage**: Default context creation requirement.

##### Context_Create_VersionFlag_SetsVersionTrue

**Scenario**: `Context.Create` is called with `["--version"]`.

**Expected**: `Version` property is true.

**Requirement coverage**: Version flag parsing requirement.

##### Context_Create_ShortVersionFlag_SetsVersionTrue

**Scenario**: `Context.Create` is called with `["-v"]`.

**Expected**: `Version` property is true.

**Requirement coverage**: Short version flag parsing requirement.

##### Context_Create_HelpFlag_SetsHelpTrue

**Scenario**: `Context.Create` is called with `["--help"]`.

**Expected**: `Help` property is true.

**Requirement coverage**: Help flag (long form) parsing requirement.

##### Context_Create_ShortHelpFlag_H_SetsHelpTrue

**Scenario**: `Context.Create` is called with `["-h"]`.

**Expected**: `Help` property is true.

**Requirement coverage**: Help flag (-h) parsing requirement.

##### Context_Create_ShortHelpFlag_Question_SetsHelpTrue

**Scenario**: `Context.Create` is called with `["-?"]`.

**Expected**: `Help` property is true.

**Requirement coverage**: Help flag (-?) parsing requirement.

##### Context_Create_SilentFlag_SetsSilentTrue

**Scenario**: `Context.Create` is called with `["--silent"]`.

**Expected**: `Silent` property is true.

**Requirement coverage**: Silent flag parsing requirement.

##### Context_Create_ValidateFlag_SetsValidateTrue

**Scenario**: `Context.Create` is called with `["--validate"]`.

**Expected**: `Validate` property is true.

**Requirement coverage**: Validate flag parsing requirement.

##### Context_Create_ResultsFlag_SetsResultsFile

**Scenario**: `Context.Create` is called with `["--results", "output.trx"]`.

**Expected**: `ResultsFile` property equals `"output.trx"`.

**Requirement coverage**: Results file path parsing requirement.

##### Context_Create_ResultAliasFlag_SetsResultsFile

**Scenario**: `Context.Create` is called with `["--result", "output.trx"]` (legacy alias).

**Expected**: `ResultsFile` property equals `"output.trx"`, identical to the `--results` flag.

**Requirement coverage**: Results alias flag parsing requirement.

##### Context_Create_LogFlag_OpensLogFile

**Scenario**: `Context.Create` is called with `["--log", "<tmp>.log"]`; `WriteLine` is then called
with a test message.

**Expected**: The log file is created; the test message is written to it.

**Requirement coverage**: Log file opening and writing requirement.

##### Context_Create_UnknownArgument_ThrowsArgumentException

**Scenario**: `Context.Create` is called with an unrecognized argument (e.g., `["--unknown"]`).

**Expected**: An `ArgumentException` is thrown containing the text "Unsupported argument".

**Requirement coverage**: Unknown argument rejection requirement.

##### Context_Create_LogFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--log"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: Log flag missing-value validation requirement.

##### Context_Create_ResultsFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--results"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: Results flag missing-value validation requirement.

##### Context_WriteLine_NotSilent_WritesToConsole

**Scenario**: A non-silent `Context` is created and `WriteLine` is called with a test message.

**Expected**: The test message appears on standard output.

**Requirement coverage**: Normal output writing requirement.

##### Context_WriteLine_Silent_DoesNotWriteToConsole

**Scenario**: A silent `Context` (created with `["--silent"]`) calls `WriteLine`.

**Expected**: Standard output receives nothing.

**Requirement coverage**: Silent mode suppression requirement.

##### Context_WriteError_Silent_DoesNotWriteToConsole

**Scenario**: A silent `Context` calls `WriteError`.

**Expected**: Standard error receives nothing.

**Requirement coverage**: Silent mode error suppression requirement.

##### Context_WriteError_SetsErrorExitCode

**Scenario**: A `Context` calls `WriteError`.

**Expected**: `ExitCode` is 1 after the call.

**Requirement coverage**: Error exit code setting requirement.

##### Context_WriteError_NotSilent_WritesToConsole

**Scenario**: A non-silent `Context` calls `WriteError` with a test message.

**Expected**: The test message appears on standard error.

**Requirement coverage**: Error output writing requirement.

##### Context_WriteError_WritesToLogFile

**Scenario**: A `Context` created with `["--silent", "--log", "<tmp>.log"]` calls `WriteError`
with a test message.

**Expected**: The test message appears in the log file.

**Requirement coverage**: Error log writing requirement.

##### Context_ErrorCount_IncrementsOnEachWriteError

**Scenario**: `WriteError` is called multiple times on the same `Context`.

**Expected**: `ErrorCount` increments by one for each call.

**Requirement coverage**: Error count tracking requirement.

##### Context_Create_DepthFlag_SetsDepth

**Scenario**: `Context.Create` is called with `["--depth", "3"]`.

**Expected**: `HeadingDepth` property equals 3.

**Requirement coverage**: Depth flag parsing requirement.

##### Context_Create_NoArguments_DepthDefaultsToOne

**Scenario**: `Context.Create` is called with an empty argument array.

**Expected**: `HeadingDepth` property equals 1 (the default).

**Requirement coverage**: Default heading depth requirement.

##### Context_Create_DepthFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: Depth flag missing-value validation requirement.

##### Context_Create_DepthFlag_NonNumeric_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "abc"]`.

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: Depth flag non-integer validation requirement.

##### Context_Create_DepthFlag_Zero_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "0"]` (below minimum of 1).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: Depth flag minimum-value validation requirement.

##### Context_Create_DepthFlag_AboveSix_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "7"]` (above maximum of 6).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: Depth flag maximum-value validation requirement.

##### Context_Create_NoArguments_ConfigFileHasDefaultValue

**Scenario**: `Context.Create` is called with an empty argument array.

**Expected**: `ConfigFile` property has the default value.

**Requirement coverage**: Default config file path requirement.

##### Context_Create_NoArguments_FiltersIsEmpty

**Scenario**: `Context.Create` is called with an empty argument array.

**Expected**: `Filters` collection is empty.

**Requirement coverage**: Default filters requirement.

##### Context_Create_ConfigFlag_SetsConfigFile

**Scenario**: `Context.Create` is called with `["--config", "my.yaml"]`.

**Expected**: `ConfigFile` property equals `"my.yaml"`.

**Requirement coverage**: Config file flag parsing requirement.

##### Context_Create_PositionalArguments_AddedToFilters

**Scenario**: `Context.Create` is called with positional arguments (e.g., `["TestA", "TestB"]`).

**Expected**: `Filters` contains `["TestA", "TestB"]`.

**Requirement coverage**: Test filter parsing requirement.

##### Context_Create_MixedArguments_ParsesCorrectly

**Scenario**: `Context.Create` is called with a mix of flags and positional arguments.

**Expected**: All flags and positional arguments are correctly parsed.

**Requirement coverage**: Mixed argument parsing requirement.

##### Context_Create_UnknownFlagWithDash_ThrowsArgumentException

**Scenario**: `Context.Create` is called with an unrecognized flag starting with `--`.

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: Unknown flag rejection requirement.

##### Context_Create_ConfigFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--config"]` (value missing).

**Expected**: An `ArgumentException` is thrown.

**Requirement coverage**: Config flag missing-value validation requirement.

#### Requirements Coverage

| Requirement                    | Test Scenario                                                   |
|--------------------------------|-----------------------------------------------------------------|
| Default context creation       | Context_Create_NoArguments_ReturnsDefaultContext                |
| --version flag parsing         | Context_Create_VersionFlag_SetsVersionTrue                      |
| -v flag parsing                | Context_Create_ShortVersionFlag_SetsVersionTrue                 |
| --help flag parsing            | Context_Create_HelpFlag_SetsHelpTrue                            |
| -h flag parsing                | Context_Create_ShortHelpFlag_H_SetsHelpTrue                     |
| -? flag parsing                | Context_Create_ShortHelpFlag_Question_SetsHelpTrue              |
| --silent flag parsing          | Context_Create_SilentFlag_SetsSilentTrue                        |
| --validate flag parsing        | Context_Create_ValidateFlag_SetsValidateTrue                    |
| --results flag parsing         | Context_Create_ResultsFlag_SetsResultsFile                      |
| --result alias parsing         | Context_Create_ResultAliasFlag_SetsResultsFile                  |
| --log flag and file writing    | Context_Create_LogFlag_OpensLogFile                             |
| Unknown argument rejection     | Context_Create_UnknownArgument_ThrowsArgumentException          |
| --log missing value            | Context_Create_LogFlag_WithoutValue_ThrowsArgumentException     |
| --results missing value        | Context_Create_ResultsFlag_WithoutValue_ThrowsArgumentException |
| Normal output writing          | Context_WriteLine_NotSilent_WritesToConsole                     |
| Silent mode output suppression | Context_WriteLine_Silent_DoesNotWriteToConsole                  |
| Silent mode error suppression  | Context_WriteError_Silent_DoesNotWriteToConsole                 |
| Error exit code                | Context_WriteError_SetsErrorExitCode                            |
| Error output to stderr         | Context_WriteError_NotSilent_WritesToConsole                    |
| Error writing to log file      | Context_WriteError_WritesToLogFile                              |
| Error count tracking           | Context_ErrorCount_IncrementsOnEachWriteError                   |
| --depth flag parsing           | Context_Create_DepthFlag_SetsDepth                              |
| Default heading depth          | Context_Create_NoArguments_DepthDefaultsToOne                   |
| --depth missing value          | Context_Create_DepthFlag_WithoutValue_ThrowsArgumentException   |
| --depth non-integer value      | Context_Create_DepthFlag_NonNumeric_ThrowsArgumentException     |
| --depth zero value (min 1)     | Context_Create_DepthFlag_Zero_ThrowsArgumentException           |
| --depth exceeds maximum (max 6)| Context_Create_DepthFlag_AboveSix_ThrowsArgumentException       |
| --config flag parsing          | Context_Create_ConfigFlag_SetsConfigFile                        |
| Default config file path       | Context_Create_NoArguments_ConfigFileHasDefaultValue            |
| Test filter parsing            | Context_Create_PositionalArguments_AddedToFilters               |
| Mixed argument parsing         | Context_Create_MixedArguments_ParsesCorrectly                   |
| Unknown flag rejection         | Context_Create_UnknownFlagWithDash_ThrowsArgumentException      |
| --config missing value         | Context_Create_ConfigFlag_WithoutValue_ThrowsArgumentException  |
