# Validation Verification

This document describes the unit-level verification design for the `Validation` unit. It defines
the test scenarios, dependency usage, and requirement coverage for `SelfTest/Validation.cs`.

## Verification Approach

`Validation` is verified with unit tests defined in `ValidationTests.cs`. Tests supply a real
`Context` object (not mocked) with a controlled argument set and assert on exit codes, output
content, and result files. Temporary directories are used for result file paths to keep tests
isolated.

## Dependencies

| Dependency    | Usage in Tests                                                            |
|---------------|---------------------------------------------------------------------------|
| `Context`     | Used directly (not mocked) — created with controlled flags for each test. |
| `PathHelpers` | Used internally by `Validation` for temp-path construction; not mocked.   |

No test doubles are introduced at the `Validation` unit level.

## Test Scenarios

### Validation_Run_NullContext_ThrowsArgumentNullException

**Scenario**: `Validation.Run` is called with a `null` context argument.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null guard at the unit boundary.

**Coverage type**: Defensive/boundary test — no formal requirement.

### Validation_Run_WithSilentContext_PrintsSummary

**Scenario**: `Validation.Run` is called with a silent context (output captured separately).

**Expected**: Summary output contains "Total Tests:", "Passed:", and "Failed:".

**Requirement coverage**: Summary output requirement.

### Validation_Run_WithSilentContext_ExitCodeIsZero

**Scenario**: `Validation.Run` is called with a silent context.

**Expected**: `context.ExitCode` is 0 after the run, confirming all sub-tests pass.

**Requirement coverage**: Successful exit code requirement.

### Validation_Run_WithTrxResultsFile_WritesTrxFile

**Scenario**: `Validation.Run` is called with a context whose `ResultsFile` points to a temporary
`.trx` path.

**Expected**: The file is created at the specified path; it contains a `<TestRun` XML element.

**Requirement coverage**: TRX results output requirement.

### Validation_Run_WithXmlResultsFile_WritesXmlFile

**Scenario**: `Validation.Run` is called with a context whose `ResultsFile` points to a temporary
`.xml` path.

**Expected**: The file is created at the specified path; it contains a `<testsuites` XML element.

**Requirement coverage**: JUnit results output requirement.

### Validation_Run_WithUnsupportedResultsFormat_DoesNotWriteFile

**Scenario**: `Validation.Run` is called with a context whose `ResultsFile` has a `.json`
extension (an unsupported format).

**Expected**: No file is created at the specified path; no exception is thrown; an error message
is written to `context` indicating the unsupported format.

**Boundary / error path**: Tests the unsupported-format error path.

**Coverage type**: Defensive/boundary test — no formal requirement.

### Validation_Run_WithSilentContext_LogContainsFileAssertResults

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains FileAssert results output.

**Requirement coverage**: Logging requirement.

### Validation_Run_WithSilentContext_LogContainsFileAssertExists

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains output from the FileAssert "exists" self-validation test.

**Requirement coverage**: Self-validation content requirement.

### Validation_Run_WithSilentContext_LogContainsFileAssertContains

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains output from the FileAssert "contains" self-validation test.

**Requirement coverage**: Self-validation content requirement.

### Validation_Run_WithDepth_UsesSpecifiedHeadingDepth

**Scenario**: `Validation.Run` is called with a context created with `["--depth", "2"]`.

**Expected**: The output uses headings at the specified depth level.

**Requirement coverage**: Heading depth requirement.

## Requirements Coverage

| Requirement                          | Test Scenario                                                   |
|--------------------------------------|-----------------------------------------------------------------|
| Defensive boundary (no req.)         | Validation_Run_NullContext_ThrowsArgumentNullException          |
| Summary output                       | Validation_Run_WithSilentContext_PrintsSummary                  |
| Successful exit code                 | Validation_Run_WithSilentContext_ExitCodeIsZero                 |
| TRX results output                   | Validation_Run_WithTrxResultsFile_WritesTrxFile                 |
| JUnit results output                 | Validation_Run_WithXmlResultsFile_WritesXmlFile                 |
| Defensive boundary (no req.)         | Validation_Run_WithUnsupportedResultsFormat_DoesNotWriteFile    |
| Logging                              | Validation_Run_WithSilentContext_LogContainsFileAssertResults   |
| Self-validation content              | Validation_Run_WithSilentContext_LogContainsFileAssertExists,   |
|                                      | Validation_Run_WithSilentContext_LogContainsFileAssertContains  |
| Heading depth                        | Validation_Run_WithDepth_UsesSpecifiedHeadingDepth              |
