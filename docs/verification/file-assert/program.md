# Program Verification

This document describes the unit-level verification design for the `Program` unit. It defines the
test scenarios, dependency usage, and requirement coverage for `Program.cs`.

## Verification Approach

`Program` is verified with unit tests defined in `ProgramTests.cs`. Because `Program` directly
instantiates `Context` from real arguments and calls `Validation.Run` when needed, no mocking is
required. The tests pass controlled argument arrays and assert on captured console output and exit
codes.

## Dependencies

| Dependency   | Usage in Tests                                                           |
|--------------|--------------------------------------------------------------------------|
| `Context`    | Used directly (not mocked) — created from the argument array under test. |
| `Validation` | Used directly (not mocked) — called when the validate flag is set.       |

No test doubles are introduced at the `Program` level; all collaborators execute their real logic.

## Test Scenarios

### Program_Run_WithVersionFlag_DisplaysVersionOnly

**Scenario**: `Program.Run` is called with a context created from `["--version"]`.

**Expected**: Standard output contains the version string; the word "Copyright" does not appear;
exit code is 0.

**Requirement coverage**: Version display requirement.

### Program_Run_WithHelpFlag_DisplaysUsageInformation

**Scenario**: `Program.Run` is called with a context created from `["--help"]`.

**Expected**: Standard output contains "Usage:"; exit code is 0.

**Requirement coverage**: Help display requirement.

### Program_Run_WithValidateFlag_RunsValidation

**Scenario**: `Program.Run` is called with a context created from `["--validate"]`.

**Expected**: Standard output contains "Total Tests:"; exit code is 0.

**Requirement coverage**: Self-validation requirement.

### Program_Run_NoArguments_DisplaysDefaultBehavior

**Scenario**: `Program.Run` is called with a context created from an empty argument array.

**Expected**: Standard output contains the tool name and copyright notice; exit code is 0.

**Requirement coverage**: Default behavior requirement.

### Program_Version_ReturnsNonEmptyString

**Scenario**: The `Program.Version` static property is read.

**Expected**: The returned string is non-empty and non-null.

**Requirement coverage**: Version string availability requirement.

## Requirements Coverage

- **Version display**: Program_Run_WithVersionFlag_DisplaysVersionOnly,
  Program_Version_ReturnsNonEmptyString
- **Help display**: Program_Run_WithHelpFlag_DisplaysUsageInformation
- **Self-validation**: Program_Run_WithValidateFlag_RunsValidation
- **Default behavior**: Program_Run_NoArguments_DisplaysDefaultBehavior
