# FileAssertTextAssert Verification

This document describes the unit-level verification design for the `FileAssertTextAssert` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertTextAssert.cs`.

## Verification Approach

`FileAssertTextAssert` is verified with unit tests defined in `FileAssertTextAssertTests.cs`. Tests
create temporary files with controlled content and assert on rule evaluation and error reporting.

## Dependencies

| Dependency       | Usage in Tests                                              |
|------------------|-------------------------------------------------------------|
| `Context`        | Used directly (not mocked) — created with controlled flags. |
| `FileAssertRule` | Used directly (not mocked).                                 |

## Test Scenarios

### FileAssertTextAssert_Create_ValidData_CreatesTextAssert

**Scenario**: `FileAssertTextAssert.Create` is called with valid data containing at least one rule.

**Expected**: A non-null `FileAssertTextAssert` instance is returned.

**Requirement coverage**: Text assert creation requirement.

### FileAssertTextAssert_Create_NullData_ThrowsArgumentNullException

**Scenario**: `FileAssertTextAssert.Create` is called with `null` data.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null data guard.

### FileAssertTextAssert_Run_FileContainsText_NoError

**Scenario**: `FileAssertTextAssert.Run` is called on a file whose content satisfies all rules.

**Expected**: No errors are written to the context; exit code is 0.

**Requirement coverage**: Text assertion pass requirement.

### FileAssertTextAssert_Run_FileMissingText_WritesError

**Scenario**: `FileAssertTextAssert.Run` is called on a file whose content does not satisfy a
`contains` rule.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Text assertion fail requirement.

### FileAssertTextAssert_Run_NonExistentFile_WritesError

**Scenario**: `FileAssertTextAssert.Run` is called with a path that does not exist.

**Expected**: An error is written to the context; exit code is non-zero.

**Boundary / error path**: Missing file error path.

### FileAssertTextAssert_Run_MultipleRulesMultipleViolations_WritesMultipleErrors

**Scenario**: `FileAssertTextAssert.Run` is called on a file that violates multiple rules.

**Expected**: A separate error is written for each violation; exit code is non-zero.

**Requirement coverage**: Multiple-rule violation reporting requirement.

## Requirements Coverage

- **Text assert creation**: FileAssertTextAssert_Create_ValidData_CreatesTextAssert
- **Null guard**: FileAssertTextAssert_Create_NullData_ThrowsArgumentNullException
- **Pass**: FileAssertTextAssert_Run_FileContainsText_NoError
- **Fail**: FileAssertTextAssert_Run_FileMissingText_WritesError
- **Missing file**: FileAssertTextAssert_Run_NonExistentFile_WritesError
- **Multiple violations**: FileAssertTextAssert_Run_MultipleRulesMultipleViolations_WritesMultipleErrors
