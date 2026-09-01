### IContext Verification

This document describes the unit-level verification design for the `IContext` interface. It
defines the test scenarios, dependency usage, and requirement coverage for `Cli/IContext.cs`.

#### Verification Approach

`IContext` has no implementation of its own; it is verified indirectly through `Context`, its
sole implementer. Tests defined in `ContextTests.cs` exercise the `WriteLine` and `WriteError`
contract members via the concrete `Context` instance, confirming that output/error reporting
and exit-code state behave as `IContext` consumers (the asserters) expect. No mocking or test
doubles are needed at this level because the tests operate directly on a `Context` instance
created with the standard `Context.Create` factory.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. The test
collection is marked `[Collection("Sequential")]` to prevent parallel execution of tests that
share `Console` state. No special hardware, peripherals, or environment configuration is required
beyond the standard build toolchain.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `IContext.cs`
meets the project minimum threshold.

#### Dependencies

No external dependencies require mocking at this level.

#### Test Scenarios

##### Context_WriteLine_NotSilent_WritesToConsole

**Scenario**: `context.WriteLine("Test message")` is called on a context created without
`--silent`.

**Expected**: The message appears on console standard output.

##### Context_WriteError_SetsErrorExitCode

**Scenario**: `context.WriteError("Test error message")` is called on a valid context.

**Expected**: `context.ExitCode` is `1`.
