## WeasyPrint OTS Design

DemaConsulting.WeasyPrintTool is a .NET dotnet global tool wrapper around the WeasyPrint Python
library. It converts HTML documents produced by Pandoc into PDF artifacts for release
distribution and compliance archiving.

### Purpose

WeasyPrint renders the HTML output produced by Pandoc into PDF documents for all document
collections: Build Notes, Code Quality, Review Plan, Review Report, Design, Verification, and
User Guide. PDF output enables FileAssert to assert PDF metadata and content during CI
validation, and provides a format suitable for long-term archivability and distribution to
reviewers.

WeasyPrint is chosen because it applies CSS-based page layout, supports custom fonts and headers
via the shared `docs/template/` CSS, and is available as a dotnet global tool wrapper that
integrates with the existing pipeline without requiring a separate Python invocation step.

### Integration

WeasyPrint is installed as a .NET local tool via the package `demaconsulting.weasyprinttool` in
`.config/dotnet-tools.json` and restored with `dotnet tool restore`. The tool is invoked as
`dotnet weasyprint` with the input HTML path and the output PDF path for each document
collection. The CI workflow installs Python via `actions/setup-python` to satisfy the
WeasyPrintTool's internal Python dependency. Version constraint: `68.1.0` (pinned in
`.config/dotnet-tools.json`).

### Configuration

WeasyPrint is configured entirely through command-line arguments: the input HTML file path and
the output PDF file path. The HTML files produced by Pandoc embed the shared CSS from
`docs/template/`, which WeasyPrint uses to apply page layout, fonts, and document styling. No
separate WeasyPrint configuration files are used in this project.

### Interfaces

The project uses the following WeasyPrint command-line interface:

| Invocation                                                  |
| :---------------------------------------------------------- |
| `dotnet weasyprint --input <html-file> --output <pdf-file>` |

The generated PDF files are validated by FileAssert assertions (`WeasyPrint_BuildNotesPdf`,
`WeasyPrint_CodeQualityPdf`, `WeasyPrint_ReviewPlanPdf`, `WeasyPrint_ReviewReportPdf`,
`WeasyPrint_DesignPdf`, `WeasyPrint_VerificationPdf`, `WeasyPrint_UserGuidePdf`) and uploaded
as release artifacts.

### Dependencies

WeasyPrintTool requires Python to be available in the CI environment. The GitHub Actions
workflow installs Python via `actions/setup-python`. The WeasyPrint Python library and its
CSS rendering dependencies (Pango, Cairo, fontconfig) must be present in the runner image.
No WeasyPrint NuGet dependencies are propagated to the main source project or the published
NuGet package.
