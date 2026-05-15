### FileAssertZipAssert Verification

This document describes the unit-level verification design for the `FileAssertZipAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertZipAssert.cs`.

#### Verification Approach

`FileAssertZipAssert` is verified with unit tests defined in `FileAssertZipAssertTests.cs`. Tests
create actual zip archives in a temporary file using `System.IO.Compression.ZipFile`, then invoke
`FileAssertZipAssert.Run` and assert on the resulting context state.

#### Dependencies

| Dependency              | Usage in Tests                                              |
|-------------------------|-------------------------------------------------------------|
| `Context`               | Used directly (not mocked) — created with controlled flags. |
| `System.IO.Compression` | Used directly to create real zip archives for each test.    |

#### Test Scenarios

##### FileAssertZipAssert_Create_ValidData_CreatesZipAssert

**Scenario**: `FileAssertZipAssert.Create` is called with a valid `FileAssertZipData` containing
one entry.

**Expected**: A non-null instance is returned with the correct pattern, min, and max values.

**Requirement coverage**: Zip assert creation requirement.

##### FileAssertZipAssert_Create_NullData_ThrowsArgumentNullException

**Scenario**: `FileAssertZipAssert.Create` is called with `null` data.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null data guard.

##### FileAssertZipAssert_Create_EntryMissingPattern_ThrowsInvalidOperationException

**Scenario**: `FileAssertZipAssert.Create` is called with an entry that has no pattern.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Missing pattern guard.

##### FileAssertZipAssert_Run_MatchingEntriesMeetConstraints_NoError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive containing an entry that matches
the pattern, with both min and max set to 1.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Entry matching pass requirement.

##### FileAssertZipAssert_Run_GlobPatternMatchesMultipleEntries_NoError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive containing multiple entries that
match a wildcard glob pattern, with only a minimum count specified.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Glob matching across multiple entries.

##### FileAssertZipAssert_Run_TooFewMatchingEntries_WritesError

**Scenario**: `FileAssertZipAssert.Run` is called on an empty zip archive where the minimum count
constraint requires at least one matching entry.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Minimum count violation reporting.

##### FileAssertZipAssert_Run_TooManyMatchingEntries_WritesError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive with two matching entries where
the maximum count is set to 1.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Maximum count violation reporting.

##### FileAssertZipAssert_Run_InvalidZipFile_WritesError

**Scenario**: `FileAssertZipAssert.Run` is called on a file that contains arbitrary bytes and
cannot be parsed as a zip archive.

**Expected**: A single error is written to the context; exit code is non-zero.

**Boundary / error path**: Invalid zip data parse error.

##### FileAssertZipAssert_Run_NonExistentFile_WritesError

**Scenario**: `FileAssertZipAssert.Run` is called with a path that does not exist.

**Expected**: A single error is written to the context; exit code is non-zero.

**Boundary / error path**: Missing file I/O error.

#### Requirements Coverage

- **Zip assert creation**: FileAssertZipAssert_Create_ValidData_CreatesZipAssert
- **Null guard**: FileAssertZipAssert_Create_NullData_ThrowsArgumentNullException
- **Missing pattern guard**: FileAssertZipAssert_Create_EntryMissingPattern_ThrowsInvalidOperationException
- **Entry matching pass**: FileAssertZipAssert_Run_MatchingEntriesMeetConstraints_NoError,
  FileAssertZipAssert_Run_GlobPatternMatchesMultipleEntries_NoError
- **Too few entries**: FileAssertZipAssert_Run_TooFewMatchingEntries_WritesError
- **Too many entries**: FileAssertZipAssert_Run_TooManyMatchingEntries_WritesError
- **Invalid zip**: FileAssertZipAssert_Run_InvalidZipFile_WritesError
- **Missing file**: FileAssertZipAssert_Run_NonExistentFile_WritesError
