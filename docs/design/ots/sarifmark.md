## SarifMark OTS Design

DemaConsulting.SarifMark is a .NET dotnet global tool that reads SARIF (Static Analysis Results
Interchange Format) files produced by CodeQL code scanning and renders them as a human-readable
Markdown report included in the Code Quality PDF artifact.

### Purpose

SarifMark converts the CodeQL SARIF output into a Markdown document that provides a persistent,
human-readable record of any security findings identified by CodeQL for each release. This
document is compiled into the Code Quality PDF artifact alongside the SonarCloud quality report,
giving reviewers a unified view of static analysis results.

SarifMark is chosen because it understands the SARIF format natively and produces Markdown
output compatible with the Pandoc pipeline, requiring no custom scripting to transform CodeQL
output into the document format.

### Integration

SarifMark is installed as a .NET local tool defined in `.config/dotnet-tools.json` under the
package name `demaconsulting.sarifmark` and restored with `dotnet tool restore`. The CI pipeline
invokes SarifMark in the build-docs job after the CodeQL scanning step has completed and the
SARIF file has been downloaded as a workflow artifact.

### Configuration

SarifMark is configured entirely through command-line arguments: the SARIF input file path and
the output Markdown file path. No additional configuration files are required. The SARIF file is
produced by the `github/codeql-action/analyze` GitHub Actions step, downloaded as a workflow
artifact, and passed directly to SarifMark.

### Interfaces

The project uses the following SarifMark command-line interface:

| Invocation                                                   | Effect                                       |
| :----------------------------------------------------------- | :------------------------------------------- |
| `dotnet sarifmark --sarif <sarif-file> --output <md-file>`   | Converts SARIF results to a Markdown report  |

The generated Markdown file is written to `docs/code_quality/generated/codeql-quality.md` and
consumed by Pandoc to produce the Code Quality HTML and subsequently the Code Quality PDF.

### Dependencies

SarifMark has no transitive NuGet dependencies that propagate to the main source project. It
requires the SARIF file produced by CodeQL as input and produces a standalone Markdown file as
output. No network access is required.
