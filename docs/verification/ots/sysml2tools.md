## SysML2Tools Verification

This document provides the verification evidence for the `SysML2Tools` OTS software item.
Requirements for this OTS item are defined in the SysML2Tools OTS Software Requirements document.

### Required Functionality

DemaConsulting.SysML2Tools validates the SysML2 architecture model under `docs/sysml2/` for syntax
and reference errors, and renders each view declared in `docs/sysml2/views/design-views.sysml` to
an SVG diagram embedded in the design documentation. Both behaviors run in the same CI pipeline
that produces the compiled Design document, so a successful pipeline run is evidence that
SysML2Tools executed without error.

### Verification Approach

SysML2Tools is verified by two complementary layers of evidence:

- **Lint evidence**: `lint.ps1` runs `dotnet sysml2tools lint 'docs/sysml2/**/*.sysml'` and fails
  the build on any syntax or reference error. A passing lint run is evidence that the model is
  well-formed.
- **Render evidence**: the build-docs job runs `dotnet sysml2tools render` to produce one SVG file
  per declared view under `docs/design/generated/`. Pandoc then compiles `docs/design/*.md`, which
  embed those SVG files by filename; if a declared view failed to render, the missing image
  reference would cause a broken image in the compiled Design HTML and PDF. WeasyPrint renders the
  result to PDF and FileAssert asserts its content (`WeasyPrint_DesignPdf`). A CI build failure at
  either step is evidence that SysML2Tools did not produce the required model validation or
  diagrams.

### Test Scenarios

#### SysML2Tools_ModelLint

**Scenario**: SysML2Tools is invoked with `lint` against every `.sysml` file under `docs/sysml2/`.

**Expected**: Exits 0 with no reported syntax or reference errors.

#### SysML2Tools_ViewRender

**Scenario**: SysML2Tools is invoked with `render` against the model and `design-views.sysml`,
producing one SVG file per declared view.

**Expected**: Exits 0 and produces a non-empty SVG file for every view declared in
`design-views.sysml`.

### Acceptance Criteria

N/A - Acceptance criteria are managed at the system integration level. This OTS item is considered
verified when the integration test scenarios that exercise its functionality pass in the CI
pipeline.
