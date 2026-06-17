### FileAssertPdfAssert Verification

This document describes the unit-level verification design for the `FileAssertPdfAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertPdfAssert.cs`.

#### Verification Approach

`FileAssertPdfAssert` is verified with unit tests defined in `FileAssertPdfAssertTests.cs`. Tests
use PDF files in test fixtures and assert on page-count constraints, metadata field assertions,
and text content assertions.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `FileAssertPdfAssert.cs`
meets the project minimum threshold.

#### Dependencies

| Dependency | Usage in Tests                                              |
|------------|-------------------------------------------------------------|
| `Context`  | Used directly (not mocked) — created with controlled flags. |

#### Test Scenarios

##### FileAssertPdfAssert_Create_ValidData_CreatesPdfAssert

**Scenario**: `FileAssertPdfAssert.Create` is called with valid data.

**Expected**: A non-null `FileAssertPdfAssert` instance is returned.

##### FileAssertPdfAssert_Create_NullData_ThrowsArgumentNullException

**Scenario**: `FileAssertPdfAssert.Create` is called with `null` data.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null data guard.

##### FileAssertPdfAssert_Run_InvalidFile_WritesError

**Scenario**: `FileAssertPdfAssert.Run` is called with a path that is not a valid PDF file.

**Expected**: An error is written to the context; exit code is non-zero.

**Boundary / error path**: Invalid PDF file error path.

##### FileAssertPdfAssert_Run_ValidPdf_PageCountSatisfied_NoError

**Scenario**: `FileAssertPdfAssert.Run` is called on a valid PDF that meets the page count
constraints.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertPdfAssert_Run_ValidPdf_TooFewPages_WritesError

**Scenario**: `FileAssertPdfAssert.Run` is called on a valid PDF with fewer pages than the minimum.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertPdfAssert_Run_ValidPdf_TooManyPages_WritesError

**Scenario**: `FileAssertPdfAssert.Run` is called on a valid PDF with more pages than the maximum.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertPdfAssert_Run_MetadataContainsRule_FieldMissing_WritesError

**Scenario**: `FileAssertPdfAssert.Run` is called with a metadata `contains` rule on a field that
does not exist in the PDF metadata.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertPdfAssert_Run_MetadataContainsRule_TitleMatches_NoError

**Scenario**: `FileAssertPdfAssert.Run` is called with a metadata `contains` rule on the Title
field, and the Title contains the expected value.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertPdfAssert_Run_MetadataContainsRule_AuthorField_NoError

**Scenario**: `FileAssertPdfAssert.Run` is called with a metadata `contains` rule on the Author
field, and the Author contains the expected value.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertPdfAssert_Run_MetadataMatchesRule_Matches_NoError

**Scenario**: `FileAssertPdfAssert.Run` is called with a metadata `matches` regex rule that
matches the field value.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertPdfAssert_Run_MetadataMatchesRule_NoMatch_WritesError

**Scenario**: `FileAssertPdfAssert.Run` is called with a metadata `matches` regex rule that does
not match the field value.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertPdfAssert_Run_TextContainsRule_ContentPresent_NoError

**Scenario**: `FileAssertPdfAssert.Run` is called with a text `contains` rule and the PDF text
contains the expected value.

**Expected**: No errors are written to the context; exit code is 0.

##### FileAssertPdfAssert_Run_TextRule_ContentMissing_WritesError

**Scenario**: `FileAssertPdfAssert.Run` is called with a text rule and the PDF text does not
satisfy the rule.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertPdfAssert_Run_TextMatchesRule_PatternMatches_NoError

**Scenario**: `FileAssertPdfAssert.Run` is called with a text `matches` regex rule and the PDF
text matches the pattern.

**Expected**: No errors are written to the context; exit code is 0.
