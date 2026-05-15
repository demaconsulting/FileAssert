# System Verification

This document describes the system-level verification design for FileAssert. It defines the overall
verification strategy, test environments, interface simulation approach, and end-to-end integration
test scenarios that together demonstrate the system meets its requirements.

## Verification Strategy

System-level verification uses end-to-end integration tests that invoke the tool as a real process
via the `Runner.Run` helper in `IntegrationTests.cs`. Each test exercises the full stack — argument
parsing, dispatch, execution, and output — and validates both exit code and console output.

This approach ensures that system requirements are verified at the system boundary without assuming
any internal implementation detail. The tests treat the tool as a black box and assert on
observable outputs only.

**Note**: `Runner.Run` merges stdout and stderr into a single combined output string. Per-stream
assertions (e.g., "standard error is empty") are therefore not possible at the integration test
level; all assertions are made against the combined output.

## Test Environments

Integration tests are executed across the following environments to satisfy multi-runtime and
multi-platform requirements:

| Runtime    | Platform |
|------------|----------|
| .NET 8.0   | Windows  |
| .NET 8.0   | Linux    |
| .NET 8.0   | macOS    |
| .NET 9.0   | Windows  |
| .NET 9.0   | Linux    |
| .NET 9.0   | macOS    |
| .NET 10.0  | Windows  |
| .NET 10.0  | Linux    |
| .NET 10.0  | macOS    |

All integration test scenarios are expected to produce identical results on all supported runtime
and platform combinations.

## External Interface Simulation

At the system level, no interfaces are mocked. All external interfaces are exercised with real
implementations:

- **Standard output / standard error** — Captured by `Runner.Run` and returned as a combined
  string for assertion.
- **File system** — Temporary files and directories are created and cleaned up within each test.
- **Process exit code** — Returned by `Runner.Run` and asserted directly.

## Integration Test Scenarios

The following integration test scenarios are defined in `IntegrationTests.cs`.

### IntegrationTest_VersionFlag_OutputsVersion

**Scenario**: The `--version` flag is passed as the sole argument.

**Expected**: Exit code 0; combined output contains a semantic version string.

### IntegrationTest_HelpFlag_OutputsUsageInformation

**Scenario**: The `--help` flag is passed as the sole argument.

**Expected**: Exit code 0; combined output contains the text "Usage".

### IntegrationTest_ValidateFlag_RunsValidation

**Scenario**: The `--validate` flag is passed as the sole argument.

**Expected**: Exit code 0; combined output contains "Total Tests:".

### IntegrationTest_ValidateWithResults_GeneratesTrxFile

**Scenario**: The `--validate` flag is combined with `--results <path>.trx`.

**Expected**: Exit code 0; a TRX file is created at the specified path containing a `<TestRun`
XML element.

### IntegrationTest_ValidateWithResults_GeneratesJUnitFile

**Scenario**: The `--validate` flag is combined with `--results <path>.xml`.

**Expected**: Exit code 0; a JUnit XML file is created at the specified path containing a
`<testsuites` XML element.

### IntegrationTest_SilentFlag_SuppressesOutput

**Scenario**: The `--silent` flag is passed.

**Expected**: Exit code 0; combined output is empty.

### IntegrationTest_LogFlag_WritesOutputToFile

**Scenario**: The `--log <path>` flag is passed.

**Expected**: Exit code 0; the log file is created and contains output.

### IntegrationTest_UnknownArgument_ReturnsError

**Scenario**: An unrecognized argument is passed.

**Expected**: Exit code non-zero; combined output contains an error message.

### IntegrationTest_TestFiltering_OnlyRunsMatchingTests

**Scenario**: A configuration file with two tests is supplied; only one test name is passed as
a positional filter argument.

**Expected**: Exit code 0; only the named test is executed.

### IntegrationTest_ValidConfig_PassingAssertions_ReturnsZero

**Scenario**: A valid configuration file is supplied where all file assertions pass.

**Expected**: Exit code 0.

### IntegrationTest_ValidConfig_FailingAssertions_ReturnsNonZero

**Scenario**: A configuration file is supplied where at least one assertion fails.

**Expected**: Exit code non-zero.

### IntegrationTest_PassingAssertions_WritesTrxWithPassedResults

**Scenario**: All assertions pass and `--results <path>.trx` is specified.

**Expected**: Exit code 0; TRX file contains passing test results.

### IntegrationTest_FailingAssertions_WritesJUnitWithFailedResults

**Scenario**: At least one assertion fails and `--results <path>.xml` is specified.

**Expected**: Exit code non-zero; JUnit file contains failing test results.

### IntegrationTest_MinCountConstraint_TooFewFiles_ReturnsNonZero

**Scenario**: A `min` constraint is configured but fewer than the required number of files exist.

**Expected**: Exit code non-zero.

### IntegrationTest_MaxCountConstraint_TooManyFiles_ReturnsNonZero

**Scenario**: A `max` constraint is configured but more files exist than allowed.

**Expected**: Exit code non-zero.

### IntegrationTest_RegexRule_MatchingContent_ReturnsZero

**Scenario**: A `matches` regex rule is configured and the file content matches the pattern.

**Expected**: Exit code 0.

### IntegrationTest_RegexRule_NonMatchingContent_ReturnsNonZero

**Scenario**: A `matches` regex rule is configured but the file content does not match.

**Expected**: Exit code non-zero.

### IntegrationTest_ExactCountConstraint_WrongCount_ReturnsNonZero

**Scenario**: An exact `count` constraint is configured but the actual file count differs.

**Expected**: Exit code non-zero.

### IntegrationTest_FileSizeConstraints_TooSmall_ReturnsNonZero

**Scenario**: A minimum file size constraint is configured but the file is too small.

**Expected**: Exit code non-zero.

### IntegrationTest_FileSizeConstraints_TooLarge_ReturnsNonZero

**Scenario**: A maximum file size constraint is configured but the file is too large.

**Expected**: Exit code non-zero.

### IntegrationTest_DoesNotContainRule_ForbiddenTextPresent_ReturnsNonZero

**Scenario**: A `does-not-contain` rule is configured and the forbidden text is present.

**Expected**: Exit code non-zero.

### IntegrationTest_DoesNotContainRegexRule_ForbiddenPatternMatches_ReturnsNonZero

**Scenario**: A `does-not-match` regex rule is configured and the forbidden pattern matches.

**Expected**: Exit code non-zero.

### IntegrationTest_XmlAssert_PassingQuery_ReturnsZero

**Scenario**: An XML XPath assertion is configured and the query matches the expected result.

**Expected**: Exit code 0.

### IntegrationTest_XmlAssert_InvalidFile_ReturnsNonZero

**Scenario**: An XML assertion is configured but the target file is not valid XML.

**Expected**: Exit code non-zero.

### IntegrationTest_HtmlAssert_PassingQuery_ReturnsZero

**Scenario**: An HTML XPath assertion is configured and the query matches the expected result.

**Expected**: Exit code 0.

### IntegrationTest_YamlAssert_PassingQuery_ReturnsZero

**Scenario**: A YAML path assertion is configured and the query matches the expected result.

**Expected**: Exit code 0.

### IntegrationTest_JsonAssert_PassingQuery_ReturnsZero

**Scenario**: A JSON path assertion is configured and the query matches the expected result.

**Expected**: Exit code 0.

### IntegrationTest_PdfAssert_InvalidFile_ReturnsNonZero

**Scenario**: A PDF assertion is configured but the target file is not a valid PDF.

**Expected**: Exit code non-zero.

### FileAssertZipAssert_Run_MatchingEntriesMeetConstraints_NoError

**Scenario**: A zip assertion is configured and the archive contains entries that satisfy
the declared minimum and maximum count constraints.

**Expected**: Exit code 0.

### FileAssertZipAssert_Run_TooFewMatchingEntries_WritesError

**Scenario**: A zip assertion is configured with a minimum count but the archive contains
fewer matching entries than required.

**Expected**: Exit code non-zero.

## Requirements Coverage

- **Version display**: IntegrationTest_VersionFlag_OutputsVersion
- **Help display**: IntegrationTest_HelpFlag_OutputsUsageInformation
- **Self-validation**: IntegrationTest_ValidateFlag_RunsValidation,
  IntegrationTest_ValidateWithResults_GeneratesTrxFile,
  IntegrationTest_ValidateWithResults_GeneratesJUnitFile
- **Silent mode**: IntegrationTest_SilentFlag_SuppressesOutput
- **Log file output**: IntegrationTest_LogFlag_WritesOutputToFile
- **Invalid argument rejection**: IntegrationTest_UnknownArgument_ReturnsError
- **Test filtering**: IntegrationTest_TestFiltering_OnlyRunsMatchingTests
- **File assertions**: IntegrationTest_ValidConfig_PassingAssertions_ReturnsZero,
  IntegrationTest_ValidConfig_FailingAssertions_ReturnsNonZero
- **Results output**: IntegrationTest_PassingAssertions_WritesTrxWithPassedResults,
  IntegrationTest_FailingAssertions_WritesJUnitWithFailedResults
- **Count/size constraints**: IntegrationTest_MinCountConstraint_TooFewFiles_ReturnsNonZero,
  IntegrationTest_MaxCountConstraint_TooManyFiles_ReturnsNonZero,
  IntegrationTest_ExactCountConstraint_WrongCount_ReturnsNonZero,
  IntegrationTest_FileSizeConstraints_TooSmall_ReturnsNonZero,
  IntegrationTest_FileSizeConstraints_TooLarge_ReturnsNonZero
- **Text rules**: IntegrationTest_RegexRule_MatchingContent_ReturnsZero,
  IntegrationTest_RegexRule_NonMatchingContent_ReturnsNonZero,
  IntegrationTest_DoesNotContainRule_ForbiddenTextPresent_ReturnsNonZero,
  IntegrationTest_DoesNotContainRegexRule_ForbiddenPatternMatches_ReturnsNonZero
- **Structured file assertions**: IntegrationTest_XmlAssert_PassingQuery_ReturnsZero,
  IntegrationTest_XmlAssert_InvalidFile_ReturnsNonZero,
  IntegrationTest_HtmlAssert_PassingQuery_ReturnsZero,
  IntegrationTest_YamlAssert_PassingQuery_ReturnsZero,
  IntegrationTest_JsonAssert_PassingQuery_ReturnsZero,
  IntegrationTest_PdfAssert_InvalidFile_ReturnsNonZero
- **Zip archive assertions**: FileAssertZipAssert_Run_MatchingEntriesMeetConstraints_NoError,
  FileAssertZipAssert_Run_TooFewMatchingEntries_WritesError
