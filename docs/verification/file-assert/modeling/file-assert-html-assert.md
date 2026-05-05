### FileAssertHtmlAssert Verification

This document describes the unit-level verification design for the `FileAssertHtmlAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertHtmlAssert.cs`.

#### Verification Approach

`FileAssertHtmlAssert` is verified with unit tests defined in `FileAssertHtmlAssertTests.cs`. Tests
create temporary HTML files with controlled content and assert on XPath query results, count
constraints, and text matching.

#### Dependencies

| Dependency | Usage in Tests                                              |
|------------|-------------------------------------------------------------|
| `Context`  | Used directly (not mocked) — created with controlled flags. |

#### Test Scenarios

##### FileAssertHtmlAssert_Create_ValidData_CreatesHtmlAssert

**Scenario**: `FileAssertHtmlAssert.Create` is called with valid data.

**Expected**: A non-null `FileAssertHtmlAssert` instance is returned.

**Requirement coverage**: HTML assert creation requirement.

##### FileAssertHtmlAssert_Create_NullData_ThrowsArgumentNullException

**Scenario**: `FileAssertHtmlAssert.Create` is called with `null` data.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null data guard.

##### FileAssertHtmlAssert_Run_ExactCount_Matches_NoError

**Scenario**: `FileAssertHtmlAssert.Run` is called with an exact count assertion and the XPath
query returns exactly the expected number of elements.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Exact count match requirement.

##### FileAssertHtmlAssert_Run_ExactCount_Mismatch_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with an exact count assertion and the XPath
query returns a different number of elements.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Exact count mismatch requirement.

##### FileAssertHtmlAssert_Run_MinMaxCount_WithinBounds_NoError

**Scenario**: `FileAssertHtmlAssert.Run` is called with min/max count constraints and the XPath
query result count is within bounds.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Min/max count constraint pass requirement.

##### FileAssertHtmlAssert_Run_NonExistentFile_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with a path that does not exist.

**Expected**: An error is written to the context; exit code is non-zero.

**Boundary / error path**: Missing file error path.

##### FileAssertHtmlAssert_Run_InvalidXPathQuery_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with a malformed XPath query string.

**Expected**: An error is written to the context; exit code is non-zero.

**Boundary / error path**: Invalid XPath query error path.

##### FileAssertHtmlAssert_Run_XPathExactTextMatch_Matches_NoError

**Scenario**: `FileAssertHtmlAssert.Run` is called with an exact-text assertion and the XPath
result matches exactly.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: XPath exact text match pass requirement.

##### FileAssertHtmlAssert_Run_XPathExactTextMatch_NoMatch_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with an exact-text assertion but the XPath
result does not match.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: XPath exact text match fail requirement.

##### FileAssertHtmlAssert_Run_XPathContainsText_Matches_NoError

**Scenario**: `FileAssertHtmlAssert.Run` is called with a `contains` text assertion and the XPath
result contains the expected value.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: XPath contains text pass requirement.

##### FileAssertHtmlAssert_Run_XPathContainsText_NoMatch_WritesError

**Scenario**: `FileAssertHtmlAssert.Run` is called with a `contains` text assertion but the XPath
result does not contain the expected value.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: XPath contains text fail requirement.

#### Requirements Coverage

- **HTML assert creation**: FileAssertHtmlAssert_Create_ValidData_CreatesHtmlAssert
- **Null guard**: FileAssertHtmlAssert_Create_NullData_ThrowsArgumentNullException
- **Missing file**: FileAssertHtmlAssert_Run_NonExistentFile_WritesError
- **Invalid query**: FileAssertHtmlAssert_Run_InvalidXPathQuery_WritesError
- **Count constraints**: FileAssertHtmlAssert_Run_ExactCount_Matches_NoError,
  FileAssertHtmlAssert_Run_ExactCount_Mismatch_WritesError,
  FileAssertHtmlAssert_Run_MinMaxCount_WithinBounds_NoError
- **Text assertions**: FileAssertHtmlAssert_Run_XPathExactTextMatch_Matches_NoError,
  FileAssertHtmlAssert_Run_XPathExactTextMatch_NoMatch_WritesError,
  FileAssertHtmlAssert_Run_XPathContainsText_Matches_NoError,
  FileAssertHtmlAssert_Run_XPathContainsText_NoMatch_WritesError
