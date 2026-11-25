### HelpCommand Verification

This document describes the unit-level verification design for the `HelpCommand` unit. It defines
the test scenarios, dependency usage, and requirement coverage for `Cli/HelpCommand.cs`.

#### Verification Approach

`HelpCommand` is verified with unit tests defined in `HelpCommandTests.cs`. Each test creates a
help command with `HelpCommand.Create`, which writes to in-memory writers instead of the console.
No mocking is applied: the tests use the real help cards embedded in the tool assembly and the
real HelpCards library, a documented dependency of the unit.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. The help
verb prints the topic index, the shipped catalog passes validation with warnings treated as
errors, and every card prints in lines of at most 120 characters.

#### Test Scenarios

##### HelpCommand_TryRun_HelpVerb_PrintsTopicIndexAndReturnsZero

**Scenario**: `TryRun` is called with `["help"]`.

**Expected**: The command answers with exit code 0, and the output lists the command of every root
topic.

##### HelpCommand_CreateOptions_ShippedCatalog_PassesValidation

**Scenario**: The catalog configuration from `CreateOptions` is validated with warnings treated as
errors.

**Expected**: No defect is reported, which proves that the root cards are exactly `RootCardIds`,
that every card carries one purpose tag, and that every command example parses as a command line
of the tool.

##### HelpCommand_TryRun_EveryCard_FitsMaximumLineWidth

**Scenario**: Every card of the shipped catalog is printed with its links and tokens expanded and a
fixed, long prerelease version in place of the version of the build.

**Expected**: Each card prints with exit code 0 and has no line longer than 120 characters.
