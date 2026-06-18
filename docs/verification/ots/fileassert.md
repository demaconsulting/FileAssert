## FileAssert Verification

This document provides the verification evidence for the FileAssert OTS software item. Requirements
for this OTS item are defined in the FileAssert OTS Software Requirements document.

### Required Functionality

DemaConsulting.FileAssert validates HTML and PDF documents produced during the build, asserting that
each document exists, has a non-trivial size, is structurally valid, and contains expected content.
Self-validation proves the tool itself is operational before ReqStream consumes the results.

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

#### FileAssert_VersionDisplay

**Scenario**: FileAssert self-validation exercises the `--version` flag.

**Expected**: Exits 0 and displays a version string.

#### FileAssert_HelpDisplay

**Scenario**: FileAssert self-validation exercises the `--help` flag.

**Expected**: Exits 0 and displays usage information.

#### FileAssert_Results

**Scenario**: FileAssert self-validation exercises the `--results` flag by running a configuration with one
passing test and one deliberately failing test, then verifying that a TRX results file is written.

**Expected**: Exits non-zero (due to the failing test) and creates a TRX results file at the specified path.

#### FileAssert_Exists

**Scenario**: FileAssert self-validation exercises file-existence checking by matching a glob pattern against
a temporary directory containing a single `.txt` file.

**Expected**: Exits 0, confirming that the glob-based file-existence assertion passes.

#### FileAssert_Contains

**Scenario**: FileAssert self-validation exercises file-content checking by asserting that a temporary `.txt`
file contains a known string.

**Expected**: Exits 0, confirming that the text `contains` assertion passes.

#### FileAssert_StructuralValidity

**Scenario**: FileAssert self-validation exercises structural validity by parsing a temporary
HTML document with `html:` queries and a temporary PDF document with `pdf:` page-count
constraints.

**Expected**: Exits 0, confirming that both documents parse successfully and that the
structured-document queries evaluate against the parsed model.

#### FileAssert_Metadata

**Scenario**: FileAssert self-validation exercises metadata assertions by reading PDF metadata
fields (Title, Author) and matching them against expected values.

**Expected**: Exits 0, confirming that the metadata assertions resolve and pass.

### Acceptance Criteria

N/A - Acceptance criteria are managed at the system integration level. This OTS item is
considered verified when the integration test scenarios that exercise its functionality
pass in the CI pipeline.
