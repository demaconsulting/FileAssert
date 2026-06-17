### FileAssertYamlAssert Verification

This document describes the unit-level verification design for the `FileAssertYamlAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertYamlAssert.cs`.

#### Verification Approach

`FileAssertYamlAssert` is verified with unit tests defined in `FileAssertYamlAssertTests.cs`. Tests
create temporary YAML files with controlled content and assert on path query results and count
constraints.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `FileAssertYamlAssert.cs`
meets the project minimum threshold.

#### Dependencies

| Dependency               | Usage in Tests                                                       |
|--------------------------|----------------------------------------------------------------------|
| `Context`                | Used directly (not mocked) — created with controlled flags.          |
| `DirectoryFileContainer` | Used directly to open temporary YAML files as container entries.     |
| Temporary file system    | Real temporary files written to disk provide controlled YAML inputs. |

#### Test Scenarios

##### FileAssertYamlAssert_Create_ValidData_CreatesYamlAssert

**Scenario**: `FileAssertYamlAssert.Create` is called with valid data.

**Expected**: A non-null `FileAssertYamlAssert` instance is returned.

##### FileAssertYamlAssert_Create_NullData_ThrowsArgumentNullException

**Scenario**: `FileAssertYamlAssert.Create` is called with `null` data.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null data guard.

##### FileAssertYamlAssert_Create_EmptyQuery_ThrowsInvalidOperationException

**Scenario**: `FileAssertYamlAssert.Create` is called with data whose query is empty.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Empty query validation.

##### FileAssertYamlAssert_Create_TrailingDotQuery_ThrowsInvalidOperationException

**Scenario**: `FileAssertYamlAssert.Create` is called with a query that ends with a dot.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Malformed query validation.

##### FileAssertYamlAssert_Create_LeadingDotQuery_ThrowsInvalidOperationException

**Scenario**: `FileAssertYamlAssert.Create` is called with a query that starts with a dot.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Malformed query validation.

##### FileAssertYamlAssert_Create_ConsecutiveDotsQuery_ThrowsInvalidOperationException

**Scenario**: `FileAssertYamlAssert.Create` is called with a query containing consecutive dots.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Malformed query validation.

##### FileAssertYamlAssert_Run_InvalidFile_WritesError

**Scenario**: `FileAssertYamlAssert.Run` is called with a path that is not valid YAML.

**Expected**: An error is written to the context; exit code is non-zero.

**Boundary / error path**: Invalid YAML file error path.

##### FileAssertYamlAssert_Run_InvalidFile_RemainingAssertionsSkipped

**Scenario**: `FileAssertYamlAssert.Run` is called on a malformed YAML file while two queries
are configured.

**Expected**: Exactly one error is written (the parse failure); the configured query
assertions are not evaluated.

**Boundary / error path**: Parse-error short-circuit — remaining assertions skipped.

##### FileAssertYamlAssert_Run_SequenceCount_Matches_NoError

**Scenario**: `FileAssertYamlAssert.Run` is called with an exact count assertion and the path
query returns a sequence with exactly the expected number of items.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertYamlAssert_Run_SequenceCount_Mismatch_WritesError

**Scenario**: `FileAssertYamlAssert.Run` is called with an exact count assertion and the path
query returns a different count.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertYamlAssert_Run_MinMaxCount_WithinBounds_NoError

**Scenario**: `FileAssertYamlAssert.Run` is called with min/max count constraints and the result
count is within bounds.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertYamlAssert_Run_ScalarValue_CountsAsOne_NoError

**Scenario**: `FileAssertYamlAssert.Run` is called on a path that resolves to a scalar value;
a count of 1 is asserted.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertYamlAssert_Run_EmptyDocument_ReportsZeroCount

**Scenario**: `FileAssertYamlAssert.Run` is called with an empty YAML file (a stream with no
documents).

**Expected**: The query returns a count of 0; if the assertion specifies `min: 1` an error is
written; if no lower bound is set no error is written.

**Boundary / error path**: Empty YAML document edge case.

##### FileAssertYamlAssert_Run_MinCount_BelowMinimum_WritesError

**Scenario**: `FileAssertYamlAssert.Run` is called with a minimum count constraint that is not
satisfied.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertYamlAssert_Run_MaxCount_ExceedsMaximum_WritesError

**Scenario**: `FileAssertYamlAssert.Run` is called with a maximum count constraint that is
exceeded.

**Expected**: An error is written to the context; exit code is non-zero.
