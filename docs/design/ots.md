# OTS Integration Design

FileAssert relies on ten off-the-shelf (OTS) tools that support CI/CD automation, documentation
generation, and compliance checking. This chapter describes the overall OTS integration strategy
and introduces each tool's role in the pipeline.

## Integration Strategy

The OTS items used by this project fall into three functional groups:

- **Build pipeline tools** — dotnet global tools installed via `.config/dotnet-tools.json` and
  invoked by the GitHub Actions workflow. Each tool performs a distinct step such as capturing
  build metadata, asserting document correctness, or publishing compliance reports.
- **Test framework** — the xUnit NuGet packages referenced by the test project and consumed
  automatically by `dotnet test`.
- **Document generation** — Pandoc converts Markdown to HTML and WeasyPrint converts HTML to
  PDF; both are installed as dotnet global tools via `.config/dotnet-tools.json`.

All dotnet global tools are pinned to specific versions in `.config/dotnet-tools.json` and
restored with `dotnet tool restore` at the beginning of each CI job. This ensures reproducible
builds and provides an audit record of which tool version produced each release artifact.

## OTS Item Summary

| OTS Item    | Role                                                                         |
| :---------- | :--------------------------------------------------------------------------- |
| BuildMark   | Generates build-notes documentation from GitHub Actions metadata             |
| FileAssert  | Validates generated HTML and PDF documents against acceptance criteria       |
| Pandoc      | Converts Markdown source documents to HTML for each document collection      |
| ReqStream   | Enforces requirements traceability against TRX test-result files             |
| ReviewMark  | Generates review plan and review report from the review configuration        |
| SarifMark   | Converts CodeQL SARIF results into a human-readable Markdown report          |
| SonarMark   | Generates a SonarCloud quality and security metrics report                   |
| VersionMark | Captures and publishes tool-version information for each CI job              |
| WeasyPrint  | Converts HTML documents to PDF for release artifact archiving                |
| xUnit       | Discovers, executes, and reports unit tests; produces TRX output             |

## Per-Item Design

Detailed design for each OTS item is provided in the following sections of this document:

- See _BuildMark OTS Design_ for the build-notes generation tool.
- See _FileAssert OTS Design_ for the document assertion tool.
- See _Pandoc OTS Design_ for the Markdown-to-HTML conversion tool.
- See _ReqStream OTS Design_ for the requirements traceability enforcement tool.
- See _ReviewMark OTS Design_ for the review plan and report generation tool.
- See _SarifMark OTS Design_ for the CodeQL SARIF report tool.
- See _SonarMark OTS Design_ for the SonarCloud quality report tool.
- See _VersionMark OTS Design_ for the tool-version capture and publish tool.
- See _WeasyPrint OTS Design_ for the HTML-to-PDF conversion tool.
- See _xUnit OTS Design_ for the unit-testing framework.

## Selection Criteria

OTS items are selected for use in this project against the following criteria:

- **License compatibility**: the item's license must permit use in the build pipeline and, where
  applicable, distribution of the produced artifacts, without imposing reciprocal licensing
  requirements on this project's source code.
- **Active maintenance**: the item must have recent release activity and an active public issue
  tracker, indicating ongoing support and timely security responses.
- **Community adoption**: preference is given to items with broad .NET or general-purpose community
  adoption, which reduces abandonment risk and simplifies onboarding for contributors.
- **Security track record**: critical vulnerabilities must be publicly disclosed and patched in a
  timely manner by the vendor or maintainer.
- **Functional fit**: the item must address the required capability with a documented API and without
  requiring significant wrapper code to adapt its interface.

## Version Management Policy

All dotnet global tools are pinned to specific versions in `.config/dotnet-tools.json` and restored
with `dotnet tool restore` at the start of each CI job. This ensures reproducible builds and provides
an audit record of which tool version produced each release artifact.

OTS items that are NuGet package dependencies are managed via the project file (`*.csproj`). Version
changes are reviewed in pull requests and must be accompanied by a passing build and full test suite
before merging.

Version numbers are not recorded in design documentation — they are managed in SBOMs and the tool
manifest outside of the design artifact set.

## General Integration Approach

OTS items in this project fall into two integration categories:

- **dotnet global tools** (BuildMark, FileAssert, Pandoc, ReqStream, ReviewMark, SarifMark,
  SonarMark, VersionMark, WeasyPrint) — installed globally in the CI environment via
  `dotnet tool restore` from `.config/dotnet-tools.json` and invoked as command-line executables
  within GitHub Actions workflow steps. No wrapper code is written; tools are invoked directly
  with documented command-line flags. A non-zero exit code from any tool step causes the CI job
  to fail immediately, consistent with the GitHub Actions default `fail-fast` behaviour.
- **NuGet package** (xUnit) — referenced in the test project file and consumed through standard
  .NET package restore. No explicit initialisation or configuration code is required beyond the
  test-project target framework declaration.

Error handling across all tool invocations relies on exit-code contracts documented in each tool's
own user guide. No custom error-recovery logic is applied.

## Qualification Strategy

OTS items are qualified for use in this project through a combination of the following approaches:

- **Vendor self-validation**: all OTS tools publish their own test suites and release notes. Vendor
  evidence is reviewed before a major version upgrade is accepted into this project.
- **Pipeline integration tests**: for OTS tools whose outputs are consumed by downstream steps
  (e.g., Pandoc HTML output validated by FileAssert assertions, ReqStream traceability checked
  against TRX results), the CI pipeline acts as an integration harness — a tool that produces
  incorrect output will cause a downstream step to fail.
- **Local unit tests** (xUnit): the xUnit framework is implicitly qualified by the passing of all
  unit and integration tests in each CI run. A test framework that cannot discover, execute, or
  report tests correctly would cause the CI pipeline to fail before any artifacts are published.
- **Release-note review**: before accepting any OTS version upgrade, the team reviews the vendor
  release notes for breaking changes, deprecations, and security advisories.
