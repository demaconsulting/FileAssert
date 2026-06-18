### FileAssertJsonAssert Verification

This document describes the unit-level verification design for the `FileAssertJsonAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertJsonAssert.cs`.

#### Verification Approach

`FileAssertJsonAssert` is verified with unit tests defined in `FileAssertJsonAssertTests.cs`. Tests
create temporary JSON files with controlled content and assert on path query results and count
constraints.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Each scenario asserts on
specific `IContext.ExitCode` and `WriteError` outcomes that uniquely determine pass/fail.

#### Dependencies

| Dependency | Usage in Tests                                              |
|------------|-------------------------------------------------------------|
| `Context`  | Used directly (not mocked) — created with controlled flags. |

#### Test Scenarios

##### FileAssertJsonAssert_Create_ValidData_CreatesJsonAssert

**Scenario**: `FileAssertJsonAssert.Create` is called with valid data.

**Expected**: A non-null `FileAssertJsonAssert` instance is returned.

##### FileAssertJsonAssert_Create_NullData_ThrowsArgumentNullException

**Scenario**: `FileAssertJsonAssert.Create` is called with `null` data.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null data guard.

##### FileAssertJsonAssert_Create_EmptyQuery_ThrowsInvalidOperationException

**Scenario**: `FileAssertJsonAssert.Create` is called with data whose query is empty.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Empty query validation.

##### FileAssertJsonAssert_Create_TrailingDotQuery_ThrowsInvalidOperationException

**Scenario**: `FileAssertJsonAssert.Create` is called with a query that ends with a dot.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Malformed query validation.

##### FileAssertJsonAssert_Create_LeadingDotQuery_ThrowsInvalidOperationException

**Scenario**: `FileAssertJsonAssert.Create` is called with a query that starts with a dot.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Malformed query validation.

##### FileAssertJsonAssert_Create_ConsecutiveDotsQuery_ThrowsInvalidOperationException

**Scenario**: `FileAssertJsonAssert.Create` is called with a query containing consecutive dots.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Malformed query validation.

##### FileAssertJsonAssert_Run_InvalidFile_WritesError

**Scenario**: `FileAssertJsonAssert.Run` is called with a path that is not valid JSON.

**Expected**: An error is written to the context; exit code is non-zero.

**Boundary / error path**: Invalid JSON file error path.

##### FileAssertJsonAssert_Run_InvalidJson_WritesParseError

**Scenario**: `FileAssertJsonAssert.Run` is called on a file whose content is not valid JSON.

**Expected**: Exactly one error is written, and its message identifies a parse failure
(`could not be parsed as a JSON document`), distinct from an IO failure.

**Boundary / error path**: JSON parse-error reporting.

##### FileAssertJsonAssert_Run_IOError_WritesReadError

**Scenario**: `FileAssertJsonAssert.Run` is called with a container whose `OpenEntry` raises an
`UnauthorizedAccessException`.

**Expected**: Exactly one error is written, and its message identifies an IO failure
(`could not be read`), distinct from a parse failure.

**Boundary / error path**: IO-error reporting.

##### FileAssertJsonAssert_Run_ArrayCount_Matches_NoError

**Scenario**: `FileAssertJsonAssert.Run` is called with an exact count assertion and the path
query returns a JSON array with exactly the expected number of elements.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertJsonAssert_Run_ArrayCount_Mismatch_WritesError

**Scenario**: `FileAssertJsonAssert.Run` is called with an exact count assertion and the path
query returns a different number of elements.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertJsonAssert_Run_MinMaxCount_WithinBounds_NoError

**Scenario**: `FileAssertJsonAssert.Run` is called with min/max count constraints and the result
count is within bounds.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertJsonAssert_Run_ScalarValue_CountsAsOne_NoError

**Scenario**: `FileAssertJsonAssert.Run` is called on a path that resolves to a scalar JSON value;
a count of 1 is asserted.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertJsonAssert_Run_MinCount_BelowMinimum_WritesError

**Scenario**: `FileAssertJsonAssert.Run` is called with a minimum count constraint that is not
satisfied.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertJsonAssert_Run_MaxCount_ExceedsMaximum_WritesError

**Scenario**: `FileAssertJsonAssert.Run` is called with a maximum count constraint that is
exceeded.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertJsonAssert_Run_MultipleQueries_InvalidJson_ShortCircuitsAfterParseError

**Scenario**: `FileAssertJsonAssert.Run` is configured with two or more path queries and is
invoked against a file whose content is not valid JSON.

**Expected**: Exactly one parse error is written for the file; subsequent queries are
short-circuited (the parse failure is reported once, not once per query).

**Boundary / error path**: Multi-query parse-error short-circuit.
