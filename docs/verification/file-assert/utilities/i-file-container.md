### IFileContainer Verification

This document describes the unit-level verification design for the `IFileContainer` interface.
It defines the test scenarios, dependency usage, and requirement coverage for
`Utilities/IFileContainer.cs` and both of its implementations.

#### Verification Approach

`IFileContainer` is verified through the concrete implementations `DirectoryFileContainer` and
`ZipFileContainer`, whose tests are defined in `IFileContainerTests.cs`. Each test exercises the
full set of interface members against both a real filesystem directory and an in-memory zip archive.
No mocking or test doubles are used — the interface contract is verified by exercising both
implementations.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. The test
collection is marked `[Collection("Sequential")]` to prevent parallel execution of tests that
share `Console` state. No special hardware, peripherals, or environment configuration is required
beyond the standard build toolchain.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `IFileContainer.cs`
meets the project minimum threshold.

#### Dependencies

`DirectoryFileContainer` depends on .NET BCL file-system APIs. `ZipFileContainer` depends on
`System.IO.Compression.ZipArchive`. No mocking is needed at this level.

#### Test Scenarios

##### DirectoryFileContainer_GetEntries_ReturnsAllFilesWithForwardSlashes

**Scenario**: A `DirectoryFileContainer` is created over a small directory tree containing
`a.txt` and `sub/b.txt`.

**Expected**: `GetEntries()` returns exactly 2 entries: `"a.txt"` and `"sub/b.txt"` (forward
slashes regardless of platform).

**Requirement coverage**: IFileContainer uniform access; DirectoryFileContainer file-system access.

##### DirectoryFileContainer_GetEntries_EmptyDirectory_ReturnsEmpty

**Scenario**: A `DirectoryFileContainer` is created over an empty directory.

**Expected**: `GetEntries()` returns an empty list.

**Requirement coverage**: DirectoryFileContainer file-system access (empty directory).

##### DirectoryFileContainer_GetEntries_NonExistentDirectory_ReturnsEmpty

**Scenario**: A `DirectoryFileContainer` is created over a path that does not exist on disk.

**Expected**: `GetEntries()` returns an empty list without throwing.

**Boundary / error path**: Non-existent directory treated as empty container.

**Requirement coverage**: DirectoryFileContainer returns empty for non-existent directories.

##### DirectoryFileContainer_OpenEntry_ExistingFile_ReturnsStream

**Scenario**: A file with known content is written to a temporary directory; `OpenEntry` is
called with its filename.

**Expected**: The returned stream contains the expected content.

**Requirement coverage**: IFileContainer stream opening.

##### DirectoryFileContainer_OpenEntry_NonExistentFile_ThrowsIOException

**Scenario**: `OpenEntry` is called for a filename that does not exist in the container.

**Expected**: A `FileNotFoundException` (subclass of `IOException`) is thrown.

**Boundary / error path**: Missing entry error handling.

**Requirement coverage**: IFileContainer error for missing entries.

##### DirectoryFileContainer_GetEntrySize_ReturnsCorrectSize

**Scenario**: A file containing exactly 5 ASCII bytes is written; `GetEntrySize` is called.

**Expected**: Returns `5L`.

**Requirement coverage**: IFileContainer size reporting.

##### DirectoryFileContainer_GetDisplayPath_RootEntry_ReturnsFullPath

**Scenario**: `GetDisplayPath("report.pdf")` is called on a `DirectoryFileContainer`.

**Expected**: Returns `Path.Combine(basePath, "report.pdf")` — the full file-system path.

**Requirement coverage**: DirectoryFileContainer display path is full file-system path.

##### ZipFileContainer_GetEntries_ReturnsFileEntriesWithForwardSlashes

**Scenario**: A `ZipFileContainer` is created from an in-memory zip containing two entries
`"lib/a.dll"` and `"lib/b.dll"`.

**Expected**: `GetEntries()` returns exactly 2 entries with forward slashes.

**Requirement coverage**: IFileContainer uniform access; ZipFileContainer archive access.

##### ZipFileContainer_OpenEntry_ExistingEntry_ReturnsStream

**Scenario**: A `ZipFileContainer` wraps a zip containing `"readme.txt"` with known content;
`OpenEntry("readme.txt")` is called.

**Expected**: The returned stream contains the expected content.

**Requirement coverage**: IFileContainer stream opening.

##### ZipFileContainer_OpenEntry_NonExistentEntry_ThrowsIOException

**Scenario**: `OpenEntry("missing.txt")` is called on a `ZipFileContainer` wrapping an empty zip.

**Expected**: An `IOException` is thrown.

**Boundary / error path**: Missing zip entry error handling.

**Requirement coverage**: IFileContainer error for missing entries (IOException).

##### ZipFileContainer_GetEntrySize_ReturnsUncompressedLength

**Scenario**: A `ZipFileContainer` wraps a zip containing a 5-byte entry; `GetEntrySize` is called.

**Expected**: Returns `5L` (uncompressed length).

**Requirement coverage**: IFileContainer size reporting for zip entries.

##### ZipFileContainer_GetDisplayPath_ReturnsDisplayNamePrefixedPath

**Scenario**: A `ZipFileContainer` with display name `"outer.zip"` has `GetDisplayPath("inner.txt")`
called.

**Expected**: Returns `"outer.zip > inner.txt"`.

**Requirement coverage**: ZipFileContainer breadcrumb display paths.

#### Requirements Coverage

- (uniform access — enumerate all entries): DirectoryFileContainer_GetEntries_ReturnsAllFilesWithForwardSlashes, ZipFileContainer_GetEntries_ReturnsFileEntriesWithForwardSlashes
- (empty container — empty or non-existent): DirectoryFileContainer_GetEntries_EmptyDirectory_ReturnsEmpty, DirectoryFileContainer_GetEntries_NonExistentDirectory_ReturnsEmpty
- (open entry as stream): DirectoryFileContainer_OpenEntry_ExistingFile_ReturnsStream, ZipFileContainer_OpenEntry_ExistingEntry_ReturnsStream
- (missing entry throws IOException): DirectoryFileContainer_OpenEntry_NonExistentFile_ThrowsIOException, ZipFileContainer_OpenEntry_NonExistentEntry_ThrowsIOException
- (entry size): DirectoryFileContainer_GetEntrySize_ReturnsCorrectSize, ZipFileContainer_GetEntrySize_ReturnsUncompressedLength
- (display path): DirectoryFileContainer_GetDisplayPath_RootEntry_ReturnsFullPath, ZipFileContainer_GetDisplayPath_ReturnsDisplayNamePrefixedPath
