## VersionMark Verification

This document provides the verification evidence for the `VersionMark` OTS software item.

### Required Functionality

DemaConsulting.VersionMark captures version metadata for each dotnet tool used in the pipeline
via `--capture`, then publishes that metadata as a versions markdown document via `--publish`.
The published document is included in the Build Notes release artifact.

### Verification Approach

VersionMark is verified by two complementary layers of evidence. Each CI job runs
`versionmark --capture` to collect tool-version JSON files, and the build-docs job runs
`versionmark --publish` to produce `docs/build_notes/generated/versions.md`. This file is included
in the Build Notes document compiled by Pandoc. If VersionMark failed to produce the versions
document, the Build Notes compilation would be incomplete. WeasyPrint renders the result to PDF
and FileAssert asserts its content (`WeasyPrint_BuildNotesPdf`). A CI build failure at any step is
evidence that VersionMark did not execute correctly.

### Test Scenarios

#### VersionMark_CapturesVersions

**Scenario**: VersionMark reads version metadata for each dotnet tool defined in the pipeline.

**Expected**: Exits 0 and captures version data for every tool.

#### VersionMark_GeneratesMarkdownReport

**Scenario**: VersionMark writes a versions markdown document to the release artifacts.

**Expected**: Exits 0 and produces a non-empty versions markdown file.

### Acceptance Criteria

N/A - Acceptance criteria are managed at the system integration level. This OTS item is
considered verified when the integration test scenarios that exercise its functionality
pass in the CI pipeline.
