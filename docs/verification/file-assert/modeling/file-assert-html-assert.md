### FileAssertHtmlAssert Verification

This document describes the unit-level verification design for the `FileAssertHtmlAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertHtmlAssert.cs`.

#### Verification Approach

`FileAssertHtmlAssert` is verified with unit tests defined in `FileAssertHtmlAssertTests.cs`. Tests
create temporary HTML files with controlled content and assert on XPath query results, count
constraints, and text matching. Because the underlying HTML parser is lenient, parsing never
fails on syntactically imperfect markup; only IO failures (missing or inaccessible files) are
verified to produce errors.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `FileAssertHtmlAssert.cs`
meets the project minimum threshold.

#### Dependencies

| Dependency               | Usage in Tests                                                          |
|--------------------------|-------------------------------------------------------------------------|
| `Context`                | Used directly (not mocked) — created with controlled flags.             |
| `CapturingContext`       | Test double captures `WriteError` calls for assertion of error wording. |
| `ThrowingFileContainer`  | Test double simulates IO failures from `OpenEntry`.                     |
| `DirectoryFileContainer` | Real implementation; backs file-system fixtures used by the tests.      |

#### Test Scenarios

##### FileAssertHtmlAssert_Create_ValidData_CreatesHtmlAssert

**Scenario**: `FileAssertHtmlAssert.Create` is called with valid data.

**Expected**: A non-null `FileAssertHtmlAssert` instance is returned.

##### FileAssertHtmlAssert_Create_NullData_ThrowsArgumentNullException

**Scenario**: `FileAssertHtmlAssert.Create` is called with `null` data.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null data guard.

##### FileAssertHtmlAssert_Run_ExactCount_Matches_NoError

**Scenario**: `FileAssertHtmlAssert.Run` is called with an exact count assertion and the XPath
query returns exactly the expected number of elements.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertHtmlAssert_Run_ExactCount_Mismatch_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with an exact count assertion and the XPath
query returns a different number of elements.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertHtmlAssert_Run_MinMaxCount_WithinBounds_NoError

**Scenario**: `FileAssertHtmlAssert.Run` is called with min/max count constraints and the XPath
query result count is within bounds.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertHtmlAssert_Run_MinCount_BelowMinimum_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with a `min` count constraint and the XPath
query returns fewer elements than the minimum.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertHtmlAssert_Run_MaxCount_ExceedsMaximum_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with a `max` count constraint and the XPath
query returns more elements than the maximum.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertHtmlAssert_Run_NonExistentFile_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with a path that does not exist.

**Expected**: An error is written to the context; exit code is non-zero.

**Boundary / error path**: Missing file error path.

##### FileAssertHtmlAssert_Run_UnauthorizedAccess_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with a container whose `OpenEntry` raises an
`UnauthorizedAccessException`.

**Expected**: Exactly one error is written reporting the IO failure; assertions are skipped.

**Boundary / error path**: IO (access-denied) error path.

##### FileAssertHtmlAssert_Run_InvalidXPathQuery_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with a malformed XPath query string.

**Expected**: An error is written to the context; exit code is non-zero.

**Boundary / error path**: Invalid XPath query error path.

##### FileAssertHtmlAssert_Run_XPathExactTextMatch_Matches_NoError

**Scenario**: `FileAssertHtmlAssert.Run` is called with an exact-text assertion and the XPath
result matches exactly.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertHtmlAssert_Run_XPathExactTextMatch_NoMatch_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with an exact-text assertion but the XPath
result does not match.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertHtmlAssert_Run_XPathContainsText_Matches_NoError

**Scenario**: `FileAssertHtmlAssert.Run` is called with a `contains` text assertion and the XPath
result contains the expected value.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertHtmlAssert_Run_XPathContainsText_NoMatch_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with a `contains` text assertion but the XPath
result does not contain the expected value.

**Expected**: An error is written to the context; exit code is non-zero.
