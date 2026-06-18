### FileAssertConfig Verification

This document describes the unit-level verification design for the `FileAssertConfig` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Configuration/FileAssertConfig.cs`.

#### Verification Approach

`FileAssertConfig` is verified with unit tests defined in `FileAssertConfigTests.cs`. Tests supply
YAML configuration files in temporary directories and assert on the resulting object state, exit
codes, and results files.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `FileAssertConfig.cs`
meets the project minimum threshold.

#### Dependencies

| Dependency     | Usage in Tests                                               |
|----------------|--------------------------------------------------------------|
| `Context`      | Used directly (not mocked) — created with controlled flags.  |

#### Test Scenarios

##### FileAssertConfig_ReadFromFile_ValidFile_ReturnsConfig

**Scenario**: `FileAssertConfig.ReadFromFile` is called with a valid YAML file path.

**Expected**: A non-null `FileAssertConfig` instance is returned with the correct properties.

##### FileAssertConfig_ReadFromFile_FileNotFound_ThrowsFileNotFoundException

**Scenario**: `FileAssertConfig.ReadFromFile` is called with a path that does not exist.

**Expected**: A `FileNotFoundException` is thrown.

**Boundary / error path**: Missing configuration file error path.

##### FileAssertConfig_ReadFromFile_NullPath_ThrowsArgumentNullException

**Scenario**: `FileAssertConfig.ReadFromFile` is called with `null` as the path.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null guard on the path parameter.

##### FileAssertConfig_Run_WithNoFilter_RunsAllTests

**Scenario**: `FileAssertConfig.Run` is called with an empty filter list.

**Expected**: All tests in the configuration are executed; exit code reflects pass or fail.

##### FileAssertConfig_Run_WithMatchingFilter_RunsMatchingTest

**Scenario**: `FileAssertConfig.Run` is called with a filter that matches one test name.

**Expected**: Only the matching test runs; exit code reflects the result of that test.

##### FileAssertConfig_Run_WithNonMatchingFilter_SkipsTests

**Scenario**: `FileAssertConfig.Run` is called with a filter that matches no tests.

**Expected**: No tests run; exit code is 0.

##### FileAssertConfig_Run_WithResultsFile_WritesTrxWithPassedOutcome

**Scenario**: `FileAssertConfig.Run` is called with a context whose `ResultsFile` points to a
temporary `.trx` path, and all assertions pass.

**Expected**: A TRX file is created; it contains a passing result entry.

##### FileAssertConfig_Run_WithResultsFile_WritesJUnitWithFailedOutcome

**Scenario**: `FileAssertConfig.Run` is called with a context whose `ResultsFile` points to a
temporary `.xml` path, and at least one assertion fails.

**Expected**: A JUnit XML file is created; it contains a failing result entry.

##### FileAssertConfig_ReadFromFile_PdfAssertConfig_ParsesCorrectly

**Scenario**: `FileAssertConfig.ReadFromFile` is called with a YAML file that includes PDF
assertion configuration (pages, metadata, text rules).

**Expected**: The PDF assertion config is correctly deserialized with all fields populated.
