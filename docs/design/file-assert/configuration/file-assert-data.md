### FileAssertData Design

#### Overview

`FileAssertData` is the set of YAML data transfer objects (DTOs) used by YamlDotNet to
deserialize the FileAssert configuration file. Each class maps directly to a YAML structure
and is intentionally free of business logic. Domain objects are constructed from these DTOs
by the Modeling subsystem.

#### Class Structure

##### FileAssertRuleData

Represents a single content validation rule within a file assertion.

| Property              | YAML alias               | Type      | Description                                         |
| :-------------------- | :----------------------- | :-------- | :-------------------------------------------------- |
| `Contains`            | `contains`               | `string?` | Substring that file content must contain.           |
| `DoesNotContain`      | `does-not-contain`       | `string?` | Substring that file content must NOT contain.       |
| `Matches`             | `matches`                | `string?` | Regular expression the file content must match.     |
| `DoesNotContainRegex` | `does-not-contain-regex` | `string?` | Regular expression the file content must NOT match. |

Exactly one property shall be set per rule. The `FileAssertRule.Create` factory enforces this.

##### FileAssertFileData

Represents a file pattern assertion within a test.

| Property   | YAML alias | Type                        | Description                                                  |
| :--------- | :--------- | :-------------------------- | :----------------------------------------------------------- |
| `Pattern`  | `pattern`  | `string?`                   | Glob pattern used to locate files.                           |
| `Min`      | `min`      | `int?`                      | Minimum number of matching files; null means no lower bound. |
| `Max`      | `max`      | `int?`                      | Maximum number of matching files; null means no upper bound. |
| `Count`    | `count`    | `int?`                      | Exact number of matching files; null means no exact bound.   |
| `MinSize`  | `min-size` | `long?`                     | Minimum file size in bytes; null means no lower bound.       |
| `MaxSize`  | `max-size` | `long?`                     | Maximum file size in bytes; null means no upper bound.       |
| `Text`     | `text`     | `List<FileAssertRuleData>?` | Text content rules (used by `FileAssertTextAssert`).         |
| `Pdf`      | `pdf`      | `FileAssertPdfData?`        | PDF document assertions.                                     |
| `Xml`      | `xml`      | `List<FileAssertQueryData>?`| XML node count assertions using XPath.                       |
| `Html`     | `html`     | `List<FileAssertQueryData>?`| HTML node count assertions using XPath.                      |
| `Yaml`     | `yaml`     | `List<FileAssertQueryData>?`| YAML node count assertions using dot-notation.               |
| `Json`     | `json`     | `List<FileAssertQueryData>?`| JSON node count assertions using dot-notation.               |

##### FileAssertTestData

Represents a named test within the configuration.

| Property | YAML alias | Type                        | Description                                  |
| :------- | :--------- | :-------------------------- | :------------------------------------------- |
| `Name`   | `name`     | `string?`                   | Human-readable name for the test.            |
| `Tags`   | `tags`     | `List<string>?`             | Tags used for command-line filter selection. |
| `Files`  | `files`    | `List<FileAssertFileData>?` | File assertions belonging to this test.      |

##### FileAssertConfigData

Represents the top-level configuration document.

| Property | YAML alias | Type                        | Description                               |
| :------- | :--------- | :-------------------------- | :---------------------------------------- |
| `Tests`  | `tests`    | `List<FileAssertTestData>?` | Tests defined in this configuration file. |

##### FileAssertPdfMetadataRuleData

Represents a single PDF metadata field assertion.

| Property   | YAML alias | Type      | Description                                         |
| :--------- | :--------- | :-------- | :-------------------------------------------------- |
| `Field`    | `field`    | `string?` | PDF metadata field name (e.g. Title, Author).       |
| `Contains` | `contains` | `string?` | Metadata value must contain this substring.         |
| `Matches`  | `matches`  | `string?` | Metadata value must match this regular expression.  |

##### FileAssertPdfPagesData

Represents PDF page count constraints.

| Property | YAML alias | Type   | Description                   |
| :------- | :--------- | :----- | :---------------------------- |
| `Min`    | `min`      | `int?` | Minimum number of pages.      |
| `Max`    | `max`      | `int?` | Maximum number of pages.      |

##### FileAssertPdfData

Represents the `pdf:` assertion block for a file entry.

| Property   | YAML alias | Type                                    | Description                              |
| :--------- | :--------- | :-------------------------------------- | :--------------------------------------- |
| `Metadata` | `metadata` | `List<FileAssertPdfMetadataRuleData>?`  | Metadata field assertions.               |
| `Pages`    | `pages`    | `FileAssertPdfPagesData?`               | Page count constraints.                  |
| `Text`     | `text`     | `List<FileAssertRuleData>?`             | Body text assertions (contains/matches). |

##### FileAssertQueryData

Represents a single structured-document query assertion, shared by XML, HTML, YAML, and JSON
assertion blocks.

| Property | YAML alias | Type      | Description                               |
| :------- | :--------- | :-------- | :---------------------------------------- |
| `Query`  | `query`    | `string?` | XPath expression or dot-notation path.    |
| `Count`  | `count`    | `int?`    | Exact number of matched nodes.            |
| `Min`    | `min`      | `int?`    | Minimum number of matched nodes.          |
| `Max`    | `max`      | `int?`    | Maximum number of matched nodes.          |

#### Design Decisions

#### Design Constraints

The `FileAssertData` classes contain no validation or business logic, delegating all
validation to the factory methods in the Modeling subsystem. This maintains a clean
separation between deserialization and domain object construction.

- **Nullable reference type properties**: All properties are nullable to correctly represent
  absent YAML keys without throwing during deserialization.
- **No validation logic in DTOs**: Validation and construction of domain objects is the
  responsibility of the factory methods in the Modeling subsystem, keeping DTOs simple.
- **YamlMember aliases**: Explicit `[YamlMember(Alias = "...")]` attributes tie each property
  to its YAML key, decoupling C# naming conventions from the YAML schema.

#### Purpose

The `FileAssertData` file defines the complete set of YAML data transfer objects (DTOs) used
by `FileAssertConfig.ReadFromFile` to deserialize the `.fileassert.yaml` configuration. Each
class carries no business logic and serves exclusively as a container for raw values produced
by YamlDotNet.

#### Data Model

| DTO Class                       | Fields                                                                      |
| :------------------------------ | :-------------------------------------------------------------------------- |
| `FileAssertConfigData`          | `Tests: List<FileAssertTestData>?`                                          |
| `FileAssertTestData`            | `Name?, Tags (list)?, Files (list)?`                                        |
| `FileAssertFileData`            | Pattern, Min, Max, Count, MinSize, MaxSize; Text/Pdf/Xml/Html/Yaml/Json/Zip |
| `FileAssertRuleData`            | `Contains?, DoesNotContain?, Matches?, DoesNotContainRegex?`                |
| `FileAssertPdfData`             | `Metadata (list)?, Pages?, Text (list)?`                                    |
| `FileAssertPdfMetadataRuleData` | `Field?, Contains?, Matches?`                                               |
| `FileAssertPdfPagesData`        | `Min: int?`, `Max: int?`                                                    |
| `FileAssertQueryData`           | `Query?, Count?, Min?, Max?`                                                |
| `FileAssertZipData`             | `Entries: List<FileAssertZipEntryData>?`                                    |
| `FileAssertZipEntryData`        | `Pattern?, Min?, Max?`                                                      |

All properties are nullable so that absent YAML keys deserialize cleanly to `null`.

#### Key Methods

N/A — DTOs are pure data containers. All properties are public `get`/`set`. No methods
are defined.

#### Error Handling

N/A — DTOs contain no validation logic. Any malformed YAML causes `YamlDotNet` to throw
a `YamlException` that propagates directly to `FileAssertConfig.ReadFromFile`. Constraint
validation (e.g. exactly one rule type per `FileAssertRuleData`) is the responsibility of
the Modeling subsystem factory methods.

#### Interactions

- **Populated by**: `YamlDotNet.Serialization.Deserializer` inside `FileAssertConfig.ReadFromFile`
  via `DeserializerBuilder().IgnoreUnmatchedProperties().Build()`.
- **Consumed by**:
  - `FileAssertTest.Create(FileAssertTestData)` in the Modeling subsystem.
  - `FileAssertFile.Create(FileAssertFileData)` in the Modeling subsystem.
  - `FileAssertRule.Create(FileAssertRuleData)` in the Modeling subsystem.
  - `FileAssertTextAssert.Create(IEnumerable<FileAssertRuleData>)` in the Modeling subsystem.
  - `FileAssertPdfAssert.Create(FileAssertPdfData)` in the Modeling subsystem.
  - `FileAssertHtmlAssert.Create(IEnumerable<FileAssertQueryData>)` in the Modeling subsystem.
  - `FileAssertJsonAssert.Create(IEnumerable<FileAssertQueryData>)` in the Modeling subsystem.
  - `FileAssertXmlAssert.Create(IEnumerable<FileAssertQueryData>)` in the Modeling subsystem.
  - `FileAssertYamlAssert.Create(IEnumerable<FileAssertQueryData>)` in the Modeling subsystem.
  - `FileAssertZipAssert.Create(FileAssertZipData)` in the Modeling subsystem.
