## Utilities Subsystem Verification

This document describes the subsystem-level verification design for the `Utilities` subsystem. It
defines the integration test approach, subsystem boundary, mocking strategy, and test scenarios
that together verify the `Utilities` subsystem requirements.

### Verification Approach

The `Utilities` subsystem is verified by integration tests defined in `UtilitiesTests.cs`. Each
test exercises the `PathHelpers` unit through realistic path-combination workflows, confirming that
valid paths are resolved correctly and traversal attacks are rejected.

### Dependencies and Mocking Strategy

`PathHelpers` depends only on .NET BCL types for path manipulation. No external dependencies
require mocking at the subsystem level.

### Integration Test Scenarios

The following integration test scenarios are defined in `UtilitiesTests.cs`.

#### Utilities_SafePathCombine_PreventsPathTraversalToFileSystem

**Scenario**: A path traversal pattern (e.g., `../`) is passed as the relative path argument to
`PathHelpers.SafePathCombine` with a real temporary directory as the base path.

**Expected**: An `ArgumentException` is thrown; no traversal of the file system occurs.

### Requirements Coverage

- **Path traversal prevention**: Utilities_SafePathCombine_PreventsPathTraversalToFileSystem
