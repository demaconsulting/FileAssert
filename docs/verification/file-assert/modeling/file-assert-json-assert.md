### FileAssertJsonAssert Verification

This document describes the unit-level verification design for the `FileAssertJsonAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertJsonAssert.cs`.

#### Verification Approach

`FileAssertJsonAssert` is verified with unit tests defined in `FileAssertJsonAssertTests.cs`. Tests
create temporary JSON files with controlled content and assert on path query results and count
constraints.

#### Dependencies

| Dependency | Usage in Tests                                              |
|------------|-------------------------------------------------------------|
| `Context`  | Used directly (not mocked) — created with controlled flags. |

#### Test Scenarios

##### FileAssertJsonAssert_Create_ValidData_CreatesJsonAssert

**Scenario**: `FileAssertJsonAssert.Create` is called with valid data.

**Expected**: A non-null `FileAssertJsonAssert` instance is returned.

**Requirement coverage**: JSON assert creation requirement.

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

##### FileAssertJsonAssert_Run_ArrayCount_Matches_NoError

**Scenario**: `FileAssertJsonAssert.Run` is called with an exact count assertion and the path
query returns a JSON array with exactly the expected number of elements.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Array count match requirement.

##### FileAssertJsonAssert_Run_ArrayCount_Mismatch_WritesError

**Scenario**: `FileAssertJsonAssert.Run` is called with an exact count assertion and the path
query returns a different number of elements.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Array count mismatch requirement.

##### FileAssertJsonAssert_Run_MinMaxCount_WithinBounds_NoError

**Scenario**: `FileAssertJsonAssert.Run` is called with min/max count constraints and the result
count is within bounds.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Min/max count constraint pass requirement.

##### FileAssertJsonAssert_Run_ScalarValue_CountsAsOne_NoError

**Scenario**: `FileAssertJsonAssert.Run` is called on a path that resolves to a scalar JSON value;
a count of 1 is asserted.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Scalar value counts as one requirement.

##### FileAssertJsonAssert_Run_MinCount_BelowMinimum_WritesError

**Scenario**: `FileAssertJsonAssert.Run` is called with a minimum count constraint that is not
satisfied.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Minimum count constraint requirement.

##### FileAssertJsonAssert_Run_MaxCount_ExceedsMaximum_WritesError

**Scenario**: `FileAssertJsonAssert.Run` is called with a maximum count constraint that is
exceeded.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Maximum count constraint requirement.

#### Requirements Coverage

- **JSON assert creation**: FileAssertJsonAssert_Create_ValidData_CreatesJsonAssert
- **Null guard**: FileAssertJsonAssert_Create_NullData_ThrowsArgumentNullException
- **Query validation**: FileAssertJsonAssert_Create_EmptyQuery_ThrowsInvalidOperationException,
  FileAssertJsonAssert_Create_TrailingDotQuery_ThrowsInvalidOperationException,
  FileAssertJsonAssert_Create_LeadingDotQuery_ThrowsInvalidOperationException,
  FileAssertJsonAssert_Create_ConsecutiveDotsQuery_ThrowsInvalidOperationException
- **Invalid file**: FileAssertJsonAssert_Run_InvalidFile_WritesError
- **Count constraints**: FileAssertJsonAssert_Run_ArrayCount_Matches_NoError,
  FileAssertJsonAssert_Run_ArrayCount_Mismatch_WritesError,
  FileAssertJsonAssert_Run_MinMaxCount_WithinBounds_NoError,
  FileAssertJsonAssert_Run_ScalarValue_CountsAsOne_NoError,
  FileAssertJsonAssert_Run_MinCount_BelowMinimum_WritesError,
  FileAssertJsonAssert_Run_MaxCount_ExceedsMaximum_WritesError
