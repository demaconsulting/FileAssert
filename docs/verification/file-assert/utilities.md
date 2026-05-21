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

### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner against
the .NET runtime specified by the build matrix. No special hardware, peripherals, or
environment configuration is required beyond the standard build toolchain.

### Acceptance Criteria

The Utilities subsystem verification passes when all test scenarios listed in
this document execute and pass in the CI pipeline without any test failures, unexpected
exceptions, or assertion errors. Each named scenario must pass on all supported runtime
and platform combinations.

### Integration Test Scenarios

The following integration test scenarios are defined in `UtilitiesTests.cs`.

#### Utilities_SafePathCombine_PreventsPathTraversalToFileSystem

**Scenario**: A path traversal pattern (e.g., `../`) is passed as the relative path argument to
`PathHelpers.SafePathCombine` with a real temporary directory as the base path.

**Expected**: An `ArgumentException` is thrown; no traversal of the file system occurs.

### Requirements Coverage

- **Path traversal prevention**: Utilities_SafePathCombine_PreventsPathTraversalToFileSystem
- **Temporary directory isolation and cleanup**: Utilities_TemporaryDirectory_IsolatesAndCleansUpScratchSpace

### TemporaryDirectory Verification

The `TemporaryDirectory` unit is verified by the unit tests defined in `TemporaryDirectoryTests.cs`.
Each test exercises construction, path resolution, and disposal against the real file system.

#### TemporaryDirectory Test Scenarios

- **TemporaryDirectory_Constructor_CreatesDirectory** – confirms the directory exists on disk
  immediately after construction.
- **TemporaryDirectory_Constructor_CreatesUniqueDirectories** – confirms two instances produce
  distinct directory paths.
- **TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory** – confirms that a simple
  relative filename resolves to a path under the temporary directory.
- **TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories** – confirms that
  intermediate subdirectories are created automatically for nested relative paths.
- **TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException** – confirms that a
  path-traversal attempt (e.g., `"../escaped.txt"`) is rejected with `ArgumentException`.
- **TemporaryDirectory_Dispose_DeletesDirectory** – confirms the directory and its contents are
  deleted when the instance is disposed.
- **TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow** – confirms that disposal does not
  throw when the directory has already been removed externally.
