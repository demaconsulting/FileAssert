### DirectoryFileContainer Verification

This document describes the unit-level verification design for the `DirectoryFileContainer` class.
It defines the test scenarios, dependency usage, and requirement coverage for
`Utilities/DirectoryFileContainer.cs`.

#### Verification Approach

`DirectoryFileContainer` is verified with unit tests defined in `IFileContainerTests.cs`. Tests
exercise all four `IFileContainer` interface members against a real filesystem directory created
by `TemporaryDirectory`. No mocking or test doubles are needed because the class interacts only
with the local file system.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. The test
collection is marked `[Collection("Sequential")]` to serialize tests that create and tear down
shared temporary directories. `TemporaryDirectory` creates these under
`Environment.CurrentDirectory` (not `Path.GetTempPath()`) to avoid OS symlink resolution issues
such as `/tmp` resolving to `/private/tmp` on macOS. No special hardware, peripherals, or
environment configuration is required beyond the standard build toolchain.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `DirectoryFileContainer.cs`
meets the project minimum threshold.

#### Dependencies

`DirectoryFileContainer` depends on .NET BCL types (`File`, `Directory`, `Path`, `FileInfo`,
`SearchOption`). No mocking is needed at this level.

#### Test Scenarios

##### DirectoryFileContainer_GetEntries_ReturnsAllFilesWithForwardSlashes

**Scenario**: A directory tree with two files (`a.txt` and `sub/b.txt`) is created; `GetEntries()`
is called.

**Expected**: Returns exactly 2 entries: `"a.txt"` and `"sub/b.txt"` with forward slashes.

##### DirectoryFileContainer_GetEntries_EmptyDirectory_ReturnsEmpty

**Scenario**: An empty directory is created; `GetEntries()` is called.

**Expected**: Returns an empty list.

##### DirectoryFileContainer_GetEntries_NonExistentDirectory_ReturnsEmpty

**Scenario**: A path that does not exist on disk is supplied to the constructor; `GetEntries()`
is called.

**Expected**: Returns an empty list without throwing any exception.

**Boundary / error path**: Non-existent base directory treated as empty container.

##### DirectoryFileContainer_OpenEntry_ExistingFile_ReturnsStream

**Scenario**: A file `"data.txt"` containing `"hello"` is written to a temporary directory;
`OpenEntry("data.txt")` is called.

**Expected**: The returned stream reads `"hello"`.

##### DirectoryFileContainer_OpenEntry_NonExistentFile_ThrowsIOException

**Scenario**: `OpenEntry("missing.txt")` is called on an empty temporary directory.

**Expected**: A `FileNotFoundException` (subclass of `IOException`) is thrown by `File.OpenRead`.

**Boundary / error path**: Missing file throws appropriate exception.

##### DirectoryFileContainer_GetEntrySize_ReturnsCorrectSize

**Scenario**: A file `"size.txt"` containing exactly 5 ASCII bytes is written; `GetEntrySize("size.txt")`
is called.

**Expected**: Returns `5L`.

##### DirectoryFileContainer_GetDisplayPath_RootEntry_ReturnsFullPath

**Scenario**: `GetDisplayPath("report.pdf")` is called on a container with a known base path.

**Expected**: Returns `Path.Combine(basePath, "report.pdf")` — the full absolute file-system path.
