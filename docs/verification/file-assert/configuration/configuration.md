# Configuration Subsystem Verification

This document describes the subsystem-level verification design for the `Configuration` subsystem.
It defines the integration test approach, subsystem boundary, mocking strategy, and test scenarios
that together verify the `Configuration` subsystem requirements.

## Verification Approach

The `Configuration` subsystem is verified by integration tests defined in `ConfigurationTests.cs`.
Each test exercises the full loading and execution pipeline — reading a YAML file, constructing
the test hierarchy, and running the resulting configuration — with a real `Context`.

## Dependencies and Mocking Strategy

All collaborators at the subsystem boundary (`Context`, `FileAssertConfig`, `PathHelpers`) use
their real implementations. Temporary directories are used for configuration files and test
artifacts so that tests remain isolated and leave no permanent file-system side-effects.

## Integration Test Scenarios

The following integration test scenarios are defined in `ConfigurationTests.cs`.

### Configuration_LoadYaml_BuildsCompleteTestHierarchy

**Scenario**: A YAML configuration file with nested test, file, and rule entries is loaded using
`FileAssertConfig.ReadFromFile`.

**Expected**: The complete object hierarchy (tests → files → rules) is correctly constructed with
all properties populated.

### Configuration_RunWithFilter_ExecutesOnlyMatchingTests

**Scenario**: A configuration with two tests is loaded. Only one file exists; a filter naming one
test is passed to `FileAssertConfig.Run`.

**Expected**: Only the named test runs; exit code is 0.

### Configuration_RunWithTagFilter_ExecutesOnlyMatchingTests

**Scenario**: A configuration with two tests with different tags is loaded. Only one file exists;
a filter naming one tag is passed to `FileAssertConfig.Run`.

**Expected**: Only the test matching the tag runs; exit code is 0.

## Requirements Coverage

- **YAML loading and hierarchy construction**: Configuration_LoadYaml_BuildsCompleteTestHierarchy
- **Test name filtering**: Configuration_RunWithFilter_ExecutesOnlyMatchingTests
- **Tag filtering**: Configuration_RunWithTagFilter_ExecutesOnlyMatchingTests
