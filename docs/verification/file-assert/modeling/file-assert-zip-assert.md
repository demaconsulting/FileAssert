### FileAssertZipAssert Verification

This document describes the unit-level verification design for the `FileAssertZipAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertZipAssert.cs`.

#### Verification Approach

`FileAssertZipAssert` is verified with unit tests defined in `FileAssertZipAssertTests.cs`. Tests
create actual zip archives in a temporary file using `System.IO.Compression.ZipFile`, then invoke
`FileAssertZipAssert.Run` and assert on the resulting context state.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `FileAssertZipAssert.cs`
meets the project minimum threshold.

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

##### FileAssertZipAssert_Run_EntryContainsRequiredText_NoError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry contains the
text required by a `text: contains:` rule.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Text content assertion pass path through a zip entry.

##### FileAssertZipAssert_Run_EntryMissingRequiredText_WritesError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry does NOT
contain the text required by a `text: contains:` rule.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Text content assertion fail path through a zip entry.

##### FileAssertZipAssert_Run_EntryXmlMatchesXPath_NoError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry contains
XML that satisfies the XPath count constraint.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: XML XPath assertion pass path through a zip entry.

##### FileAssertZipAssert_Run_EntryXmlFailsXPath_WritesError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry contains
XML that does not satisfy the XPath count constraint.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: XML XPath assertion fail path through a zip entry.

##### FileAssertZipAssert_Run_EntryYamlMatchesQuery_NoError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry contains
YAML that satisfies the dot-notation query count constraint.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: YAML query assertion pass path through a zip entry.

##### FileAssertZipAssert_Run_EntryYamlFailsQuery_WritesError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry contains
YAML that does not satisfy the dot-notation query count constraint.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: YAML query assertion fail path through a zip entry.

##### FileAssertZipAssert_Run_EntryJsonMatchesQuery_NoError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry contains
JSON that satisfies the dot-notation query count constraint.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: JSON query assertion pass path through a zip entry.

##### FileAssertZipAssert_Run_EntryJsonFailsQuery_WritesError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry contains
JSON that does not satisfy the dot-notation query count constraint.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: JSON query assertion fail path through a zip entry.

##### FileAssertZipAssert_Run_EntryMeetsMinSizeConstraint_NoError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry has an
uncompressed size that satisfies the `min-size` constraint.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Minimum-size constraint pass path through a zip entry.

##### FileAssertZipAssert_Run_EntryBelowMinSizeConstraint_WritesError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry has an
uncompressed size below the `min-size` constraint.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Minimum-size constraint fail path through a zip entry.

##### FileAssertZipAssert_Run_EntryMeetsMaxSizeConstraint_NoError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry has an
uncompressed size within the `max-size` constraint.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Maximum-size constraint pass path through a zip entry.

##### FileAssertZipAssert_Run_EntryExceedsMaxSizeConstraint_WritesError

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive whose matching entry has an
uncompressed size that exceeds the `max-size` constraint.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Maximum-size constraint fail path through a zip entry.

##### FileAssertZipAssert_Run_NestedZipTextContent_InnerEntryContentMatches_NoError

**Scenario**: `FileAssertZipAssert.Run` is called on an outer zip that contains an inner zip,
which in turn contains a text file. The assertion chain traverses both archives and evaluates
a `text: contains:` rule on the innermost entry.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Nested zip-in-zip content assertion pass path.

##### FileAssertZipAssert_Run_ContentAssertionFails_ErrorContainsBreadcrumbs

**Scenario**: `FileAssertZipAssert.Run` is called on a zip archive and the text content assertion
on a matching entry fails. A `CapturingContext` collects the error message.

**Expected**: The captured error message contains both the zip file name and the failing entry path
as breadcrumb segments.

**Requirement coverage**: Scoped context breadcrumb propagation for inner assertion failures.

#### End-to-End Test Scenarios

##### IntegrationTest_ZipAssert_TextAssertionPassing_ReturnsZero

**Scenario**: End-to-end run via `dotnet` with a YAML config declaring a `text: contains:` rule
on a zip entry whose content satisfies the rule.

**Expected**: Exit code is 0.

**Requirement coverage**: Text content assertion pass path through a zip (end-to-end).

##### IntegrationTest_ZipAssert_TextAssertionFailing_ReturnsNonZero

**Scenario**: End-to-end run via `dotnet` with a YAML config declaring a `text: contains:` rule
on a zip entry whose content does not satisfy the rule.

**Expected**: Exit code is non-zero.

**Requirement coverage**: Text content assertion fail path through a zip (end-to-end).

##### IntegrationTest_ZipAssert_XmlAssertionPassing_ReturnsZero

**Scenario**: End-to-end run via `dotnet` with a YAML config declaring an `xml: query:` assertion
on a zip entry whose XML content satisfies the count constraint.

**Expected**: Exit code is 0.

**Requirement coverage**: XML assertion pass path through a zip (end-to-end).

##### IntegrationTest_ZipAssert_NestedZipTextContent_ReturnsZero

**Scenario**: End-to-end run via `dotnet` with a YAML config that asserts on a text entry inside
an inner zip that is itself an entry in an outer zip.

**Expected**: Exit code is 0.

**Requirement coverage**: Nested zip-in-zip content assertion pass path (end-to-end).

##### IntegrationTest_ZipAssert_FailingContentAssertion_ErrorContainsEntryPath

**Scenario**: End-to-end run via `dotnet` (without `--silent`) with a YAML config that declares a
failing `text: contains:` rule on a zip entry. The combined stdout/stderr output is captured.

**Expected**: Exit code is non-zero and the output contains both the zip filename and the entry
name as breadcrumbs.

**Requirement coverage**: Scoped context breadcrumb propagation (end-to-end).

#### Requirements Coverage

- **Zip assert creation**: FileAssertZipAssert_Create_ValidData_CreatesZipAssert
- **Null guard**: FileAssertZipAssert_Create_NullData_ThrowsArgumentNullException
- **Missing pattern guard**: FileAssertZipAssert_Create_EntryMissingPattern_ThrowsInvalidOperationException
- **Entry matching pass**: FileAssertZipAssert_Run_MatchingEntriesMeetConstraints_NoError,
  FileAssertZipAssert_Run_GlobPatternMatchesMultipleEntries_NoError
- **Too few entries**: FileAssertZipAssert_Run_TooFewMatchingEntries_WritesError
- **Too many entries**: FileAssertZipAssert_Run_TooManyMatchingEntries_WritesError
- **Text content pass**: FileAssertZipAssert_Run_EntryContainsRequiredText_NoError,
  IntegrationTest_ZipAssert_TextAssertionPassing_ReturnsZero
- **Text content fail**: FileAssertZipAssert_Run_EntryMissingRequiredText_WritesError,
  IntegrationTest_ZipAssert_TextAssertionFailing_ReturnsNonZero
- **XML XPath pass**: FileAssertZipAssert_Run_EntryXmlMatchesXPath_NoError,
  IntegrationTest_ZipAssert_XmlAssertionPassing_ReturnsZero
- **XML XPath fail**: FileAssertZipAssert_Run_EntryXmlFailsXPath_WritesError
- **YAML query pass**: FileAssertZipAssert_Run_EntryYamlMatchesQuery_NoError
- **YAML query fail**: FileAssertZipAssert_Run_EntryYamlFailsQuery_WritesError
- **JSON query pass**: FileAssertZipAssert_Run_EntryJsonMatchesQuery_NoError
- **JSON query fail**: FileAssertZipAssert_Run_EntryJsonFailsQuery_WritesError
- **Min-size pass**: FileAssertZipAssert_Run_EntryMeetsMinSizeConstraint_NoError
- **Min-size fail**: FileAssertZipAssert_Run_EntryBelowMinSizeConstraint_WritesError
- **Max-size pass**: FileAssertZipAssert_Run_EntryMeetsMaxSizeConstraint_NoError
- **Max-size fail**: FileAssertZipAssert_Run_EntryExceedsMaxSizeConstraint_WritesError
- **Nested zip pass**: FileAssertZipAssert_Run_NestedZipTextContent_InnerEntryContentMatches_NoError,
  IntegrationTest_ZipAssert_NestedZipTextContent_ReturnsZero
- **Breadcrumb propagation**: FileAssertZipAssert_Run_ContentAssertionFails_ErrorContainsBreadcrumbs,
  IntegrationTest_ZipAssert_FailingContentAssertion_ErrorContainsEntryPath
- **Invalid zip**: FileAssertZipAssert_Run_InvalidZipFile_WritesError
- **Missing file**: FileAssertZipAssert_Run_NonExistentFile_WritesError
