# FileAssertRule Design

## Overview

The `FileAssertRule` class hierarchy provides the content validation rules used by
`FileAssertFile` to assert the textual content of matched files. Rules are created
from YAML configuration data using a factory method and are applied to file content
during test execution.

## Class Structure

### FileAssertRule (Abstract Base Class)

`FileAssertRule` is an abstract class that defines the common interface for all
content validation rules. It provides a static factory method that selects the
correct concrete implementation based on the deserialized YAML data.

### Factory Method

```csharp
internal static FileAssertRule Create(FileAssertRuleData data)
```

The factory inspects the `Contains` and `Matches` properties of the data object in
order. The first non-null property determines the concrete type returned. If neither
property is set the factory throws `InvalidOperationException` with a descriptive
message.

### Abstract Method

```csharp
internal abstract void Apply(Context context, string fileName, string content)
```

Each derived class implements `Apply` to perform its specific check against
`content`. When the check fails the rule calls `context.WriteError` to record the
failure; no exception is thrown, allowing all rules to be evaluated independently.

### FileAssertContainsRule

Checks whether the file content contains a required substring using an ordinal
(byte-exact) string comparison. This is appropriate for license header checks,
copyright notices, and other exact-text requirements.

#### Error Message Format

```text
File '<fileName>' does not contain expected text '<Value>'
```

### FileAssertMatchesRule

Checks whether the file content matches a regular expression. The regex is compiled
at construction time with a ten-second evaluation timeout to guard against
catastrophic backtracking on adversarial or malformed content.

#### Regex Error Message Format

```text
File '<fileName>' does not match pattern '<Pattern>'
```

## YAML Configuration

Rules are declared under the `rules` key of a file entry. Each rule item specifies
exactly one of the supported rule types:

```yaml
rules:
  - contains: "Copyright (c) DEMA Consulting"
  - matches: "Copyright \\(c\\) \\d{4}"
```

The `FileAssertRuleData` data transfer object is deserialized by YamlDotNet and
passed to `FileAssertRule.Create` to produce the concrete rule instance.

## Design Decisions

- **Abstract base class over interface**: The base class provides the factory
  method alongside the abstract `Apply` method, keeping rule creation and
  execution logic in one cohesive type.
- **Ordinal comparison in ContainsRule**: Locale-independent comparison avoids
  subtle failures when CI runners have different cultural settings.
- **Regex timeout**: A ten-second timeout prevents a single malformed pattern from
  blocking the entire test run indefinitely.
- **No exception on failure**: Rules report failures via `context.WriteError` rather
  than throwing, so all rules are applied to all files and all failures are reported
  in a single run.
