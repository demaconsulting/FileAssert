## VersionMark OTS Design

DemaConsulting.VersionMark is a .NET dotnet global tool that captures installed tool version
information during each CI job and publishes it as a Markdown document included in the Build
Notes PDF artifact.

### Purpose

VersionMark provides an audit record of the exact tool versions used to produce each release.
Each CI job captures the versions of the dotnet tools and runtime components it uses; the
build-docs job then merges all captured data and publishes a Markdown document included in the
Build Notes artifact. This supports reproducibility and compliance traceability for all build
tools.

VersionMark is chosen because it operates as a local dotnet tool alongside the other pipeline
tools, requires no external service, and produces Markdown output compatible with the Pandoc
pipeline.

### Integration

VersionMark is installed as a .NET local tool defined in `.config/dotnet-tools.json` under the
package name `demaconsulting.versionmark` and restored with `dotnet tool restore`. It is used
in two modes in the CI pipeline:

- **Capture mode** (each CI job): invoked with `--capture --job-id <job> --output <json>` to
  interrogate installed tool versions and write a JSON file to the artifacts folder.
- **Publish mode** (build-docs job): invoked with `--publish <json-files...> --output <md>` to
  merge all captured JSON files and write the consolidated Markdown versions document.
- **Self-validation**: invoked with `--validate --results <trx-file>` to run the built-in
  validation suite and write TRX evidence for ReqStream consumption.

Version constraint: `1.3.0` (pinned in `.config/dotnet-tools.json`).

### Configuration

VersionMark is configured entirely through command-line arguments. In capture mode, `--job-id`
identifies the CI job and `--output` specifies the JSON output path. In publish mode, the input
JSON files and the output Markdown path are provided as positional and named arguments
respectively. No external configuration files are required.

### Interfaces

The project uses the following VersionMark command-line interfaces:

| Invocation                                                               | Effect                                       |
| :----------------------------------------------------------------------- | :------------------------------------------- |
| `dotnet versionmark --capture --job-id <id> --output <json-file>`        | Captures tool versions to a JSON file        |
| `dotnet versionmark --publish <json-files...> --output <markdown-file>`  | Publishes merged version info as Markdown    |
| `dotnet versionmark --validate --results <trx-file>`                     | Runs self-validation and writes TRX evidence |

The generated Markdown file is included in the Build Notes document and consumed by Pandoc.

### Dependencies

VersionMark has no transitive NuGet dependencies that propagate to the main source project. It
reads installed tool metadata from the local environment and writes JSON and Markdown files. No
network access is required.
