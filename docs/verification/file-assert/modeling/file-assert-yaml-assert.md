### FileAssertYamlAssert Verification

This document describes the unit-level verification design for the `FileAssertYamlAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertYamlAssert.cs`.

#### Verification Approach

`FileAssertYamlAssert` is verified with unit tests defined in `FileAssertYamlAssertTests.cs`. Tests
create temporary YAML files with controlled content and assert on path query results and count
constraints.

#### Dependencies

| Dependency | Usage in Tests                                              |
|------------|-------------------------------------------------------------|
| `Context`  | Used directly (not mocked) — created with controlled flags. |

#### Test Scenarios

##### FileAssertYamlAssert_Create_ValidData_CreatesYamlAssert

**Scenario**: `FileAssertYamlAssert.Create` is called with valid data.

**Expected**: A non-null `FileAssertYamlAssert` instance is returned.

**Requirement coverage**: YAML assert creation requirement.

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

##### FileAssertYamlAssert_Run_SequenceCount_Matches_NoError

**Scenario**: `FileAssertYamlAssert.Run` is called with an exact count assertion and the path
query returns a sequence with exactly the expected number of items.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Sequence count match requirement.

##### FileAssertYamlAssert_Run_SequenceCount_Mismatch_WritesError

**Scenario**: `FileAssertYamlAssert.Run` is called with an exact count assertion and the path
query returns a different count.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Sequence count mismatch requirement.

##### FileAssertYamlAssert_Run_MinMaxCount_WithinBounds_NoError

**Scenario**: `FileAssertYamlAssert.Run` is called with min/max count constraints and the result
count is within bounds.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Min/max count constraint pass requirement.

##### FileAssertYamlAssert_Run_ScalarValue_CountsAsOne_NoError

**Scenario**: `FileAssertYamlAssert.Run` is called on a path that resolves to a scalar value;
a count of 1 is asserted.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Scalar value counts as one requirement.

##### FileAssertYamlAssert_Run_MinCount_BelowMinimum_WritesError

**Scenario**: `FileAssertYamlAssert.Run` is called with a minimum count constraint that is not
satisfied.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Minimum count constraint requirement.

##### FileAssertYamlAssert_Run_MaxCount_ExceedsMaximum_WritesError

**Scenario**: `FileAssertYamlAssert.Run` is called with a maximum count constraint that is
exceeded.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Maximum count constraint requirement.

#### Requirements Coverage

- **YAML assert creation**: FileAssertYamlAssert_Create_ValidData_CreatesYamlAssert
- **Null guard**: FileAssertYamlAssert_Create_NullData_ThrowsArgumentNullException
- **Query validation**: FileAssertYamlAssert_Create_EmptyQuery_ThrowsInvalidOperationException,
  FileAssertYamlAssert_Create_TrailingDotQuery_ThrowsInvalidOperationException,
  FileAssertYamlAssert_Create_LeadingDotQuery_ThrowsInvalidOperationException,
  FileAssertYamlAssert_Create_ConsecutiveDotsQuery_ThrowsInvalidOperationException
- **Invalid file**: FileAssertYamlAssert_Run_InvalidFile_WritesError
- **Count constraints**: FileAssertYamlAssert_Run_SequenceCount_Matches_NoError,
  FileAssertYamlAssert_Run_SequenceCount_Mismatch_WritesError,
  FileAssertYamlAssert_Run_MinMaxCount_WithinBounds_NoError,
  FileAssertYamlAssert_Run_ScalarValue_CountsAsOne_NoError,
  FileAssertYamlAssert_Run_MinCount_BelowMinimum_WritesError,
  FileAssertYamlAssert_Run_MaxCount_ExceedsMaximum_WritesError
