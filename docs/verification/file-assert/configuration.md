## Configuration Subsystem Verification

This document describes the subsystem-level verification design for the `Configuration` subsystem.
It defines the integration test approach, subsystem boundary, mocking strategy, and test scenarios
that together verify the `Configuration` subsystem requirements.

### Verification Approach

The `Configuration` subsystem is verified by integration tests defined in `ConfigurationTests.cs`.
Each test exercises the full loading and execution pipeline — reading a YAML file, constructing
the test hierarchy, and running the resulting configuration — with a real `Context`.

### Dependencies and Mocking Strategy

All collaborators at the subsystem boundary (`Context`, `FileAssertConfig`, `PathHelpers`) use
their real implementations. Temporary directories are used for configuration files and test
artifacts so that tests remain isolated and leave no permanent file-system side-effects.

### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner against
the .NET runtime specified by the build matrix. No special hardware, peripherals, or
environment configuration is required beyond the standard build toolchain.

### Acceptance Criteria

The Configuration subsystem verification passes when all test scenarios listed in
this document execute and pass in the CI pipeline without any test failures, unexpected
exceptions, or assertion errors. Each named scenario must pass on all supported runtime
and platform combinations.

### Test Scenarios

The following integration test scenarios are defined in `ConfigurationTests.cs`.

#### Configuration_LoadYaml_BuildsCompleteTestHierarchy

**Scenario**: A YAML configuration file with nested test, file, and rule entries is loaded using
`FileAssertConfig.ReadFromFile`.

**Expected**: The complete object hierarchy (tests → files → rules) is correctly constructed with
all properties populated.

#### Configuration_RunWithFilter_ExecutesOnlyMatchingTests

**Scenario**: A configuration with two tests is loaded. Only one file exists; a filter naming one
test is passed to `FileAssertConfig.Run`.

**Expected**: Only the named test runs; exit code is 0.

#### Configuration_RunWithTagFilter_ExecutesOnlyMatchingTests

**Scenario**: A configuration with two tests with different tags is loaded. Only one file exists;
a filter naming one tag is passed to `FileAssertConfig.Run`.

**Expected**: Only the test matching the tag runs; exit code is 0.

#### Configuration_Run_WithResultsFile_WritesTrxResultsFile

**Scenario**: A configuration file with one test is loaded. A results file path with a `.trx`
extension is provided to the context via `--results`.

**Expected**: `FileAssertConfig.Run` completes and a TRX results file is written to the specified
path.

#### Configuration_Run_WithResultsFile_WritesJUnitResultsFile

**Scenario**: A configuration file with one test is loaded. A results file path with an `.xml`
extension is provided to the context via `--results`.

**Expected**: `FileAssertConfig.Run` completes and a JUnit XML results file (containing a
`<testsuites` root element) is written to the specified path. This confirms the subsystem selects
the JUnit format from the file extension, complementing the TRX scenario above.

#### Configuration_LoadYaml_InvalidYaml_ThrowsOrReportsParseError

**Scenario** (negative): A YAML file containing syntactically invalid YAML (for example, an
unbalanced bracket or a stray tab character that breaks the parser) is supplied to
`FileAssertConfig.ReadFromFile`.

**Expected**: `FileAssertConfig.ReadFromFile` does not return a partially-constructed configuration.
A YAML deserialization exception (`YamlDotNet.Core.YamlException`, surfaced through the loader)
propagates to the caller; no test hierarchy is constructed and no assertions are executed. The
caller is responsible for translating the exception into the appropriate non-zero exit code.
