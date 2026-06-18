### FileAssertRule Verification

This document describes the unit-level verification design for the `FileAssertRule` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertRule.cs`.

#### Verification Approach

`FileAssertRule` is verified with unit tests defined in `FileAssertRuleTests.cs`. Tests supply
controlled rule data objects and string content, asserting on rule type creation, application
results, and error reporting.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

All listed unit test scenarios pass on every supported platform and runtime combination. No
test failures, unhandled exceptions, or assertion errors occur. Code coverage for `FileAssertRule.cs`
meets the project minimum threshold.

#### Dependencies

| Dependency | Usage in Tests                                              |
|------------|-------------------------------------------------------------|
| `Context`  | Used directly (not mocked) — created with controlled flags. |

#### Test Scenarios

##### FileAssertRule_Create_WithContains_ReturnsContainsRule

**Scenario**: `FileAssertRule.Create` is called with data specifying a `contains` rule.

**Expected**: A `ContainsRule` instance is returned.

##### FileAssertRule_Create_WithMatches_ReturnsMatchesRule

**Scenario**: `FileAssertRule.Create` is called with data specifying a `matches` regex rule.

**Expected**: A `MatchesRule` instance is returned.

##### FileAssertRule_Create_WithDoesNotContain_ReturnsDoesNotContainRule

**Scenario**: `FileAssertRule.Create` is called with data specifying a `does-not-contain` rule.

**Expected**: A `DoesNotContainRule` instance is returned.

##### FileAssertRule_Create_WithDoesNotContainRegex_ReturnsDoesNotMatchRule

**Scenario**: `FileAssertRule.Create` is called with data specifying a `does-not-match` regex rule.

**Expected**: A `DoesNotMatchRule` instance is returned.

##### FileAssertRule_Create_WithNoType_ThrowsInvalidOperationException

**Scenario**: `FileAssertRule.Create` is called with data that specifies no rule type.

**Expected**: An `InvalidOperationException` is thrown.

**Boundary / error path**: Unknown rule type validation.

##### FileAssertRule_Create_WithNullData_ThrowsArgumentNullException

**Scenario**: `FileAssertRule.Create` is called with `null` data.

**Expected**: An `ArgumentNullException` is thrown.

**Boundary / error path**: Null data guard.

##### FileAssertContainsRule_Apply_ContentContainsValue_NoError

**Scenario**: A `ContainsRule` is applied to content that contains the expected value.

**Expected**: No error is written to the context; exit code is 0.

##### FileAssertContainsRule_Apply_ContentMissingValue_WritesError

**Scenario**: A `ContainsRule` is applied to content that does not contain the expected value.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertMatchesRule_Apply_ContentMatchesPattern_NoError

**Scenario**: A `MatchesRule` is applied to content that matches the regex pattern.

**Expected**: No error is written to the context; exit code is 0.

##### FileAssertMatchesRule_Apply_ContentDoesNotMatchPattern_WritesError

**Scenario**: A `MatchesRule` is applied to content that does not match the regex pattern.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertDoesNotContainRule_Apply_ContentContainsValue_WritesError

**Scenario**: A `DoesNotContainRule` is applied to content that contains the forbidden value.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertDoesNotContainRule_Apply_ContentMissingValue_NoError

**Scenario**: A `DoesNotContainRule` is applied to content that does not contain the forbidden value.

**Expected**: No error is written to the context; exit code is 0.

##### FileAssertDoesNotMatchRule_Apply_ContentMatchesPattern_WritesError

**Scenario**: A `DoesNotMatchRule` is applied to content that matches the forbidden regex pattern.

**Expected**: An error is written to the context; exit code is non-zero.

##### FileAssertDoesNotMatchRule_Apply_ContentDoesNotMatchPattern_NoError

**Scenario**: A `DoesNotMatchRule` is applied to content that does not match the forbidden pattern.

**Expected**: No error is written to the context; exit code is 0.
