# Introduction

This document provides the verification design for FileAssert, a .NET command-line tool that
validates document content in build pipelines.

## Purpose

The purpose of this document is to describe how each requirement for FileAssert is verified. For
every software item — system, subsystem, and unit — this document names the verification approach,
identifies the test scenarios (including boundary conditions and error paths), describes what is
mocked or stubbed, and maps each requirement to at least one named test scenario. The document does
not restate design; it explains how the design is proven correct.

## Scope

This document covers the verification design for the same software items described in the
*FileAssert Software Design Document*:

- **FileAssert** — the system as a whole
- **Program** — entry point and execution orchestrator
- **Cli** — command-line interface subsystem
  - **Context** — argument parser and I/O owner
  - **IContext** — output and error-reporting contract for assertion context
- **Configuration** — configuration loading subsystem
  - **FileAssertConfig** — configuration file reader
  - **FileAssertData** — deserialization data transfer objects
- **Modeling** — assertion execution subsystem
  - **FileAssertTest** — test definition and filter evaluation
  - **FileAssertFile** — file-pattern resolution and constraint checking
  - **FileAssertRule** — text rule evaluation
  - **FileAssertTextAssert** — plain-text content assertions
  - **FileAssertPdfAssert** — PDF structure and content assertions
  - **FileAssertXmlAssert** — XML XPath query assertions
  - **FileAssertHtmlAssert** — HTML XPath query assertions
  - **FileAssertYamlAssert** — YAML path query assertions
  - **FileAssertJsonAssert** — JSON path query assertions
  - **FileAssertZipAssert** — Zip archive entry assertions
- **Utilities** — shared utility subsystem
  - **PathHelpers** — safe path combination utilities
  - **TemporaryDirectory** — temporary workspace lifecycle utility
  - **IFileContainer** — uniform file-access abstraction over directories and zip archives
  - **DirectoryFileContainer** — filesystem-backed file container
  - **ZipFileContainer** — zip-archive-backed file container
- **SelfTest** — self-validation subsystem
  - **Validation** — self-validation test runner

The following topics are out of scope:

- Test infrastructure (xUnit framework, test helpers, Runner utility)
- Build pipeline and CI/CD configuration

The following OTS items are also covered:

- **BuildMark** — build-notes documentation tool
- **FileAssert** — document assertion tool (self-validation)
- **Pandoc** — Markdown-to-HTML conversion tool
- **ReqStream** — requirements traceability tool
- **ReviewMark** — file review enforcement tool
- **SarifMark** — SARIF report conversion tool
- **SonarMark** — SonarCloud quality report tool
- **VersionMark** — tool-version documentation tool
- **WeasyPrint** — HTML-to-PDF conversion tool
- **xUnit** — unit-testing framework
- **YamlDotNet** — YAML parsing and deserialization library
- **PdfPig** — PDF parsing library
- **HtmlAgilityPack** — HTML parsing library
- **FileSystemGlobbing** — glob pattern-matching library

## Software Structure

The following tree shows the software items covered by this document:

```text
FileAssert (System)
├── Program (Unit)
├── Cli (Subsystem)
│   ├── Context (Unit)
│   └── IContext (Unit)
├── Configuration (Subsystem)
│   ├── FileAssertConfig (Unit)
│   └── FileAssertData (Unit)
├── Modeling (Subsystem)
│   ├── FileAssertTest (Unit)
│   ├── FileAssertFile (Unit)
│   ├── FileAssertRule (Unit)
│   ├── FileAssertTextAssert (Unit)
│   ├── FileAssertPdfAssert (Unit)
│   ├── FileAssertXmlAssert (Unit)
│   ├── FileAssertHtmlAssert (Unit)
│   ├── FileAssertYamlAssert (Unit)
│   ├── FileAssertJsonAssert (Unit)
│   └── FileAssertZipAssert (Unit)
├── Utilities (Subsystem)
│   ├── PathHelpers (Unit)
│   ├── TemporaryDirectory (Unit)
│   ├── IFileContainer (Unit)
│   ├── DirectoryFileContainer (Unit)
│   └── ZipFileContainer (Unit)
└── SelfTest (Subsystem)
    └── Validation (Unit)

OTS Items
├── BuildMark
├── FileAssert
├── Pandoc
├── ReqStream
├── ReviewMark
├── SarifMark
├── SonarMark
├── VersionMark
├── WeasyPrint
├── xUnit
├── YamlDotNet
├── PdfPig
├── HtmlAgilityPack
└── FileSystemGlobbing
```

## Companion Artifact Structure

In-house items have corresponding artifacts in parallel directory trees:

- Requirements: `docs/reqstream/{system-name}.yaml`, `docs/reqstream/{system-name}/.../{item}.yaml`
- Design docs: `docs/design/{system-name}.md`, `docs/design/{system-name}/.../{item}.md`
- Verification design: `docs/verification/{system-name}.md`, `docs/verification/{system-name}/.../{item}.md`
- Source code: `src/{SystemName}/.../{Item}.cs` (PascalCase for C#)
- Tests: `test/{SystemName}.Tests/.../{Item}Tests.cs` (PascalCase for C#)

OTS items have parallel artifacts in:

- Requirements: `docs/reqstream/ots/{ots-name}.yaml`
- Design: `docs/design/ots/{ots-name}.md`
- Verification: `docs/verification/ots/{ots-name}.md`

Review-sets: defined in `.reviewmark.yaml`

## References

- [FileAssert releases](https://github.com/demaconsulting/FileAssert/releases) — compiled release
  artifacts for FileAssert, including the Verification Design Document PDF
