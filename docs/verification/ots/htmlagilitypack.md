## HtmlAgilityPack Verification

This document provides the verification evidence for the HtmlAgilityPack OTS software item.
Requirements for this OTS item are defined in the HtmlAgilityPack OTS Software Requirements document.

### Required Functionality

HtmlAgilityPack is the HTML parser used by FileAssert to read HTML documents under test for `html:`
XPath assertions. Lenient parsing of syntactically imperfect markup and evaluation of XPath
node-count and text queries are required.

### Verification Approach

HtmlAgilityPack is verified indirectly through FileAssert's own test suite. Each scenario names a
specific FileAssert test that exercises HtmlAgilityPack through the HTML assertion pipeline and
records its result in a TRX file. A passing CI run for all scenarios constitutes evidence that the
requirement is satisfied.

### Test Environment

The standard `dotnet test` runner on the supported .NET runtimes (net8.0, net9.0, net10.0); no
additional environment setup is required.

### Acceptance Criteria

N/A – Acceptance criteria are managed at the system integration level. This OTS item is considered
verified when the test scenarios that exercise its functionality pass in the CI pipeline.

### Test Scenarios

#### FileAssertHtmlAssert_Run_ExactCount_Matches_NoError

**Scenario**: FileAssert parses an HTML document with HtmlAgilityPack and asserts an exact XPath
node count that matches.

**Expected**: The test passes and the result appears in the TRX output.

**Requirement coverage**: `FileAssert-OTS-HtmlAgilityPack`.

#### FileAssertHtmlAssert_Run_MinMaxCount_WithinBounds_NoError

**Scenario**: FileAssert parses an HTML document with HtmlAgilityPack and asserts an XPath node
count within min/max bounds.

**Expected**: The test passes and the result appears in the TRX output.

**Requirement coverage**: `FileAssert-OTS-HtmlAgilityPack`.

#### FileAssertHtmlAssert_Run_XPathContainsText_Matches_NoError

**Scenario**: FileAssert evaluates an XPath text query via HtmlAgilityPack and confirms the matched
node contains the expected text.

**Expected**: The test passes and the result appears in the TRX output.

**Requirement coverage**: `FileAssert-OTS-HtmlAgilityPack`.

#### FileAssertHtmlAssert_Run_NonExistentFile_WritesError

**Scenario**: FileAssert attempts to read a missing HTML file; the resulting IO error is reported.

**Expected**: The test passes and the result appears in the TRX output.

**Requirement coverage**: `FileAssert-OTS-HtmlAgilityPack`.
