# FileAssertData Verification

This document describes the unit-level verification design for the `FileAssertData` unit. It
defines the verification approach for `Configuration/FileAssertData.cs`.

## Verification Approach

`FileAssertData` consists of data-transfer objects used exclusively for YAML deserialization. They
carry no logic and are exercised indirectly through `FileAssertConfig.ReadFromFile` in
`FileAssertConfigTests.cs`. No dedicated test file exists; all coverage is inherited from the
`FileAssertConfig` tests.

## Dependencies

`FileAssertData` depends only on YamlDotNet deserialization annotations. No mocking is needed.

## Coverage

`FileAssertData` objects are verified indirectly by every `FileAssertConfig_ReadFromFile_*` test
scenario in `FileAssertConfigTests.cs` that supplies YAML content. Correct population of all
fields confirms the data-transfer objects are correctly annotated and deserialized.

## Requirements Coverage

All `FileAssertData` requirements are satisfied indirectly by the `FileAssertConfig` test
scenarios. See [FileAssertConfig Verification](file-assert-config.md) for details.
