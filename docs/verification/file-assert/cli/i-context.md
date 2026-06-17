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

N/A – Acceptance criteria are managed at the subsystem and system integration levels.
Unit tests provide fine-grained coverage evidence; formal acceptance is declared at the
subsystem level when all unit tests supporting a subsystem requirement pass.

#### Dependencies

`ScopedContext` depends on `Context` for error accumulation. No external dependencies
require mocking at this level.

#### Test Scenarios

##### Context_WithPrefix_ReturnsNonNullScopedContext

**Scenario**: `context.WithPrefix("archive.zip")` is called on a valid root context.

**Expected**: The returned `IContext` instance is not null.

**Requirement coverage**: IContext output contract requirement.

##### Context_WithPrefix_NullPrefix_ThrowsArgumentNullException

**Scenario**: `context.WithPrefix(null!)` is called on a valid root context.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null argument guard.

**Requirement coverage**: IContext output contract requirement.

##### ScopedContext_WriteError_PropagatesExitCodeToRoot

**Scenario**: An error is written via a scoped context derived from a root context.

**Expected**: `context.ExitCode` is `1` and `context.ErrorCount` is `1`.

**Requirement coverage**: Error propagation to root context.

##### ScopedContext_WriteLine_DoesNotSetError

**Scenario**: An informational message is written via a scoped context.

**Expected**: `context.ExitCode` is `0` and `context.ErrorCount` is `0`.

**Requirement coverage**: Informational output does not affect error state.

##### ScopedContext_Nested_WriteError_PropagatesExitCodeToRoot

**Scenario**: Two levels of `WithPrefix` are applied; an error is written via the deepest scope.

**Expected**: `context.ExitCode` is `1` and `context.ErrorCount` is `1`.

**Requirement coverage**: Multi-level prefix chaining and error propagation.

##### ScopedContext_MultipleErrors_AllAccumulateOnRoot

**Scenario**: Two separate scoped contexts and the root context each write one error.

**Expected**: `context.ErrorCount` is `3` and `context.ExitCode` is `1`.

**Requirement coverage**: Error accumulation across multiple scopes.

#### Requirements Coverage

- (output contract — WithPrefix returns scoped context): Context_WithPrefix_ReturnsNonNullScopedContext
- (null prefix rejection): Context_WithPrefix_NullPrefix_ThrowsArgumentNullException
- (error propagates to root): ScopedContext_WriteError_PropagatesExitCodeToRoot
- (informational output has no error effect): ScopedContext_WriteLine_DoesNotSetError
- (nested prefix chaining): ScopedContext_Nested_WriteError_PropagatesExitCodeToRoot
- (multiple errors accumulate): ScopedContext_MultipleErrors_AllAccumulateOnRoot
