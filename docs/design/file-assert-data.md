# FileAssertData Design

## Overview

`FileAssertData` is the set of YAML data transfer objects (DTOs) used by YamlDotNet to
deserialize the FileAssert configuration file. Each class maps directly to a YAML structure
and is intentionally free of business logic. Domain objects are constructed from these DTOs
by the Modeling subsystem.

## Class Structure

### FileAssertRuleData

Represents a single content validation rule within a file assertion.

| Property   | YAML alias  | Type      | Description                                     |
| :--------- | :---------- | :-------- | :---------------------------------------------- |
| `Contains` | `contains`  | `string?` | Substring that file content must contain.       |
| `Matches`  | `matches`   | `string?` | Regular expression the file content must match. |

Exactly one property shall be set per rule. The `FileAssertRule.Create` factory enforces this.

### FileAssertFileData

Represents a file pattern assertion within a test.

| Property  | YAML alias | Type                         | Description                                                  |
| :-------- | :--------- | :--------------------------- | :----------------------------------------------------------- |
| `Pattern` | `pattern`  | `string?`                    | Glob pattern used to locate files.                           |
| `Min`     | `min`      | `int?`                       | Minimum number of matching files; null means no lower bound. |
| `Max`     | `max`      | `int?`                       | Maximum number of matching files; null means no upper bound. |
| `Rules`   | `rules`    | `List<FileAssertRuleData>?`  | Content rules applied to each matched file.                  |

### FileAssertTestData

Represents a named test within the configuration.

| Property | YAML alias | Type                         | Description                                   |
| :------- | :--------- | :--------------------------- | :-------------------------------------------- |
| `Name`   | `name`     | `string?`                    | Human-readable name for the test.             |
| `Tags`   | `tags`     | `List<string>?`              | Tags used for command-line filter selection.  |
| `Files`  | `files`    | `List<FileAssertFileData>?`  | File assertions belonging to this test.       |

### FileAssertConfigData

Represents the top-level configuration document.

| Property | YAML alias | Type                         | Description                                   |
| :------- | :--------- | :--------------------------- | :-------------------------------------------- |
| `Tests`  | `tests`    | `List<FileAssertTestData>?`  | Tests defined in this configuration file.     |

## Design Decisions

- **Nullable reference type properties**: All properties are nullable to correctly represent
  absent YAML keys without throwing during deserialization.
- **No validation logic in DTOs**: Validation and construction of domain objects is the
  responsibility of the factory methods in the Modeling subsystem, keeping DTOs simple.
- **YamlMember aliases**: Explicit `[YamlMember(Alias = "...")]` attributes tie each property
  to its YAML key, decoupling C# naming conventions from the YAML schema.
