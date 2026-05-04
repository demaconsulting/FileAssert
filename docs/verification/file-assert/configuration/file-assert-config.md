# FileAssertConfig Verification

This document describes the unit-level verification design for the `FileAssertConfig` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Configuration/FileAssertConfig.cs`.

## Verification Approach

`FileAssertConfig` is verified with unit tests defined in `FileAssertConfigTests.cs`. Tests supply
YAML configuration files in temporary directories and assert on the resulting object state, exit
codes, and results files.

## Dependencies

| Dependency     | Usage in Tests                                               |
|----------------|--------------------------------------------------------------|
| `Context`      | Used directly (not mocked) — created with controlled flags.  |
| `PathHelpers`  | Used internally by `FileAssertConfig`; not mocked.           |

## Test Scenarios

### FileAssertConfig_ReadFromFile_ValidFile_ReturnsConfig

**Scenario**: `FileAssertConfig.ReadFromFile` is called with a valid YAML file path.

**Expected**: A non-null `FileAssertConfig` instance is returned with the correct properties.

**Requirement coverage**: Configuration file reading requirement.

### FileAssertConfig_ReadFromFile_FileNotFound_ThrowsFileNotFoundException

**Scenario**: `FileAssertConfig.ReadFromFile` is called with a path that does not exist.

**Expected**: A `FileNotFoundException` is thrown.

**Boundary / error path**: Missing configuration file error path.

### FileAssertConfig_ReadFromFile_NullPath_ThrowsArgumentNullException

**Scenario**: `FileAssertConfig.ReadFromFile` is called with `null` as the path.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null guard on the path parameter.

### FileAssertConfig_Run_WithNoFilter_RunsAllTests

**Scenario**: `FileAssertConfig.Run` is called with an empty filter list.

**Expected**: All tests in the configuration are executed; exit code reflects pass or fail.

**Requirement coverage**: Run-all-tests requirement.

### FileAssertConfig_Run_WithMatchingFilter_RunsMatchingTest

**Scenario**: `FileAssertConfig.Run` is called with a filter that matches one test name.

**Expected**: Only the matching test runs; exit code reflects the result of that test.

**Requirement coverage**: Test name filtering requirement.

### FileAssertConfig_Run_WithNonMatchingFilter_SkipsTests

**Scenario**: `FileAssertConfig.Run` is called with a filter that matches no tests.

**Expected**: No tests run; exit code is 0.

**Requirement coverage**: Non-matching filter skips all tests requirement.

### FileAssertConfig_Run_WithResultsFile_WritesTrxWithPassedOutcome

**Scenario**: `FileAssertConfig.Run` is called with a context whose `ResultsFile` points to a
temporary `.trx` path, and all assertions pass.

**Expected**: A TRX file is created; it contains a passing result entry.

**Requirement coverage**: TRX results output requirement.

### FileAssertConfig_Run_WithResultsFile_WritesJUnitWithFailedOutcome

**Scenario**: `FileAssertConfig.Run` is called with a context whose `ResultsFile` points to a
temporary `.xml` path, and at least one assertion fails.

**Expected**: A JUnit XML file is created; it contains a failing result entry.

**Requirement coverage**: JUnit results output requirement.

### FileAssertConfig_ReadFromFile_PdfAssertConfig_ParsesCorrectly

**Scenario**: `FileAssertConfig.ReadFromFile` is called with a YAML file that includes PDF
assertion configuration (pages, metadata, text rules).

**Expected**: The PDF assertion config is correctly deserialized with all fields populated.

**Requirement coverage**: PDF assertion configuration parsing requirement.

## Requirements Coverage

- **Configuration file reading**: FileAssertConfig_ReadFromFile_ValidFile_ReturnsConfig
- **Missing file error path**: FileAssertConfig_ReadFromFile_FileNotFound_ThrowsFileNotFoundException
- **Null path guard**: FileAssertConfig_ReadFromFile_NullPath_ThrowsArgumentNullException
- **Run all tests**: FileAssertConfig_Run_WithNoFilter_RunsAllTests
- **Name filter**: FileAssertConfig_Run_WithMatchingFilter_RunsMatchingTest
- **Non-matching filter**: FileAssertConfig_Run_WithNonMatchingFilter_SkipsTests
- **TRX results output**: FileAssertConfig_Run_WithResultsFile_WritesTrxWithPassedOutcome
- **JUnit results output**: FileAssertConfig_Run_WithResultsFile_WritesJUnitWithFailedOutcome
- **PDF config parsing**: FileAssertConfig_ReadFromFile_PdfAssertConfig_ParsesCorrectly
