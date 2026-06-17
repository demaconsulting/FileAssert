### FileAssertXmlAssert Verification

This document describes the unit-level verification design for the `FileAssertXmlAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertXmlAssert.cs`.

#### Verification Approach

`FileAssertXmlAssert` is verified with unit tests defined in `FileAssertXmlAssertTests.cs`. Tests
create temporary XML files with controlled content and assert on XPath query results, count
constraints, and text matching.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `FileAssertXmlAssert.cs`
meets the project minimum threshold.

#### Dependencies

| Dependency | Usage in Tests                                              |
|------------|-------------------------------------------------------------|
| `Context`  | Used directly (not mocked) — created with controlled flags. |

#### Test Scenarios

##### FileAssertXmlAssert_Create_ValidData_CreatesXmlAssert

**Scenario**: `FileAssertXmlAssert.Create` is called with valid data.

**Expected**: A non-null `FileAssertXmlAssert` instance is returned.

##### FileAssertXmlAssert_Create_NullData_ThrowsArgumentNullException

**Scenario**: `FileAssertXmlAssert.Create` is called with `null` data.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null data guard.

##### FileAssertXmlAssert_Create_BlankQuery_ThrowsInvalidOperationException

**Scenario**: `FileAssertXmlAssert.Create` is called with a query that is blank or
whitespace-only.

**Expected**: An `InvalidOperationException` is thrown at construction time, before any file
system or XML parsing is attempted.

**Boundary / error path**: Blank-query validation guard.

##### FileAssertXmlAssert_Run_InvalidFile_WritesError

**Scenario**: `FileAssertXmlAssert.Run` is called with a path that is not valid XML.

**Expected**: An error is written to the context; exit code is non-zero.

**Boundary / error path**: Invalid XML file error path.

##### FileAssertXmlAssert_Run_ExactCount_Matches_NoError

**Scenario**: `FileAssertXmlAssert.Run` is called with an exact count assertion and the XPath
query returns exactly the expected number of nodes.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertXmlAssert_Run_ExactCount_Mismatch_WritesError

**Scenario**: `FileAssertXmlAssert.Run` is called with an exact count assertion and the XPath
query returns a different number of nodes.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertXmlAssert_Run_MinMaxCount_WithinBounds_NoError

**Scenario**: `FileAssertXmlAssert.Run` is called with min/max count constraints and the XPath
query result count is within bounds.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertXmlAssert_Run_MinCount_NotMet_WritesError

**Scenario**: `FileAssertXmlAssert.Run` is called with a `min` count constraint and the XPath
query returns fewer nodes than the minimum.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertXmlAssert_Run_MaxCount_Exceeded_WritesError

**Scenario**: `FileAssertXmlAssert.Run` is called with a `max` count constraint and the XPath
query returns more nodes than the maximum.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertXmlAssert_Run_InvalidXPathQuery_WritesError

**Scenario**: `FileAssertXmlAssert.Run` is called with a malformed XPath query string.

**Expected**: An error is written to the context; exit code is non-zero.

**Boundary / error path**: Invalid XPath query error path.

##### FileAssertXmlAssert_Run_XPathExactTextMatch_Matches_NoError

**Scenario**: `FileAssertXmlAssert.Run` is called with an exact-text assertion and the first
XPath result node matches exactly.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertXmlAssert_Run_XPathExactTextMatch_NoMatch_WritesError

**Scenario**: `FileAssertXmlAssert.Run` is called with an exact-text assertion but the XPath
result does not match.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertXmlAssert_Run_XPathContainsText_Matches_NoError

**Scenario**: `FileAssertXmlAssert.Run` is called with a `contains` text assertion and the XPath
result contains the expected value.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertXmlAssert_Run_XPathContainsText_NoMatch_WritesError

**Scenario**: `FileAssertXmlAssert.Run` is called with a `contains` text assertion but the XPath
result does not contain the expected value.

**Expected**: An error is written to the context; exit code is non-zero.
