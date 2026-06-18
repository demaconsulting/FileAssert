### IContext Verification

This document describes the unit-level verification design for the `IContext` interface and its
`ScopedContext` implementation. It defines the test scenarios, dependency usage, and requirement
coverage for `Cli/IContext.cs` and the nested `ScopedContext` class inside `Cli/Context.cs`.

#### Verification Approach

`IContext` and `ScopedContext` are verified with unit tests defined in `ScopedContextTests.cs`.
Tests exercise `Context.WithPrefix`, error propagation from scoped contexts to the root context,
and multi-level nesting. No mocking or test doubles are needed because the tests operate directly
on a `Context` instance created with `["--silent"]` to suppress console output.

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

`ScopedContext` depends on `Context` for error accumulation. No external dependencies
require mocking at this level.

#### Test Scenarios

##### Context_WithPrefix_ReturnsNonNullScopedContext

**Scenario**: `context.WithPrefix("archive.zip")` is called on a valid root context.

**Expected**: The returned `IContext` instance is not null.

##### Context_WithPrefix_NullPrefix_ThrowsArgumentNullException

**Scenario**: `context.WithPrefix(null!)` is called on a valid root context.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null argument guard.

##### ScopedContext_WriteError_PropagatesExitCodeToRoot

**Scenario**: An error is written via a scoped context derived from a root context.

**Expected**: `context.ExitCode` is `1` and `context.ErrorCount` is `1`.

##### ScopedContext_WriteLine_DoesNotSetError

**Scenario**: An informational message is written via a scoped context.

**Expected**: `context.ExitCode` is `0` and `context.ErrorCount` is `0`.

##### ScopedContext_Nested_WriteError_PropagatesExitCodeToRoot

**Scenario**: Two levels of `WithPrefix` are applied; an error is written via the deepest scope.

**Expected**: `context.ExitCode` is `1` and `context.ErrorCount` is `1`.

##### ScopedContext_MultipleErrors_AllAccumulateOnRoot

**Scenario**: Two separate scoped contexts and the root context each write one error.

**Expected**: `context.ErrorCount` is `3` and `context.ExitCode` is `1`.
