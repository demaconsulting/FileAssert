# Introduction

This document provides the detailed design for FileAssert — a .NET command-line application for
asserting file properties using YAML-defined test suites. It covers local software items (systems,
subsystems, and units), the OTS software items they consume, and the FileAssert shared package
consumed by this project's own build pipeline.

## Purpose

The purpose of this document is to define the design for each software item in FileAssert — full
architectural and detailed design for local items (systems, subsystems, and units), and integration
and usage design for OTS software items and the shared package. A reviewer should be able to
understand how each item satisfies its requirements without reading source code. The document does
not restate requirements; it explains how they are realized.

## Scope

This document covers the following software items:

Local items:

- **FileAssert**: system, subsystem, and unit design for all local components.

OTS items:

- **BuildMark**: integration and usage design.
- **FileSystemGlobbing**: integration and usage design.
- **HtmlAgilityPack**: integration and usage design.
- **Pandoc**: integration and usage design.
- **PdfPig**: integration and usage design.
- **ReqStream**: integration and usage design.
- **ReviewMark**: integration and usage design.
- **SarifMark**: integration and usage design.
- **SonarMark**: integration and usage design.
- **SysML2Tools**: integration and usage design.
- **VersionMark**: integration and usage design.
- **WeasyPrint**: integration and usage design.
- **XUnit**: integration and usage design.
- **YamlDotNet**: integration and usage design.

Shared packages:

- **FileAssert**: integration and usage design.

The following topics are out of scope:

- External library internals
- Build pipeline configuration
- Deployment and packaging
- Test projects

## Software Structure

The software structure is modeled in SysML2 under `docs/sysml2/` and rendered to the
diagram below by SysML2Tools as part of the build pipeline. AI agents should query the
SysML2 model directly (see the `sysml2tools-query` skill) rather than parsing this
diagram or the prose below.

![Software Structure](SoftwareStructureView.svg)

## Folder Layout

- **src/** - source files and projects
  - **DemaConsulting.FileAssert/** - FileAssert system source
    - **Cli/** - Cli subsystem
    - **Configuration/** - Configuration subsystem
    - **Modeling/** - Modeling subsystem
    - **Utilities/** - Utilities subsystem
    - **SelfTest/** - SelfTest subsystem

## Document Conventions

Throughout this document:

- Class names, method names, property names, and file names appear in `monospace` font.
- The word **shall** denotes a design constraint that the implementation must satisfy.
- Section headings within each unit chapter follow a consistent structure: overview, data model,
  methods/algorithms, and interactions with other units.
- Text tables are used in preference to diagrams, which may not render in all PDF viewers.

## Companion Artifact Structure

Each in-house software item has corresponding artifacts in parallel directory trees:

- Requirements: `docs/reqstream/{system-name}.yaml`, `docs/reqstream/{system-name}/.../{item}.yaml`
- Design docs: `docs/design/{system-name}.md`, `docs/design/{system-name}/.../{item}.md`
- Verification: `docs/verification/{system-name}.md`, `docs/verification/{system-name}/.../{item}.md`
- Source code: `src/{SystemName}/.../{Item}.cs` (PascalCase for C#)
- Tests: `test/{SystemName}.Tests/.../{Item}Tests.cs` (PascalCase for C#)

OTS items have integration/usage design docs at `docs/design/ots/{ots-name}.md` describing how
FileAssert integrates the third-party library; their artifacts sit parallel to system folders:

- Requirements: `docs/reqstream/ots/{ots-name}.yaml`
- Design: `docs/design/ots/{ots-name}.md`
- Verification: `docs/verification/ots/{ots-name}.md`

Shared packages (earlier releases of the same program's own packages consumed in CI) have
integration/usage design docs at `docs/design/shared/{package-name}.md`; their artifacts also
sit parallel to system and OTS folders:

- Requirements: `docs/reqstream/shared/{package-name}.yaml`
- Design: `docs/design/shared/{package-name}.md`
- Verification: `docs/verification/shared/{package-name}.md`

Review-sets: defined in `.reviewmark.yaml`

## References

- FileAssert User Guide — the `README.md` document at the root of the FileAssert repository.
- FileAssert Repository — the `demaconsulting/FileAssert` source repository hosted on GitHub.
