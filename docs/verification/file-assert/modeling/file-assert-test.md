### FileAssertTest Verification

This document describes the unit-level verification design for the `FileAssertTest` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertTest.cs`.

#### Verification Approach

`FileAssertTest` is verified with unit tests defined in `FileAssertTestTests.cs`. Tests supply
controlled `FileAssertTestData` instances and assert on filter matching behavior, creation
validation, and execution delegation.

#### Dependencies

| Dependency        | Usage in Tests                                                       |
|-------------------|----------------------------------------------------------------------|
| `Context`         | Used directly (not mocked) — created with controlled flags.          |
| `FileAssertFile`  | Used directly (not mocked) — called through `FileAssertTest.Run`.    |

#### Test Scenarios

##### FileAssertTest_Create_ValidData_CreatesTest

**Scenario**: `FileAssertTest.Create` is called with valid data containing a name and files list.

**Expected**: A non-null `FileAssertTest` instance is returned with correct properties.

**Requirement coverage**: Test creation requirement.

##### FileAssertTest_Create_NullData_ThrowsArgumentNullException

**Scenario**: `FileAssertTest.Create` is called with `null` data.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null guard on data.

##### FileAssertTest_Create_NullName_ThrowsInvalidOperationException

**Scenario**: `FileAssertTest.Create` is called with data whose `Name` property is `null`.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Null name validation.

##### FileAssertTest_Create_WhitespaceName_ThrowsInvalidOperationException

**Scenario**: `FileAssertTest.Create` is called with data whose `Name` property is whitespace.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Whitespace name validation.

##### FileAssertTest_MatchesFilter_EmptyFilters_ReturnsTrue

**Scenario**: `FileAssertTest.MatchesFilter` is called with an empty filter list.

**Expected**: Returns `true` (no filter means run all tests).

**Requirement coverage**: Empty filter match requirement.

##### FileAssertTest_MatchesFilter_MatchingName_ReturnsTrue

**Scenario**: `FileAssertTest.MatchesFilter` is called with a filter list containing the test name.

**Expected**: Returns `true`.

**Requirement coverage**: Name-based filter match requirement.

##### FileAssertTest_MatchesFilter_MatchingTag_ReturnsTrue

**Scenario**: `FileAssertTest.MatchesFilter` is called with a filter list containing one of the
test's tags.

**Expected**: Returns `true`.

**Requirement coverage**: Tag-based filter match requirement.

##### FileAssertTest_MatchesFilter_NonMatchingFilter_ReturnsFalse

**Scenario**: `FileAssertTest.MatchesFilter` is called with a filter list containing neither the
test name nor any of its tags.

**Expected**: Returns `false`.

**Requirement coverage**: Non-matching filter requirement.

##### FileAssertTest_MatchesFilter_CaseInsensitiveName_ReturnsTrue

**Scenario**: `FileAssertTest.MatchesFilter` is called with a filter that differs only in case
from the test name.

**Expected**: Returns `true`.

**Requirement coverage**: Case-insensitive name matching requirement.

##### FileAssertTest_MatchesFilter_CaseInsensitiveTag_ReturnsTrue

**Scenario**: `FileAssertTest.MatchesFilter` is called with a filter that differs only in case
from a test tag.

**Expected**: Returns `true`.

**Requirement coverage**: Case-insensitive tag matching requirement.

##### FileAssertTest_Run_RunsAllFiles

**Scenario**: `FileAssertTest.Run` is called on a test with multiple file entries.

**Expected**: All file entries are evaluated.

**Requirement coverage**: Run-all-files requirement.

##### FileAssertTest_Run_NullContext_ThrowsArgumentNullException

**Scenario**: `FileAssertTest.Run` is called with a `null` context.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null context guard.

##### FileAssertTest_Run_NullBasePath_ThrowsArgumentNullException

**Scenario**: `FileAssertTest.Run` is called with a `null` base path.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null base path guard.

#### Requirements Coverage

- **Test creation**: FileAssertTest_Create_ValidData_CreatesTest
- **Null data guard**: FileAssertTest_Create_NullData_ThrowsArgumentNullException
- **Null/whitespace name guard**: FileAssertTest_Create_NullName_ThrowsInvalidOperationException,
  FileAssertTest_Create_WhitespaceName_ThrowsInvalidOperationException
- **Empty filter match**: FileAssertTest_MatchesFilter_EmptyFilters_ReturnsTrue
- **Name filter**: FileAssertTest_MatchesFilter_MatchingName_ReturnsTrue,
  FileAssertTest_MatchesFilter_CaseInsensitiveName_ReturnsTrue
- **Tag filter**: FileAssertTest_MatchesFilter_MatchingTag_ReturnsTrue,
  FileAssertTest_MatchesFilter_CaseInsensitiveTag_ReturnsTrue
- **Non-matching filter**: FileAssertTest_MatchesFilter_NonMatchingFilter_ReturnsFalse
- **Run all files**: FileAssertTest_Run_RunsAllFiles
- **Null context guard**: FileAssertTest_Run_NullContext_ThrowsArgumentNullException
- **Null base path guard**: FileAssertTest_Run_NullBasePath_ThrowsArgumentNullException
