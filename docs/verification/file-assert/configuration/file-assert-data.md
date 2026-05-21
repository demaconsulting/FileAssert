### FileAssertData Verification

This document describes the unit-level verification design for the `FileAssertData` unit. It
defines the verification approach for `Configuration/FileAssertData.cs`.

#### Verification Approach

`FileAssertData` consists of data-transfer objects used exclusively for YAML deserialization. They
carry no logic and are exercised indirectly through `FileAssertConfig.ReadFromFile` in
`FileAssertConfigTests.cs`. No dedicated test file exists; all coverage is inherited from the
`FileAssertConfig` tests.

#### Test Environment

Tests execute in the standard CI pipeline environment using the xUnit test runner. No
special hardware, peripherals, or environment configuration is required.

#### Acceptance Criteria

N/A – Acceptance criteria are managed at the subsystem and system integration levels.
Unit tests provide fine-grained coverage evidence; formal acceptance is declared at the
subsystem level when all unit tests supporting a subsystem requirement pass.

#### Dependencies

`FileAssertData` depends only on YamlDotNet deserialization annotations. No mocking is needed.

#### Test Scenarios

The following named scenarios exercise the `FileAssertData` schema indirectly via
`FileAssertConfig.ReadFromFile`:

- **Valid minimal YAML schema deserializes correctly** — A YAML file containing a single
  `tests` entry with a `name` and a `files` list with one `pattern` is parsed without error
  and the resulting domain object reflects the expected values.
  Covered by: `FileAssertConfig_ReadFromFile_ValidFile_ReturnsConfig`.

- **PDF assertion block deserializes correctly** — A YAML file containing a `pdf:` block
  with `metadata`, `pages`, and `text` sub-keys is parsed without error and the resulting
  domain object contains a non-null PDF assertion.
  Covered by: `FileAssertConfig_ReadFromFile_PdfAssertConfig_ParsesCorrectly`.

#### Coverage

`FileAssertData` objects are verified indirectly by every `FileAssertConfig_ReadFromFile_*` test
scenario in `FileAssertConfigTests.cs` that supplies YAML content. Correct population of all
fields confirms the data-transfer objects are correctly annotated and deserialized.

#### Requirements Coverage

All `FileAssertData` requirements are satisfied indirectly by the `FileAssertConfig` test
scenarios. See the FileAssertConfig Verification document for details.
