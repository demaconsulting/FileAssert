## PdfPig Verification

This document provides the verification evidence for the PdfPig OTS software item. Requirements for
this OTS item are defined in the PdfPig OTS Software Requirements document.

### Required Functionality

PdfPig is the PDF parsing library used by FileAssert to read PDF documents under test for `pdf:`
assertions. Extraction of page counts, document metadata, and page text, and detection of files that
are not valid PDF documents, are required.

### Verification Approach

PdfPig is verified indirectly through FileAssert's own test suite. Each scenario names a specific
FileAssert test that exercises PdfPig through the PDF assertion pipeline and records its result in a
TRX file. A passing CI run for all scenarios constitutes evidence that the requirement is satisfied.

### Test Environment

The standard `dotnet test` runner on the supported .NET runtimes (net8.0, net9.0, net10.0); no
additional environment setup is required.

### Acceptance Criteria

N/A - Acceptance criteria are managed at the system integration level. This OTS item is considered
verified when the test scenarios that exercise its functionality pass in the CI pipeline.

### Test Scenarios

#### FileAssertPdfAssert_Run_ValidPdf_PageCountSatisfied_NoError

**Scenario**: FileAssert parses a valid PDF with PdfPig and asserts a page count that is satisfied.

**Expected**: The test passes and the result appears in the TRX output.

#### FileAssertPdfAssert_Run_MetadataContainsRule_TitleMatches_NoError

**Scenario**: FileAssert reads PDF metadata via PdfPig and asserts that the title contains an
expected value.

**Expected**: The test passes and the result appears in the TRX output.

#### FileAssertPdfAssert_Run_TextRule_ContentMissing_WritesError

**Scenario**: FileAssert extracts page text via PdfPig and reports an error when required content is
missing.

**Expected**: The test passes and the result appears in the TRX output.

#### FileAssertPdfAssert_Run_InvalidFile_WritesError

**Scenario**: FileAssert attempts to parse a file that is not a valid PDF; PdfPig raises a parse
error that FileAssert reports.

**Expected**: The test passes and the result appears in the TRX output.
