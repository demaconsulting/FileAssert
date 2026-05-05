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

### Integration Test Scenarios

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

### Requirements Coverage

- **Self-validation execution**: SelfTest_Run_ExecutesBuiltInTestsAndProducesSummary
- **System info header**: SelfTest_Run_WhenInvoked_PrintsSystemInfoHeader
- **TRX results output**: SelfTest_Run_WithResultsFile_WritesTrxResultsFile
