# Modeling Subsystem Design

## Overview

The Modeling subsystem contains the domain objects that represent a FileAssert test suite at
runtime. It transforms the data transfer objects produced by the Configuration subsystem into
executable domain objects and drives the assertion logic.

## Subsystem Contents

| Unit              | File                | Responsibility                                                  |
| :---------------- | :------------------ | :-------------------------------------------------------------- |
| `FileAssertTest`  | `FileAssertTest.cs` | Named test with file assertions and tag-based filter matching.  |
| `FileAssertFile`  | `FileAssertFile.cs` | Glob pattern matcher with count constraints and content rules.  |
| `FileAssertRule`  | `FileAssertRule.cs` | Abstract content validation rule hierarchy.                     |

## Subsystem Responsibilities

- Construct domain objects from Configuration DTOs via static factory methods.
- Validate required fields (test name, file pattern, rule type) during construction.
- Execute file glob matching using `Microsoft.Extensions.FileSystemGlobbing`.
- Enforce minimum and maximum file count constraints.
- Apply content rules to matched file text.
- Report assertion failures via the `Context` from the Cli subsystem.

## Object Hierarchy

```text
FileAssertTest
└── FileAssertFile (one or more)
    └── FileAssertRule (zero or more)
        ├── FileAssertContainsRule
        └── FileAssertMatchesRule
```

## Interactions with Other Subsystems

| Dependency    | Usage                                                    |
| :------------ | :------------------------------------------------------- |
| Cli           | Receives `Context` to report assertion failures.         |
| Configuration | Accepts DTO types for test, file, and rule construction. |

## Design Decisions

- **Factory methods over constructors**: Each domain class provides an `internal static Create`
  method that validates the DTO and constructs the domain object, keeping constructors private.
- **Error accumulation**: Failures are reported via `context.WriteError` rather than exceptions,
  so all assertions run to completion and all failures are visible in a single pass.
- **Glob via FileSystemGlobbing**: Uses the `Microsoft.Extensions.FileSystemGlobbing` library for
  cross-platform glob pattern evaluation consistent with the rest of the .NET ecosystem.
- **Compiled regex with timeout**: The `FileAssertMatchesRule` compiles its regex at construction
  time and applies a ten-second evaluation timeout to guard against catastrophic backtracking.
