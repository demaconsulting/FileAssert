# Configuration Subsystem Design

## Overview

The Configuration subsystem is responsible for reading the YAML test-suite configuration file
and constructing the domain object hierarchy that drives test execution. It owns the data
transfer objects used during deserialization and the top-level configuration class that loads
and runs the tests.

## Subsystem Contents

| Unit               | File                   | Responsibility                                              |
| :----------------- | :--------------------- | :---------------------------------------------------------- |
| `FileAssertConfig` | `FileAssertConfig.cs`  | Loads the YAML file and runs the filtered test suite.       |
| `FileAssertData`   | `FileAssertData.cs`    | Data transfer objects for YAML deserialization.             |

## Subsystem Responsibilities

- Read and deserialize a YAML configuration file using YamlDotNet.
- Tolerate unknown YAML properties for forward compatibility.
- Construct the full `FileAssertTest → FileAssertFile → FileAssertRule` hierarchy from the
  deserialized data.
- Resolve the base directory for glob patterns from the configuration file path.
- Filter tests by name or tag before execution.

## Interactions with Other Subsystems

| Dependency  | Usage                                                                       |
| :---------- | :-------------------------------------------------------------------------- |
| Cli         | Receives a `Context` to report errors and write progress output.            |
| Modeling    | Delegates test construction to `FileAssertTest.Create` and execution to     |
|             | `FileAssertTest.Run`.                                                       |

## YAML Configuration Format

The top-level YAML structure is:

```yaml
tests:
  - name: "Test Name"
    tags:
      - tag1
    files:
      - pattern: "**/*.cs"
        min: 1
        rules:
          - contains: "Copyright"
```

## Design Decisions

- **Separation of data and domain objects**: The `FileAssertData` classes are pure data holders
  with no logic. The Modeling subsystem owns the domain objects built from them.
- **Forward-compatible deserialization**: `IgnoreUnmatchedProperties()` allows configuration
  files to contain keys introduced in later tool versions without causing parse failures.
- **Base directory from config path**: Resolving glob patterns relative to the configuration
  file location is more intuitive than the working directory, especially when the tool is
  invoked from a build script in a different directory.
