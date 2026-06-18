## FileSystemGlobbing Verification

This document provides the verification evidence for the FileSystemGlobbing OTS software item.
Requirements for this OTS item are defined in the FileSystemGlobbing OTS Software Requirements
document.

### Required Functionality

Microsoft.Extensions.FileSystemGlobbing provides the glob pattern matcher used by FileAssert to
resolve file-assertion patterns against candidate files. Correct wildcard, recursive-wildcard, and
exact-path matching that drives count constraints is required.

### Verification Approach

FileSystemGlobbing is verified indirectly through FileAssert's own test suite. Each scenario names a
specific FileAssert test that exercises the glob matcher through the file-assertion pipeline and
records its result in a TRX file. A passing CI run for all scenarios constitutes evidence that the
requirement is satisfied.

### Test Environment

The standard `dotnet test` runner on the supported .NET runtimes (net8.0, net9.0, net10.0); no
additional environment setup is required.

### Acceptance Criteria

N/A - Acceptance criteria are managed at the system integration level. This OTS item is considered
verified when the test scenarios that exercise its functionality pass in the CI pipeline.

### Test Scenarios

#### FileAssertFile_Run_WithMatchingFiles_NoConstraints_NoError

**Scenario**: FileAssert resolves a glob pattern with FileSystemGlobbing and finds matching files.

**Expected**: The test passes and the result appears in the TRX output.

#### FileAssertFile_Run_TooFewFiles_WritesError

**Scenario**: FileAssert resolves a glob pattern with FileSystemGlobbing and reports an error when
fewer files match than the minimum constraint allows.

**Expected**: The test passes and the result appears in the TRX output.

#### FileAssertFile_Run_TooManyFiles_WritesError

**Scenario**: FileAssert resolves a glob pattern with FileSystemGlobbing and reports an error when
more files match than the maximum constraint allows.

**Expected**: The test passes and the result appears in the TRX output.

#### IntegrationTest_RecursiveGlob_MatchesFilesAcrossSubdirectories

**Scenario**: FileAssert resolves a recursive glob pattern of the form `**/...` (for example
`**/*.txt`) against a directory tree that contains matching files at multiple nesting levels.

**Expected**: All files at every level are matched and the resulting count satisfies the
configured constraint; the test passes and the result appears in the TRX output.
