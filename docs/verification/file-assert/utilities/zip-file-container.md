### ZipFileContainer Verification

This document describes the unit-level verification design for the `ZipFileContainer` class.
It defines the test scenarios, dependency usage, and requirement coverage for
`Utilities/ZipFileContainer.cs`.

#### Verification Approach

`ZipFileContainer` is verified with unit tests defined in `IFileContainerTests.cs`. Tests
exercise all four `IFileContainer` interface members against in-memory zip archives constructed
with `System.IO.Compression.ZipArchive` using `MemoryStream`. No mocking or test doubles are
needed because the class interacts only with the in-memory `ZipArchive` API.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. The test
collection is marked `[Collection("Sequential")]` to prevent parallel execution of tests that
share `Console` state. No special hardware, peripherals, or environment configuration is required
beyond the standard build toolchain.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `ZipFileContainer.cs`
meets the project minimum threshold.

#### Dependencies

`ZipFileContainer` depends on `System.IO.Compression.ZipArchive` and `ZipArchiveEntry`. No
mocking is needed at this level.

#### Test Scenarios

##### ZipFileContainer_GetEntries_ReturnsFileEntriesWithForwardSlashes

**Scenario**: A `ZipFileContainer` is created from an in-memory zip containing two entries
`"lib/a.dll"` and `"lib/b.dll"`.

**Expected**: `GetEntries()` returns exactly 2 entries with forward slashes; directory marker
entries (names ending in `/`) are excluded.

**Requirement coverage**: Zip archive access — file entry enumeration with forward slashes.

##### ZipFileContainer_GetEntries_ExcludesDirectoryMarkers

**Scenario**: A `ZipFileContainer` is created from an in-memory zip containing a directory
marker entry `"lib/"` and a file entry `"lib/a.dll"`.

**Expected**: `GetEntries()` returns only `"lib/a.dll"`; the directory marker entry (name ending
in `/`) is excluded.

**Boundary / error path**: Directory marker exclusion during entry enumeration.

**Requirement coverage**: Entry enumeration — directory markers excluded.

##### ZipFileContainer_OpenEntry_ExistingEntry_ReturnsStream

**Scenario**: A zip containing `"readme.txt"` with content `"zip content"` is opened; `OpenEntry("readme.txt")`
is called.

**Expected**: The returned stream reads `"zip content"`.

**Requirement coverage**: Zip archive access — stream opening for existing zip entries.

##### ZipFileContainer_OpenEntry_NonExistentEntry_ThrowsIOException

**Scenario**: `OpenEntry("missing.txt")` is called on a `ZipFileContainer` wrapping an empty zip.

**Expected**: An `IOException` is thrown with a message containing the missing entry name.

**Boundary / error path**: Missing zip entry uses `IOException` rather than `FileNotFoundException`,
since zip entries are not file-system files.

**Requirement coverage**: Zip archive access — IOException for missing entries.

##### ZipFileContainer_GetEntrySize_ReturnsUncompressedLength

**Scenario**: A zip containing a single entry with 5 ASCII characters is created; `GetEntrySize` is called.

**Expected**: Returns `5L` (the uncompressed `ZipArchiveEntry.Length` value).

**Requirement coverage**: Zip archive access — correct uncompressed byte size reporting.

##### ZipFileContainer_GetDisplayPath_ReturnsDisplayNamePrefixedPath

**Scenario**: A `ZipFileContainer` is constructed with display name `"outer.zip"`; `GetDisplayPath("inner.txt")`
is called.

**Expected**: Returns `"outer.zip > inner.txt"`.

**Requirement coverage**: Zip archive access — breadcrumb display paths for error messages.

#### Requirements Coverage

- (file entry enumeration with forward slashes): ZipFileContainer_GetEntries_ReturnsFileEntriesWithForwardSlashes
- (directory markers excluded): ZipFileContainer_GetEntries_ExcludesDirectoryMarkers
- (open zip entry as stream): ZipFileContainer_OpenEntry_ExistingEntry_ReturnsStream
- (missing entry throws IOException): ZipFileContainer_OpenEntry_NonExistentEntry_ThrowsIOException
- (uncompressed entry size): ZipFileContainer_GetEntrySize_ReturnsUncompressedLength
- (breadcrumb display path): ZipFileContainer_GetDisplayPath_ReturnsDisplayNamePrefixedPath
