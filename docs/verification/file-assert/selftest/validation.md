### Validation Verification

This document describes the unit-level verification design for the `Validation` unit. It defines
the test scenarios, dependency usage, and requirement coverage for `SelfTest/Validation.cs`.

#### Verification Approach

`Validation` is verified with unit tests defined in `ValidationTests.cs`. Tests supply a real
`Context` object (not mocked) with a controlled argument set and assert on exit codes, output
content, and result files. Temporary directories are used for result file paths to keep tests
isolated.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `Validation.cs`
meets the project minimum threshold.

#### Dependencies

| Dependency                   | Usage in Tests                                                            |
|------------------------------|---------------------------------------------------------------------------|
| `Context`                    | Used directly (not mocked) — created with controlled flags for each test. |
| `PathHelpers`                | Used internally by `Validation` for temp-path construction; not mocked.   |
| `DemaConsulting.TestResults` | Real TRX/JUnit serializers; their output files are inspected by tests.    |

No test doubles are introduced at the `Validation` unit level.

#### Test Scenarios

##### Validation_Run_NullContext_ThrowsArgumentNullException

**Scenario**: `Validation.Run` is called with a `null` context argument.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null guard at the unit boundary.

##### Validation_Run_WithSilentContext_PrintsSummary

**Scenario**: `Validation.Run` is called with a silent context (output captured separately).

**Expected**: Summary output contains "Total Tests:", "Passed:", and "Failed:".

##### Validation_Run_WithSilentContext_ExitCodeIsZero

**Scenario**: `Validation.Run` is called with a silent context.

**Expected**: `context.ExitCode` is 0 after the run, confirming all sub-tests pass.

##### Validation_Run_WithTrxResultsFile_WritesTrxFile

**Scenario**: `Validation.Run` is called with a context whose `ResultsFile` points to a temporary
`.trx` path.

**Expected**: The file is created at the specified path; it contains a `<TestRun` XML element;
exit code is 0 (the validation suite itself passes).

##### Validation_Run_WithXmlResultsFile_WritesXmlFile

**Scenario**: `Validation.Run` is called with a context whose `ResultsFile` points to a temporary
`.xml` path.

**Expected**: The file is created at the specified path; it contains a `<testsuites` XML element.

##### Validation_Run_WithUnsupportedResultsFormat_DoesNotWriteFile

**Scenario**: `Validation.Run` is called with a context whose `ResultsFile` has a `.json`
extension (an unsupported format).

**Expected**: No file is created at the specified path; no exception is thrown; an error message
is written to `context` indicating the unsupported format.

**Boundary / error path**: Tests the unsupported-format error path.

(boundary condition: unsupported extension is rejected without creating a file).

##### Validation_Run_WithSilentContext_LogContainsFileAssertResults

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains FileAssert results output.

##### Validation_Run_WithSilentContext_LogContainsFileAssertFile

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains output from the `FileAssert_File` self-validation test.

##### Validation_Run_WithSilentContext_LogContainsFileAssertText

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains output from the `FileAssert_Text` self-validation test.

##### Validation_Run_WithSilentContext_LogContainsFileAssertHtml

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains output from the `FileAssert_Html` self-validation test.

##### Validation_Run_WithSilentContext_LogContainsFileAssertXml

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains output from the `FileAssert_Xml` self-validation test.

##### Validation_Run_WithSilentContext_LogContainsFileAssertYaml

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains output from the `FileAssert_Yaml` self-validation test.

##### Validation_Run_WithSilentContext_LogContainsFileAssertJson

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains output from the `FileAssert_Json` self-validation test.

##### Validation_Run_WithSilentContext_LogContainsFileAssertPdf

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains output from the `FileAssert_Pdf` self-validation test.

##### Validation_Run_WithSilentContext_LogContainsFileAssertZip

**Scenario**: `Validation.Run` is called with a context that has logging enabled.

**Expected**: The log contains output from the `FileAssert_Zip` self-validation test.

##### Validation_Run_WithDepth_UsesSpecifiedHeadingDepth

**Scenario**: `Validation.Run` is called with a context created with `["--depth", "3"]`.

**Expected**: The output uses headings at the specified depth level.

##### Validation_Run_HeaderFields

**Scenario**: `Validation.Run` is called with a real context.

**Expected**: The captured output contains the system information header. Only the "FileAssert"
banner and copyright lines are asserted directly; the additional header fields produced by the
implementation (such as `DotNet Runtime` and `Time Stamp`) are observable in the log but are not
individually asserted because their values are environment-dependent. This is documented here so
that reviewers know the verification covers a representative subset rather than an exhaustive
match of every header field.
