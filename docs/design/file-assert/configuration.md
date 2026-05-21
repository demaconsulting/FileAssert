## Configuration Subsystem Design

### Overview

The Configuration subsystem is responsible for reading the YAML test-suite configuration file
and constructing the domain object hierarchy that drives test execution. It owns the data
transfer objects used during deserialization and the top-level configuration class that loads
and runs the tests.

### Subsystem Contents

| Unit               | File                   | Responsibility                                              |
| :----------------- | :--------------------- | :---------------------------------------------------------- |
| `FileAssertConfig` | `FileAssertConfig.cs`  | Loads the YAML file and runs the filtered test suite.       |
| `FileAssertData`   | `FileAssertData.cs`    | Data transfer objects for YAML deserialization.             |

### Subsystem Responsibilities

- Read and deserialize a YAML configuration file using YamlDotNet.
- Tolerate unknown YAML properties for forward compatibility.
- Construct the full `FileAssertTest → FileAssertFile → FileAssertRule` hierarchy from the
  deserialized data.
- Resolve the base directory for glob patterns from the configuration file path.
- Filter tests by name or tag before execution.

### Interfaces

#### Exposed

| Member / Class                               | Description                                                                       |
| :------------------------------------------- | :-------------------------------------------------------------------------------- |
| `FileAssertConfig.ReadFromFile(path)`        | Reads and deserializes the YAML file; returns a `FileAssertConfig` instance.      |
| `FileAssertConfig.Run(context, filters)`     | Executes filtered tests; writes results when `context.ResultsFile` is set.        |
| `FileAssertData` DTO classes                 | Intermediate data holders produced during deserialization; consumed by Modeling.  |

#### Consumed

| Dependency                          | Usage                                                                     |
| :---------------------------------- | :----------------------------------------------------------------------   |
| `Context` (Cli subsystem)           | Receives error and progress output; provides filter list and config path. |
| `FileAssertTest.Create` (Modeling)  | Converts each `FileAssertTestData` DTO into a domain object.              |
| `FileAssertTest.Run` (Modeling)     | Executes each selected test.                                              |
| YamlDotNet                          | Deserializes the YAML configuration file into DTO objects.                |
| `DemaConsulting.TestResults`        | Serializes test outcomes to TRX or JUnit XML when results are requested.  |

### Design

`FileAssertConfig.ReadFromFile` and `FileAssertConfig.Run` collaborate as follows:

1. `ReadFromFile` opens and deserializes the YAML file via YamlDotNet into
   `FileAssertConfigData` (the top-level DTO), then calls `FileAssertTest.Create` for each
   `FileAssertTestData` entry to produce the domain-object list.
2. `Run` materializes the filter list, derives the base directory from the config file path,
   and iterates the test list — calling `FileAssertTest.MatchesFilter` to skip non-matching
   tests and `FileAssertTest.Run` to execute each selected test.
3. `FileAssertData` classes are pure data holders: they carry no logic and are used only during
   deserialization. Once `ReadFromFile` returns, the DTOs are discarded.

### Interactions with Other Subsystems

| Dependency  | Usage                                                                       |
| :---------- | :-------------------------------------------------------------------------- |
| Cli         | Receives a `Context` to report errors and write progress output.            |
| Modeling    | Delegates test construction to `FileAssertTest.Create` and execution to     |
|             | `FileAssertTest.Run`.                                                       |

### YAML Configuration Format

The top-level YAML structure is:

```yaml
tests:
  - name: "Test Name"
    tags:
      - tag1
    files:
      - pattern: "**/*.cs"
        min: 1
        text:
          - contains: "Copyright"
```

### Design Decisions

- **Separation of data and domain objects**: The `FileAssertData` classes are pure data holders
  with no logic. The Modeling subsystem owns the domain objects built from them.
- **Forward-compatible deserialization**: `IgnoreUnmatchedProperties()` allows configuration
  files to contain keys introduced in later tool versions without causing parse failures.
- **Base directory from config path**: Resolving glob patterns relative to the configuration
  file location is more intuitive than the working directory, especially when the tool is
  invoked from a build script in a different directory.
