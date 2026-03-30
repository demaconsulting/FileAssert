# Introduction

This document provides the detailed design for the FileAssert tool, a .NET command-line application
for asserting file properties using YAML-defined test suites.

## Purpose

The purpose of this document is to describe the internal design of each software unit that comprises
FileAssert. It captures data models, algorithms, key methods, and inter-unit interactions at a level
of detail sufficient for formal code review, compliance verification, and future maintenance. The
document does not restate requirements; it explains how they are realized.

## Scope

This document covers the detailed design of the following software units:

- **Program** — entry point and execution orchestrator (`Program.cs`)
- **Context** — command-line argument parser and I/O owner (`Context.cs`)
- **FileAssertConfig** — top-level configuration loader and test runner (`FileAssertConfig.cs`)
- **FileAssertData** — YAML data transfer objects for configuration deserialization (`FileAssertData.cs`)
- **FileAssertTest** — named test with file assertions and tag filtering (`FileAssertTest.cs`)
- **FileAssertFile** — glob pattern matcher with count constraints and content rules (`FileAssertFile.cs`)
- **FileAssertRule** — abstract content validation rule hierarchy (`FileAssertRule.cs`)
- **PathHelpers** — safe path-combination utility (`PathHelpers.cs`)
- **Validation** — self-validation test runner (`Validation.cs`)

The following topics are out of scope:

- External library internals (YamlDotNet, Microsoft.Extensions.FileSystemGlobbing,
  DemaConsulting.TestResults)
- Build pipeline configuration
- Deployment and packaging

## Software Structure

The following tree shows how the FileAssert software items are organized across the system,
subsystem, and unit levels:

```text
FileAssert (System)
├── Program (Unit)
├── Cli (Subsystem)
│   └── Context (Unit)
├── Configuration (Subsystem)
│   ├── FileAssertConfig (Unit)
│   └── FileAssertData (Unit)
├── Modeling (Subsystem)
│   ├── FileAssertTest (Unit)
│   ├── FileAssertFile (Unit)
│   └── FileAssertRule (Unit)
├── Utilities (Subsystem)
│   └── PathHelpers (Unit)
└── SelfTest (Subsystem)
    └── Validation (Unit)
```

Each unit is described in detail in its own chapter within this document.

## Folder Layout

The source code folder structure mirrors the top-level subsystem breakdown above, giving
reviewers an explicit navigation aid from design to code:

```text
src/DemaConsulting.FileAssert/
├── Program.cs                      — entry point and execution orchestrator
├── Cli/
│   └── Context.cs                  — command-line argument parser and I/O owner
├── Configuration/
│   ├── FileAssertConfig.cs         — top-level configuration loader and test runner
│   └── FileAssertData.cs           — YAML data transfer objects
├── Modeling/
│   ├── FileAssertTest.cs           — named test with file assertions and tag filtering
│   ├── FileAssertFile.cs           — glob pattern matcher with count constraints and rules
│   └── FileAssertRule.cs           — abstract content validation rule hierarchy
├── Utilities/
│   └── PathHelpers.cs              — safe path-combination utility
└── SelfTest/
    └── Validation.cs               — self-validation test runner
```

The test project mirrors the same layout under `test/DemaConsulting.FileAssert.Tests/`.

## Document Conventions

Throughout this document:

- Class names, method names, property names, and file names appear in `monospace` font.
- The word **shall** denotes a design constraint that the implementation must satisfy.
- Section headings within each unit chapter follow a consistent structure: overview, data model,
  methods/algorithms, and interactions with other units.
- Text tables are used in preference to diagrams, which may not render in all PDF viewers.

## References

- [FileAssert User Guide][guide]
- [FileAssert Repository][repo]

[guide]: ../../README.md
[repo]: https://github.com/demaconsulting/FileAssert
