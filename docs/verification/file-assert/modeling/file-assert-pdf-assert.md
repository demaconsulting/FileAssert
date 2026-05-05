### FileAssertPdfAssert Verification

This document describes the unit-level verification design for the `FileAssertPdfAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertPdfAssert.cs`.

#### Verification Approach

`FileAssertPdfAssert` is verified with unit tests defined in `FileAssertPdfAssertTests.cs`. Tests
use PDF files in test fixtures and assert on page-count constraints, metadata field assertions,
and text content assertions.

#### Dependencies

| Dependency | Usage in Tests                                              |
|------------|-------------------------------------------------------------|
| `Context`  | Used directly (not mocked) — created with controlled flags. |

#### Test Scenarios

##### FileAssertPdfAssert_Create_ValidData_CreatesPdfAssert

**Scenario**: `FileAssertPdfAssert.Create` is called with valid data.

**Expected**: A non-null `FileAssertPdfAssert` instance is returned.

**Requirement coverage**: PDF assert creation requirement.

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

**Requirement coverage**: Page count constraint pass requirement.

##### FileAssertPdfAssert_Run_ValidPdf_TooFewPages_WritesError

**Scenario**: `FileAssertPdfAssert.Run` is called on a valid PDF with fewer pages than the minimum.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Minimum page count constraint requirement.

##### FileAssertPdfAssert_Run_ValidPdf_TooManyPages_WritesError

**Scenario**: `FileAssertPdfAssert.Run` is called on a valid PDF with more pages than the maximum.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Maximum page count constraint requirement.

##### FileAssertPdfAssert_Run_MetadataContainsRule_FieldMissing_WritesError

**Scenario**: `FileAssertPdfAssert.Run` is called with a metadata `contains` rule on a field that
does not exist in the PDF metadata.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Metadata field missing error requirement.

##### FileAssertPdfAssert_Run_MetadataContainsRule_TitleMatches_NoError

**Scenario**: `FileAssertPdfAssert.Run` is called with a metadata `contains` rule on the Title
field, and the Title contains the expected value.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Metadata title match requirement.

##### FileAssertPdfAssert_Run_MetadataContainsRule_AuthorField_NoError

**Scenario**: `FileAssertPdfAssert.Run` is called with a metadata `contains` rule on the Author
field, and the Author contains the expected value.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Metadata author match requirement.

##### FileAssertPdfAssert_Run_MetadataMatchesRule_Matches_NoError

**Scenario**: `FileAssertPdfAssert.Run` is called with a metadata `matches` regex rule that
matches the field value.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Metadata regex match pass requirement.

##### FileAssertPdfAssert_Run_MetadataMatchesRule_NoMatch_WritesError

**Scenario**: `FileAssertPdfAssert.Run` is called with a metadata `matches` regex rule that does
not match the field value.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Metadata regex match fail requirement.

##### FileAssertPdfAssert_Run_TextContainsRule_ContentPresent_NoError

**Scenario**: `FileAssertPdfAssert.Run` is called with a text `contains` rule and the PDF text
contains the expected value.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: PDF text content assertion pass requirement.

##### FileAssertPdfAssert_Run_TextRule_ContentMissing_WritesError

**Scenario**: `FileAssertPdfAssert.Run` is called with a text rule and the PDF text does not
satisfy the rule.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: PDF text content assertion fail requirement.

##### FileAssertPdfAssert_Run_TextMatchesRule_PatternMatches_NoError

**Scenario**: `FileAssertPdfAssert.Run` is called with a text `matches` regex rule and the PDF
text matches the pattern.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: PDF text regex match requirement.

#### Requirements Coverage

- **PDF assert creation**: FileAssertPdfAssert_Create_ValidData_CreatesPdfAssert
- **Null guard**: FileAssertPdfAssert_Create_NullData_ThrowsArgumentNullException
- **Invalid file**: FileAssertPdfAssert_Run_InvalidFile_WritesError
- **Page count constraints**: FileAssertPdfAssert_Run_ValidPdf_PageCountSatisfied_NoError,
  FileAssertPdfAssert_Run_ValidPdf_TooFewPages_WritesError,
  FileAssertPdfAssert_Run_ValidPdf_TooManyPages_WritesError
- **Metadata assertions**: FileAssertPdfAssert_Run_MetadataContainsRule_FieldMissing_WritesError,
  FileAssertPdfAssert_Run_MetadataContainsRule_TitleMatches_NoError,
  FileAssertPdfAssert_Run_MetadataContainsRule_AuthorField_NoError,
  FileAssertPdfAssert_Run_MetadataMatchesRule_Matches_NoError,
  FileAssertPdfAssert_Run_MetadataMatchesRule_NoMatch_WritesError
- **Text assertions**: FileAssertPdfAssert_Run_TextContainsRule_ContentPresent_NoError,
  FileAssertPdfAssert_Run_TextRule_ContentMissing_WritesError,
  FileAssertPdfAssert_Run_TextMatchesRule_PatternMatches_NoError
