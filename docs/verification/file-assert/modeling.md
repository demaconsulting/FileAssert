## Modeling Subsystem Verification

This document describes the subsystem-level verification design for the `Modeling` subsystem. It
defines the integration test approach, subsystem boundary, mocking strategy, and test scenarios
that together verify the `Modeling` subsystem requirements.

### Verification Approach

The `Modeling` subsystem is verified by integration tests defined in `ModelingTests.cs`. Each test
exercises the assertion execution pipeline — creating a `FileAssertTest`, resolving file patterns,
evaluating constraints, and reporting results through a real `Context`. All asserters and
`FileAssertFile` now accept `IContext` and `IFileContainer`, which are satisfied by the real
`Context` and `DirectoryFileContainer` implementations at the subsystem level.

### Dependencies and Mocking Strategy

All collaborators at the subsystem boundary use their real implementations. Temporary directories
are used for test files so that tests remain isolated.

### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner against
the .NET runtime specified by the build matrix. No special hardware, peripherals, or
environment configuration is required beyond the standard build toolchain.

### Acceptance Criteria

The Modeling subsystem verification passes when all test scenarios listed in
this document execute and pass in the CI pipeline without any test failures, unexpected
exceptions, or assertion errors. Each named scenario must pass on all supported runtime
and platform combinations.

### Test Scenarios

The following integration test scenarios are defined in `ModelingTests.cs`.

#### Modeling_ExecuteChain_PassesWhenAllConstraintsMet

**Scenario**: A `FileAssertTest` is created with a configuration where all file pattern, count,
and content constraints are satisfied by the test files in a temporary directory.

**Expected**: No errors are written to the context; exit code is 0.

#### Modeling_ExecuteChain_ReportsFailuresThroughContext

**Scenario**: A `FileAssertTest` is created with a configuration where at least one constraint
is not satisfied.

**Expected**: Errors are written to the context; exit code is non-zero.

#### Modeling_FileTypeParsing_InvalidXml_ReportsParseError

**Scenario**: A `FileAssertFile` with an XML assertion is configured to evaluate a file that is
not valid XML.

**Expected**: An error is written to the context; exit code is non-zero.

#### Modeling_QueryAssertions_XmlQueryMeetsCount_NoError

**Scenario**: A `FileAssertFile` with an XML XPath assertion is configured and a valid XML file
satisfying the query and count constraints is provided.

**Expected**: No errors are written to the context; exit code is 0.

#### Modeling_ZipEntryContentAssertions_TextContentPassesWhenConstraintsMet

**Scenario**: A `FileAssertTest` is configured with a zip archive pattern and a `zip: files:` block
matching a text entry with a `text: contains:` rule. The zip archive in the temporary directory
contains the entry with content that satisfies the constraint.

**Expected**: No errors are written to the context; exit code is 0.

#### Modeling_ZipEntryContentAssertions_FailureReportsWithBreadcrumbs

**Scenario**: A `FileAssertTest` is configured with a zip archive pattern and a `text: contains:`
rule that the zip entry content does not satisfy. The context is created with a log file to capture
error messages.

**Expected**: An error is written to the context; exit code is non-zero; the error message in the
log file contains both the zip filename and the entry name as breadcrumbs.
