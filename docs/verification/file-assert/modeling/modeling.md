# Modeling Subsystem Verification

This document describes the subsystem-level verification design for the `Modeling` subsystem. It
defines the integration test approach, subsystem boundary, mocking strategy, and test scenarios
that together verify the `Modeling` subsystem requirements.

## Verification Approach

The `Modeling` subsystem is verified by integration tests defined in `ModelingTests.cs`. Each test
exercises the assertion execution pipeline — creating a `FileAssertTest`, resolving file patterns,
evaluating constraints, and reporting results through a real `Context`.

## Dependencies and Mocking Strategy

All collaborators at the subsystem boundary use their real implementations. Temporary directories
are used for test files so that tests remain isolated.

## Integration Test Scenarios

The following integration test scenarios are defined in `ModelingTests.cs`.

### Modeling_ExecuteChain_PassesWhenAllConstraintsMet

**Scenario**: A `FileAssertTest` is created with a configuration where all file pattern, count,
and content constraints are satisfied by the test files in a temporary directory.

**Expected**: No errors are written to the context; exit code is 0.

### Modeling_ExecuteChain_ReportsFailuresThroughContext

**Scenario**: A `FileAssertTest` is created with a configuration where at least one constraint
is not satisfied.

**Expected**: Errors are written to the context; exit code is non-zero.

### Modeling_FileTypeParsing_InvalidXml_ReportsParseError

**Scenario**: A `FileAssertFile` with an XML assertion is configured to evaluate a file that is
not valid XML.

**Expected**: An error is written to the context; exit code is non-zero.

### Modeling_QueryAssertions_XmlQueryMeetsCount_NoError

**Scenario**: A `FileAssertFile` with an XML XPath assertion is configured and a valid XML file
satisfying the query and count constraints is provided.

**Expected**: No errors are written to the context; exit code is 0.

## Requirements Coverage

- **Constraint evaluation**: Modeling_ExecuteChain_PassesWhenAllConstraintsMet,
  Modeling_ExecuteChain_ReportsFailuresThroughContext
- **XML parsing error reporting**: Modeling_FileTypeParsing_InvalidXml_ReportsParseError
- **XML query assertion**: Modeling_QueryAssertions_XmlQueryMeetsCount_NoError
