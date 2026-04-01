# FileAssertConfig Design

## Overview

The `FileAssertConfig` class is the top-level entry point for the FileAssert tool's
main execution path. It loads a YAML configuration file, builds the full hierarchy
of tests, files, and rules, and drives test execution with optional name or tag
filtering.

## Class Structure

### Properties

| Property | Type                            | Description                              |
| :------- | :------------------------------ | :--------------------------------------- |
| `Tests`  | `IReadOnlyList<FileAssertTest>` | Tests defined in the configuration file. |

### Factory Method

```csharp
internal static FileAssertConfig ReadFromFile(string path)
```

The factory:

1. Validates that `path` is not null.
2. Throws `FileNotFoundException` if the file does not exist, allowing the caller
   to distinguish between a missing default configuration and a missing explicitly
   specified file.
3. Reads the YAML text and deserializes it using `YamlDotNet` with
   `IgnoreUnmatchedProperties()` to allow forward-compatible configuration files
   that contain future keys not yet understood by this version of the tool.
4. Converts each `FileAssertTestData` entry to a `FileAssertTest` via
   `FileAssertTest.Create`.
5. Stores the config file path internally so the base directory can be resolved
   at run time.

### Execution Method

```csharp
internal void Run(Context context, IEnumerable<string> filters)
```

Execution proceeds as follows:

1. The base directory is derived from the absolute path of the configuration file
   using `Path.GetDirectoryName(Path.GetFullPath(_configPath))`. This ensures that
   glob patterns in file assertions are resolved relative to the configuration file
   location, not the working directory.
2. The `filters` collection is materialized to avoid multiple enumeration.
3. Each test that satisfies `MatchesFilter(filterList)` is executed in order.

## YAML Configuration Format

The top-level YAML structure is:

```yaml
tests:
  - name: "Test Name"
    tags:
      - tag1
      - tag2
    files:
      - pattern: "**/*.cs"
        min: 1
        rules:
          - contains: "Copyright"
```

## Integration with Program

`Program.RunToolLogic` loads the configuration from the path stored in
`context.ConfigFile` (defaulting to `.fileassert.yaml`) and passes
`context.Filters` to `Run`. When the default configuration file is absent,
the tool prints usage guidance rather than an error. When an explicitly specified
file is absent, the tool reports an error and exits with a non-zero code.

## Design Decisions

- **Static factory with stored path**: Storing the configuration file path in the
  object avoids threading the path through every method signature while still
  allowing the base directory to be resolved correctly at run time.
- **IgnoreUnmatchedProperties**: Forward-compatible deserialization prevents the
  tool from failing when configuration files contain keys introduced in later
  versions.
- **Base directory from config path**: Resolving glob patterns relative to the
  configuration file location is more intuitive than using the working directory,
  particularly when the tool is invoked from a build script in a different directory.
