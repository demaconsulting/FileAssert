### PathHelpers Verification

This document describes the unit-level verification design for the `PathHelpers` unit. It defines
the test scenarios, dependency usage, and requirement coverage for `Utilities/PathHelpers.cs`.

#### Verification Approach

`PathHelpers` is verified with unit tests defined in `PathHelpersTests.cs`. Because `PathHelpers`
performs pure path manipulation using only .NET BCL types, no mocking or test doubles are needed.
Tests call `PathHelpers.SafePathCombine` directly with controlled base and relative path arguments
and assert on the returned string or the thrown exception.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

N/A – Acceptance criteria are managed at the subsystem and system integration levels.
Unit tests provide fine-grained coverage evidence; formal acceptance is declared at the
subsystem level when all unit tests supporting a subsystem requirement pass.

#### Dependencies

`PathHelpers` has no dependencies on other tool units. All path operations use .NET BCL types
(`Path`, `string`); no mocking is needed at this level.

#### Test Scenarios

##### PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly

**Scenario**: A relative path (e.g., `"subfolder/file.txt"`) is combined with a base path.

**Expected**: The returned path equals the expected combined result; no exception is thrown.

**Requirement coverage**: Valid path combination requirement.

##### PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException

**Scenario**: A relative path starting with `"../"` is passed to `SafePathCombine`.

**Expected**: An `ArgumentException` is thrown containing the text "Invalid path component".

**Boundary / error path**: Directory traversal attempt via leading `../`.

**Requirement coverage**: Traversal rejection requirement.

##### PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException

**Scenario**: A relative path containing `"subfolder/../../../etc/passwd"` is passed to
`SafePathCombine`.

**Expected**: An `ArgumentException` is thrown.

**Boundary / error path**: Directory traversal attempt via embedded `../` sequence.

**Requirement coverage**: Embedded traversal rejection requirement.

##### PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException

**Scenario**: An absolute path is passed as the relative argument to `SafePathCombine`.
Sub-cases:

- Unix-style: `"/etc/passwd"` (tested on all platforms).
- Windows-style: `"C:\Windows\System32\file.txt"` (tested only when `OperatingSystem.IsWindows()` is true).

**Expected**: An `ArgumentException` is thrown for each sub-case.

**Boundary / error path**: Absolute path used where a relative path is required.

**Requirement coverage**: Absolute path rejection requirement.

##### PathHelpers_SafePathCombine_CurrentDirectoryReference_CombinesCorrectly

**Scenario**: A relative path starting with `"./"` (e.g., `"./subfolder/file.txt"`) is combined
with a base path.

**Expected**: The returned path equals the expected combined result; no exception is thrown.

**Requirement coverage**: Current-directory prefix requirement.

##### PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly

**Scenario**: A deeply nested relative path (e.g., `"a/b/c/d/file.txt"`) is combined with a
base path.

**Expected**: The returned path equals the expected combined result; no exception is thrown.

**Requirement coverage**: Nested path combination requirement.

##### PathHelpers_SafePathCombine_EmptyRelativePath_ReturnsBasePath

**Scenario**: An empty string is passed as the relative path argument.

**Expected**: The returned path equals the base path; no exception is thrown.

**Boundary / error path**: Empty relative path edge case.

**Requirement coverage**: Empty relative path requirement.

##### PathHelpers_SafePathCombine_DoubleDotInFilename_CombinesCorrectly

**Scenario**: A relative path whose filename starts with `".."` but is not a traversal sequence
(e.g., `"..data/file.txt"`) is combined with a base path.

**Expected**: The returned path equals the expected combined result; no exception is thrown.

**Boundary / error path**: Filename beginning with `".."` must not be misidentified as a traversal.

**Requirement coverage**: Dot-dot-prefixed filename requirement.

##### PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException

**Scenario**: `null` is passed as the `basePath` argument to `SafePathCombine`.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null guard on `basePath`.

**Requirement coverage**: Null input rejection requirement.

##### PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException

**Scenario**: `null` is passed as the `relativePath` argument to `SafePathCombine`.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null guard on `relativePath`.

**Requirement coverage**: Null input rejection requirement.

#### Requirements Coverage

- **FileAssert-PathHelpers-SafeCombine** (safe path combination):
  - PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly
  - PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException
  - PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException
  - PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException
  - PathHelpers_SafePathCombine_CurrentDirectoryReference_CombinesCorrectly
  - PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly
  - PathHelpers_SafePathCombine_EmptyRelativePath_ReturnsBasePath
  - PathHelpers_SafePathCombine_DoubleDotInFilename_CombinesCorrectly

- **FileAssert-PathHelpers-NullValidation** (null input rejection):
  - PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException
  - PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException
