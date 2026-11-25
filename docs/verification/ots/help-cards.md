## HelpCards Verification

This document provides the verification evidence for the HelpCards OTS software item. Requirements
for this OTS item are defined in the HelpCards OTS Software Requirements document.

### Required Functionality

HelpCards is the library that serves the built-in help of FileAssert. Answering the `help` command
from the cards embedded in the tool assembly, and reporting card defects and the results of the
command examples to the tests of the tool, are required.

### Verification Approach

HelpCards is a runtime library with no self-validation mode, so it is verified by test evidence
from FileAssert's own test suite, which uses the real package and the cards embedded in the tool
assembly. Each scenario names a specific FileAssert test and records its result in a TRX file.

### Test Environment

The standard `dotnet test` runner on the supported .NET runtimes (net10.0, net11.0); no
additional environment setup is required.

### Acceptance Criteria

N/A - Acceptance criteria are managed at the system integration level. This OTS item is considered
verified when the test scenarios that exercise its functionality pass in the CI pipeline.

### Test Scenarios

#### HelpCommand_TryRun_HelpVerb_PrintsTopicIndexAndReturnsZero

**Scenario**: The help verb is answered in-process from the embedded cards.

**Expected**: The test passes and the result appears in the TRX output.

#### IntegrationTest_HelpTopics_HelpVerb_ListsTopics

**Scenario**: The help verb is answered by the built tool from the embedded cards.

**Expected**: The test passes and the result appears in the TRX output.

#### HelpCommand_CreateOptions_ShippedCatalog_PassesValidation

**Scenario**: The validator of HelpCards accepts the shipped cards with warnings treated as errors.

**Expected**: The test passes and the result appears in the TRX output.

#### IntegrationTest_HelpExamples_AllCommandExamples_GiveStatedResults

**Scenario**: The example runner of HelpCards runs every command example against the built tool
and compares the results with the cards.

**Expected**: The test passes and the result appears in the TRX output.
