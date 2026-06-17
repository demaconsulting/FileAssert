## YamlDotNet Verification

This document provides the verification evidence for the YamlDotNet OTS software item. Requirements
for this OTS item are defined in the YamlDotNet OTS Software Requirements document.

### Required Functionality

YamlDotNet is the YAML parser used by FileAssert to deserialize the `.fileassert.yaml` configuration
and to parse YAML documents under test for `yaml:` dot-notation path assertions. Correct scalar and
sequence handling and detection of malformed documents are required.

### Verification Approach

YamlDotNet is verified indirectly through FileAssert's own test suite. Each scenario names a specific
FileAssert test that exercises YamlDotNet through the YAML assertion pipeline and records its result
in a TRX file. A passing CI run for all scenarios constitutes evidence that the requirement is
satisfied.

### Test Environment

The standard `dotnet test` runner on the supported .NET runtimes (net8.0, net9.0, net10.0); no
additional environment setup is required.

### Acceptance Criteria

N/A - Acceptance criteria are managed at the system integration level. This OTS item is considered
verified when the test scenarios that exercise its functionality pass in the CI pipeline.

### Test Scenarios

#### FileAssertYamlAssert_Run_SequenceCount_Matches_NoError

**Scenario**: FileAssert parses a YAML document with YamlDotNet and asserts a sequence's element
count, which matches the constraint.

**Expected**: The test passes and the result appears in the TRX output.

#### FileAssertYamlAssert_Run_ScalarValue_CountsAsOne_NoError

**Scenario**: FileAssert parses a YAML document with YamlDotNet and treats a scalar value at a path
as a single match.

**Expected**: The test passes and the result appears in the TRX output.

#### FileAssertYamlAssert_Run_MinMaxCount_WithinBounds_NoError

**Scenario**: FileAssert parses a YAML document with YamlDotNet and asserts a match count within
min/max bounds.

**Expected**: The test passes and the result appears in the TRX output.

#### FileAssertYamlAssert_Run_InvalidFile_WritesError

**Scenario**: FileAssert attempts to parse a malformed YAML document; YamlDotNet raises a parse
error that FileAssert reports.

**Expected**: The test passes and the result appears in the TRX output.
