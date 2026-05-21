## BuildMark OTS Design

DemaConsulting.BuildMark is a .NET dotnet global tool that queries the GitHub Actions API to
capture workflow run details and renders them as a Markdown build-notes document included in
the release artifacts.

### Purpose

BuildMark provides an automated build-notes report for each CI pipeline run. It captures the
GitHub Actions workflow run details — including the workflow name, run number, trigger, and
associated commit — and renders them as a Markdown document. This document is compiled into the
Build Notes PDF artifact, giving reviewers a permanent record of the build provenance for each
release.

BuildMark is chosen because it integrates directly with the GitHub Actions event context,
requiring no manual input, and produces Markdown that is compatible with the Pandoc pipeline
already used for all other document collections.

### Integration

BuildMark is installed as a .NET local tool defined in `.config/dotnet-tools.json` under the
package name `demaconsulting.buildmark` and restored with `dotnet tool restore`. The tool is
invoked in the CI pipeline's build-docs job with the `--output` argument pointing to the
generated Markdown path. The generated file is placed in
`docs/build_notes/generated/build_notes.md`, which Pandoc incorporates into the Build Notes
HTML document.

### Configuration

BuildMark requires the GitHub Actions environment variables `GITHUB_TOKEN`, `GITHUB_RUN_ID`,
`GITHUB_REPOSITORY`, and `GITHUB_SERVER_URL`, which GitHub Actions provides automatically in
every job. No additional configuration files are used; all options are supplied as command-line
arguments.

### Interfaces

The project uses the following BuildMark command-line interface:

| Invocation                                  | Effect                                                            |
| :------------------------------------------ | :---------------------------------------------------------------- |
| `dotnet buildmark --output <markdown-file>` | Queries the GitHub API and writes a Markdown build-notes document |

The output Markdown file is the sole artifact consumed downstream. No programmatic API or SDK
is used; all interaction occurs through the command-line interface.

### Dependencies

BuildMark operates as an isolated tool process. Its internal dependencies do not propagate to
the main source project or the published NuGet package. The tool requires network access to the
GitHub REST API during the CI job that invokes it. The generated Markdown file has no runtime
dependency on BuildMark after it is produced.
