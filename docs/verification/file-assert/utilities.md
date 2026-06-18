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

### Test Scenarios

The following integration test scenarios are defined in `UtilitiesTests.cs`.

#### Utilities_SafePathCombine_PreventsPathTraversalToFileSystem

**Scenario**: A path traversal pattern (e.g., `../`) is passed as the relative path argument to
`PathHelpers.SafePathCombine` with a real temporary directory as the base path.

**Expected**: An `ArgumentException` is thrown; no traversal of the file system occurs.

#### Utilities_FileContainerAbstraction_ZipFileContainer_EndToEnd

**Scenario**: A real in-memory zip archive containing multiple entries is wrapped in a
`ZipFileContainer`, and the full `IFileContainer` surface (`GetEntries`, `OpenEntry`,
`GetEntrySize`, `GetDisplayPath`) is exercised through the abstraction.

**Expected**: All entries are enumerated, content is read back correctly, entry sizes report
the uncompressed lengths, and display paths include the archive breadcrumb prefix.

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

### IFileContainer Verification

The `IFileContainer` interface and its two implementations (`DirectoryFileContainer`,
`ZipFileContainer`) are verified by the unit tests defined in `IFileContainerTests.cs`.
Each test exercises all four interface members against real filesystem directories and
in-memory zip archives.

#### IFileContainer Test Scenarios

- **DirectoryFileContainer_GetEntries_ReturnsAllFilesWithForwardSlashes** – confirms recursive
  enumeration with forward-slash separator normalization.
- **DirectoryFileContainer_GetEntries_EmptyDirectory_ReturnsEmpty** – confirms empty list for
  an empty directory.
- **DirectoryFileContainer_GetEntries_NonExistentDirectory_ReturnsEmpty** – confirms empty list
  (no exception) when the base directory does not exist.
- **DirectoryFileContainer_OpenEntry_ExistingFile_ReturnsStream** – confirms a readable stream
  is returned for an existing file.
- **DirectoryFileContainer_OpenEntry_NonExistentFile_ThrowsIOException** – confirms
  `FileNotFoundException` is thrown for a missing file.
- **DirectoryFileContainer_GetEntrySize_ReturnsCorrectSize** – confirms the correct byte count
  is returned.
- **DirectoryFileContainer_GetDisplayPath_RootEntry_ReturnsFullPath** – confirms the full
  file-system path is returned.
- **ZipFileContainer_GetEntries_ReturnsFileEntriesWithForwardSlashes** – confirms zip entry
  enumeration with forward slashes, excluding directory markers.
- **ZipFileContainer_GetEntries_ExcludesDirectoryMarkers** – confirms that zip directory marker
  entries (names ending in `/`) are excluded from `GetEntries()`.
- **ZipFileContainer_OpenEntry_ExistingEntry_ReturnsStream** – confirms a readable stream for
  an existing zip entry.
- **ZipFileContainer_OpenEntry_NonExistentEntry_ThrowsIOException** – confirms `IOException`
  for a missing zip entry.
- **ZipFileContainer_GetEntrySize_ReturnsUncompressedLength** – confirms the uncompressed byte
  count is returned.
- **ZipFileContainer_GetDisplayPath_ReturnsDisplayNamePrefixedPath** – confirms the
  breadcrumb-style display path `"displayName > entryPath"`.
