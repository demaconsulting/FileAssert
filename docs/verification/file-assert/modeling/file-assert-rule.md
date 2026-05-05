### FileAssertRule Verification

This document describes the unit-level verification design for the `FileAssertRule` unit. It
defines the test scenarios, dependency usage, and requirement coverage for
`Modeling/FileAssertRule.cs`.

#### Verification Approach

`FileAssertRule` is verified with unit tests defined in `FileAssertRuleTests.cs`. Tests supply
controlled rule data objects and string content, asserting on rule type creation, application
results, and error reporting.

#### Dependencies

| Dependency | Usage in Tests                                              |
|------------|-------------------------------------------------------------|
| `Context`  | Used directly (not mocked) — created with controlled flags. |

#### Test Scenarios

##### FileAssertRule_Create_WithContains_ReturnsContainsRule

**Scenario**: `FileAssertRule.Create` is called with data specifying a `contains` rule.

**Expected**: A `ContainsRule` instance is returned.

**Requirement coverage**: Contains rule creation requirement.

##### FileAssertRule_Create_WithMatches_ReturnsMatchesRule

**Scenario**: `FileAssertRule.Create` is called with data specifying a `matches` regex rule.

**Expected**: A `MatchesRule` instance is returned.

**Requirement coverage**: Regex matches rule creation requirement.

##### FileAssertRule_Create_WithDoesNotContain_ReturnsDoesNotContainRule

**Scenario**: `FileAssertRule.Create` is called with data specifying a `does-not-contain` rule.

**Expected**: A `DoesNotContainRule` instance is returned.

**Requirement coverage**: Does-not-contain rule creation requirement.

##### FileAssertRule_Create_WithDoesNotContainRegex_ReturnsDoesNotMatchRule

**Scenario**: `FileAssertRule.Create` is called with data specifying a `does-not-match` regex rule.

**Expected**: A `DoesNotMatchRule` instance is returned.

**Requirement coverage**: Does-not-match rule creation requirement.

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

**Requirement coverage**: Contains rule pass requirement.

##### FileAssertContainsRule_Apply_ContentMissingValue_WritesError

**Scenario**: A `ContainsRule` is applied to content that does not contain the expected value.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Contains rule fail requirement.

##### FileAssertMatchesRule_Apply_ContentMatchesPattern_NoError

**Scenario**: A `MatchesRule` is applied to content that matches the regex pattern.

**Expected**: No error is written to the context; exit code is 0.

**Requirement coverage**: Regex match pass requirement.

##### FileAssertMatchesRule_Apply_ContentDoesNotMatchPattern_WritesError

**Scenario**: A `MatchesRule` is applied to content that does not match the regex pattern.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Regex match fail requirement.

##### FileAssertDoesNotContainRule_Apply_ContentContainsValue_WritesError

**Scenario**: A `DoesNotContainRule` is applied to content that contains the forbidden value.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Does-not-contain fail requirement.

##### FileAssertDoesNotContainRule_Apply_ContentMissingValue_NoError

**Scenario**: A `DoesNotContainRule` is applied to content that does not contain the forbidden value.

**Expected**: No error is written to the context; exit code is 0.

**Requirement coverage**: Does-not-contain pass requirement.

##### FileAssertDoesNotMatchRule_Apply_ContentMatchesPattern_WritesError

**Scenario**: A `DoesNotMatchRule` is applied to content that matches the forbidden regex pattern.

**Expected**: An error is written to the context; exit code is non-zero.

**Requirement coverage**: Does-not-match fail requirement.

##### FileAssertDoesNotMatchRule_Apply_ContentDoesNotMatchPattern_NoError

**Scenario**: A `DoesNotMatchRule` is applied to content that does not match the forbidden pattern.

**Expected**: No error is written to the context; exit code is 0.

**Requirement coverage**: Does-not-match pass requirement.

#### Requirements Coverage

- **Contains rule**: FileAssertRule_Create_WithContains_ReturnsContainsRule,
  FileAssertContainsRule_Apply_ContentContainsValue_NoError,
  FileAssertContainsRule_Apply_ContentMissingValue_WritesError
- **Regex matches rule**: FileAssertRule_Create_WithMatches_ReturnsMatchesRule,
  FileAssertMatchesRule_Apply_ContentMatchesPattern_NoError,
  FileAssertMatchesRule_Apply_ContentDoesNotMatchPattern_WritesError
- **Does-not-contain rule**: FileAssertRule_Create_WithDoesNotContain_ReturnsDoesNotContainRule,
  FileAssertDoesNotContainRule_Apply_ContentContainsValue_WritesError,
  FileAssertDoesNotContainRule_Apply_ContentMissingValue_NoError
- **Does-not-match rule**: FileAssertRule_Create_WithDoesNotContainRegex_ReturnsDoesNotMatchRule,
  FileAssertDoesNotMatchRule_Apply_ContentMatchesPattern_WritesError,
  FileAssertDoesNotMatchRule_Apply_ContentDoesNotMatchPattern_NoError
- **Unknown type guard**: FileAssertRule_Create_WithNoType_ThrowsInvalidOperationException
- **Null data guard**: FileAssertRule_Create_WithNullData_ThrowsArgumentNullException
