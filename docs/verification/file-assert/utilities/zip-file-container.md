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
collection is marked `[Collection("Sequential")]` to serialize tests that share temporary
directory and stream resources. No special hardware, peripherals, or environment configuration
is required beyond the standard build toolchain.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. The IO behaviors documented in
the design (entry enumeration, lookup, size reporting, display-path formatting) all return the
documented value or raise the documented exception type for each scenario.

#### Dependencies

`ZipFileContainer` depends on `System.IO.Compression.ZipArchive` and `ZipArchiveEntry`. No
mocking is needed at this level.

#### Test Scenarios

##### ZipFileContainer_GetEntries_ReturnsFileEntriesWithForwardSlashes

**Scenario**: A `ZipFileContainer` is created from an in-memory zip containing two entries
`"lib/a.dll"` and `"lib/b.dll"`.

**Expected**: `GetEntries()` returns exactly 2 entries with forward slashes; directory marker
entries (names ending in `/`) are excluded.

##### ZipFileContainer_GetEntries_ExcludesDirectoryMarkers

**Scenario**: A `ZipFileContainer` is created from an in-memory zip containing a directory
marker entry `"lib/"` and a file entry `"lib/a.dll"`.

**Expected**: `GetEntries()` returns only `"lib/a.dll"`; the directory marker entry (name ending
in `/`) is excluded.

**Boundary / error path**: Directory marker exclusion during entry enumeration.

##### ZipFileContainer_OpenEntry_ExistingEntry_ReturnsStream

**Scenario**: A zip containing `"readme.txt"` with content `"zip content"` is opened; `OpenEntry("readme.txt")`
is called.

**Expected**: The returned stream reads `"zip content"`.

##### ZipFileContainer_OpenEntry_NonExistentEntry_ThrowsIOException

**Scenario**: `OpenEntry("missing.txt")` is called on a `ZipFileContainer` wrapping an empty zip.

**Expected**: An `IOException` is thrown with a message containing the missing entry name.

**Boundary / error path**: Missing zip entry uses `IOException` rather than `FileNotFoundException`,
since zip entries are not file-system files.

##### ZipFileContainer_GetEntrySize_ReturnsUncompressedLength

**Scenario**: A zip containing a single entry with 5 ASCII characters is created; `GetEntrySize` is called.

**Expected**: Returns `5L` (the uncompressed `ZipArchiveEntry.Length` value).

##### ZipFileContainer_GetDisplayPath_ReturnsDisplayNamePrefixedPath

**Scenario**: A `ZipFileContainer` is constructed with display name `"outer.zip"`; `GetDisplayPath("inner.txt")`
is called.

**Expected**: Returns `"outer.zip > inner.txt"`.

##### ZipFileContainer_BackslashEntryPath_OpensAndSizesAfterNormalization

**Scenario**: A zip stores entry `"lib/a.dll"` (forward slash). The test calls
`OpenEntry("lib\\a.dll")` and `GetEntrySize("lib\\a.dll")` using a backslash separator.

**Expected**: `OpenEntry` returns a stream that reads the entry's content; `GetEntrySize`
returns the uncompressed length of the entry. Both APIs locate the entry by normalizing
backslashes to forward slashes before calling `ZipArchive.GetEntry(...)`.

**Boundary / error path**: Backslash-separator normalization in `OpenEntry` and `GetEntrySize`.
