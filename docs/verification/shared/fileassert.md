## FileAssert Verification

This document provides the verification evidence for the FileAssert shared package. Requirements
for this shared package are defined in the FileAssert Shared Package Software Requirements document.

### Required Functionality

DemaConsulting.FileAssert validates HTML and PDF documents produced during the build. The
specific capabilities required are: TRX result file output, file count via glob patterns, text
content assertions, HTML structure assertions, and PDF document content assertions.

### Verification Approach

FileAssert is verified by two complementary layers of evidence. First, the CI pipeline runs
`fileassert --validate --results artifacts/fileassert-self-validation.trx` after all documents
have been generated, exercising FileAssert's built-in self-validation suite and recording
functional test results for ReqStream.

Second, FileAssert is used throughout the pipeline to validate every generated document. If
FileAssert were non-functional, these validation steps would fail, causing `reqstream --enforce`
to report missing test coverage and fail the build. A passing CI build therefore constitutes
transitive evidence that FileAssert correctly asserted document content at each stage of the
pipeline.

### Test Scenarios

#### FileAssert_Results

**Covers**: `FileAssert-Shared-FileAssert-Results`

**Scenario**: FileAssert self-validation exercises the `--results` flag by running a configuration
with one passing test and one deliberately failing test, then verifying that a TRX results file
is written.

**Expected**: Exits non-zero (due to the failing test) and creates a TRX results file at the
specified path.

#### FileAssert_File

**Covers**: `FileAssert-Shared-FileAssert-File`

**Scenario**: FileAssert self-validation exercises file glob matching and count assertions by
matching a glob pattern against a temporary directory containing a known set of files.

**Expected**: Exits 0, confirming that glob-based file count assertions pass.

#### FileAssert_Text

**Covers**: `FileAssert-Shared-FileAssert-Text`

**Scenario**: FileAssert self-validation exercises text content assertions by asserting that a
temporary file contains a known string and does not contain an absent string.

**Expected**: Exits 0, confirming that the `contains` and `does-not-contain` text assertions pass.

#### FileAssert_Html

**Covers**: `FileAssert-Shared-FileAssert-Html`

**Scenario**: FileAssert self-validation exercises HTML structure assertions by parsing a
temporary HTML document with `html:` XPath queries and verifying node counts.

**Expected**: Exits 0, confirming that the HTML document parses successfully and the XPath
queries evaluate against the parsed document model.

#### FileAssert_Pdf

**Covers**: `FileAssert-Shared-FileAssert-Pdf`

**Scenario**: FileAssert self-validation exercises PDF content assertions by reading metadata
fields (Title, Author), checking page count constraints, and asserting PDF body text content
against a temporary PDF document.

**Expected**: Exits 0, confirming that metadata field assertions, page count constraints, and
body text assertions all resolve and pass.

### Acceptance Criteria

N/A - Acceptance criteria are managed at the system integration level. This shared package is
considered verified when the self-validation test scenarios that exercise its functionality
pass in the CI pipeline.
