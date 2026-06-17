## SelfTest Subsystem Verification

This document describes the subsystem-level verification design for the `SelfTest` subsystem. It
defines the integration test approach, subsystem boundary, mocking strategy, and test scenarios
that together verify the `SelfTest` subsystem requirements.

### Verification Approach

The `SelfTest` subsystem is verified by integration tests defined in `SelfTestTests.cs`. Each
test exercises the `Validation.Run` method with a real `Context` to confirm that the subsystem
produces correct output and result files across the supported result-format options.

### Dependencies and Mocking Strategy

At the subsystem boundary, `Context` (from the `Cli` subsystem) and `PathHelpers` (from the
`Utilities` subsystem) are used with their real implementations. No mocking is applied. Temporary
directories are used for result file output so that tests remain isolated.

### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner against
the .NET runtime specified by the build matrix. No special hardware, peripherals, or
environment configuration is required beyond the standard build toolchain.

### Acceptance Criteria

The SelfTest subsystem verification passes when all test scenarios listed in
this document execute and pass in the CI pipeline without any test failures, unexpected
exceptions, or assertion errors. Each named scenario must pass on all supported runtime
and platform combinations.

### Test Scenarios

The following integration test scenarios are defined in `SelfTestTests.cs`.

#### SelfTest_Run_ExecutesBuiltInTestsAndProducesSummary

**Scenario**: `Validation.Run` is called with a real context.

**Expected**: Validation completes without error; exit code is 0; output contains a summary
including "Total Tests:".

#### SelfTest_Run_WhenInvoked_PrintsSystemInfoHeader

**Scenario**: `Validation.Run` is called with a real context.

**Expected**: Output contains a system information header.

#### SelfTest_Run_WithResultsFile_WritesTrxResultsFile

**Scenario**: `Validation.Run` is called with a context whose `ResultsFile` points to a temporary
`.trx` path.

**Expected**: A TRX file is created at the specified path; the file contains a `<TestRun` XML
element; exit code is 0.

#### SelfTest_Run_WithXmlResultsFile_WritesXmlResultsFile

**Scenario**: `Validation.Run` is called with a context whose `ResultsFile` points to a temporary
`.xml` path.

**Expected**: An XML results file is created at the specified path and exit code is 0. The
detailed contents of the XML file are verified by the unit-level scenarios in
`docs/verification/file-assert/selftest/validation.md`.
