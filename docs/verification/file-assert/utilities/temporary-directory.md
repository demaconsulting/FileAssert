### TemporaryDirectory Verification

This document describes the unit-level verification design for the `TemporaryDirectory` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Utilities/TemporaryDirectory.cs`.

#### Verification Approach

`TemporaryDirectory` is verified with unit tests defined in `TemporaryDirectoryTests.cs`. Tests
exercise construction, path resolution, and disposal against the real file system rooted under
`Environment.CurrentDirectory`. No mocking or test doubles are needed because the class interacts
only with the local file system and delegates traversal protection to `PathHelpers.SafePathCombine`.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. The test
collection is marked `[Collection("Sequential")]` to prevent parallel execution of tests that
share `Console` state. No special hardware, peripherals, or environment configuration is required
beyond the standard build toolchain.

#### Acceptance Criteria

N/A – Acceptance criteria are managed at the subsystem and system integration levels.
Unit tests provide fine-grained coverage evidence; formal acceptance is declared at the
subsystem level when all unit tests supporting a subsystem requirement pass.

#### Dependencies

`TemporaryDirectory` depends on `PathHelpers.SafePathCombine` for traversal-safe path
construction and on .NET BCL types (`Directory`, `Path`, `Environment`) for file system
operations. No mocking is needed at this level.

#### Test Scenarios

##### TemporaryDirectory_Constructor_CreatesDirectory

**Scenario**: A `TemporaryDirectory` instance is constructed inside a `using` block.

**Expected**: `Directory.Exists(tmpDir.DirectoryPath)` returns `true` immediately after
construction.

**Requirement coverage**: Lifecycle creation requirement.

##### TemporaryDirectory_Constructor_CreatesUniqueDirectories

**Scenario**: Two `TemporaryDirectory` instances are constructed sequentially without disposal
between them.

**Expected**: The two `DirectoryPath` values are not equal.

**Boundary / error path**: Ensures uniqueness under rapid successive construction.

**Requirement coverage**: Lifecycle uniqueness requirement.

##### TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory

**Scenario**: `GetFilePath("output.md")` is called on a live `TemporaryDirectory` instance.

**Expected**: The returned path starts with `tmpDir.DirectoryPath` and ends with `"output.md"`.
No exception is thrown.

**Requirement coverage**: Safe path construction requirement.

##### TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories

**Scenario**: `GetFilePath(Path.Combine("sub", "nested", "output.md"))` is called on a live
`TemporaryDirectory` instance.

**Expected**: `Directory.Exists` on the parent directory of the returned path returns `true`,
confirming that intermediate subdirectories were created automatically. No exception is thrown.

**Requirement coverage**: Intermediate subdirectory creation requirement.

##### TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException

**Scenario**: `GetFilePath("../escaped.txt")` is called on a live `TemporaryDirectory` instance.

**Expected**: An `ArgumentException` is thrown; no file is created outside the temporary
directory.

**Boundary / error path**: Path-traversal attempt using leading `../`.

**Requirement coverage**: Traversal rejection requirement.

##### TemporaryDirectory_Dispose_DeletesDirectory

**Scenario**: A `TemporaryDirectory` is constructed, a file is written inside it via
`GetFilePath`, and then the instance is disposed by exiting a `using` block.

**Expected**: `Directory.Exists` on the captured `DirectoryPath` returns `false` after disposal,
confirming that the directory and its contents were deleted.

**Requirement coverage**: Lifecycle cleanup requirement.

##### TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow

**Scenario**: The underlying directory is manually deleted before `Dispose()` is called on the
`TemporaryDirectory` instance.

**Expected**: `Dispose()` completes without throwing any exception.

**Boundary / error path**: Cleanup error suppression when the directory no longer exists.

**Requirement coverage**: Resilient disposal requirement.

#### Requirements Coverage

- (directory created on construction): TemporaryDirectory_Constructor_CreatesDirectory
- (unique directory per instance): TemporaryDirectory_Constructor_CreatesUniqueDirectories
- (directory deleted on disposal): TemporaryDirectory_Dispose_DeletesDirectory
- (disposal safe when already deleted): TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow
- (simple file path under directory): TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory
- (nested path creates subdirectories): TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories
- (traversal attempt rejected): TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException
