### FileAssertFile Verification

This document describes the unit-level verification design for the `FileAssertFile` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertFile.cs`.

#### Verification Approach

`FileAssertFile` is verified with unit tests defined in `FileAssertFileTests.cs`. Tests create
temporary directories with controlled file sets and assert on constraint evaluation and error
reporting behavior.

#### Dependencies

| Dependency   | Usage in Tests                                              |
|--------------|-------------------------------------------------------------|
| `Context`    | Used directly (not mocked) — created with controlled flags. |

#### Test Scenarios

##### FileAssertFile_Create_ValidData_CreatesFile

**Scenario**: `FileAssertFile.Create` is called with valid data containing a pattern.

**Expected**: A non-null `FileAssertFile` instance is returned.

**Requirement coverage**: File entry creation requirement.

##### FileAssertFile_Create_NullData_ThrowsArgumentNullException

**Scenario**: `FileAssertFile.Create` is called with `null` data.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null data guard.

##### FileAssertFile_Create_NullPattern_ThrowsInvalidOperationException

**Scenario**: `FileAssertFile.Create` is called with data whose `Pattern` is `null`.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Null pattern validation.

##### FileAssertFile_Create_BlankPattern_ThrowsInvalidOperationException

**Scenario**: `FileAssertFile.Create` is called with data whose `Pattern` is blank.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Blank pattern validation.

##### FileAssertFile_Run_NoMatchingFiles_NoConstraints_NoError

**Scenario**: `FileAssertFile.Run` is called with a pattern that matches no files and no count
constraints are specified.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertFile_Run_WithMatchingFiles_NoConstraints_NoError

**Scenario**: `FileAssertFile.Run` is called with a pattern that matches one or more files and no
constraints are specified.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertFile_Run_TooFewFiles_WritesError

**Scenario**: `FileAssertFile.Run` is called with a `min` constraint but fewer than the required
files match the pattern.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertFile_Run_TooManyFiles_WritesError

**Scenario**: `FileAssertFile.Run` is called with a `max` constraint but more files than allowed
match the pattern.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertFile_Run_WithContentRule_ContentContainsValue_NoError

**Scenario**: `FileAssertFile.Run` is called with a `contains` text rule; the matching file
contains the expected text.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertFile_Run_WithContentRule_ContentMissingValue_WritesError

**Scenario**: `FileAssertFile.Run` is called with a `contains` text rule; the matching file does
not contain the expected text.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertFile_Run_WrongCount_WritesError

**Scenario**: `FileAssertFile.Run` is called with an exact `count` constraint but the actual file
count differs.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertFile_Run_TooSmall_WritesError

**Scenario**: `FileAssertFile.Run` is called with a minimum file size constraint; the matching
file is smaller than the minimum.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertFile_Run_TooLarge_WritesError

**Scenario**: `FileAssertFile.Run` is called with a maximum file size constraint; the matching
file is larger than the maximum.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertFile_Run_MultipleFiles_MultipleViolateSizeConstraints_WritesErrorForEachViolation

**Scenario**: Multiple files match the pattern; more than one violates the size constraints.

**Expected**: A separate error is written for each violation; exit code is non-zero.

##### FileAssertFile_Run_MultipleFiles_MultipleFailContentRule_WritesErrorForEachViolation

**Scenario**: Multiple files match the pattern; more than one fails a content rule.

**Expected**: A separate error is written for each violation; exit code is non-zero.

#### Requirements Coverage

- **File entry creation**: FileAssertFile_Create_ValidData_CreatesFile
- **Null/blank pattern guards**: FileAssertFile_Create_NullData_ThrowsArgumentNullException,
  FileAssertFile_Create_NullPattern_ThrowsInvalidOperationException,
  FileAssertFile_Create_BlankPattern_ThrowsInvalidOperationException
- **Count constraints**: FileAssertFile_Run_TooFewFiles_WritesError,
  FileAssertFile_Run_TooManyFiles_WritesError, FileAssertFile_Run_WrongCount_WritesError
- **Size constraints**: FileAssertFile_Run_TooSmall_WritesError,
  FileAssertFile_Run_TooLarge_WritesError,
  FileAssertFile_Run_MultipleFiles_MultipleViolateSizeConstraints_WritesErrorForEachViolation
- **Content rules**: FileAssertFile_Run_WithContentRule_ContentContainsValue_NoError,
  FileAssertFile_Run_WithContentRule_ContentMissingValue_WritesError,
  FileAssertFile_Run_MultipleFiles_MultipleFailContentRule_WritesErrorForEachViolation
