## ReqStream OTS Design

DemaConsulting.ReqStream is a .NET dotnet global tool that processes requirements YAML files and
TRX test-result files to generate requirements reports and enforce that every requirement is
linked to at least one passing test.

### Purpose

ReqStream reads the project's `requirements.yaml` root file — which includes all subsystem and
OTS requirements YAML files — together with the TRX files accumulated by the CI pipeline, and
produces three generated Markdown reports: a requirements report, a justifications document, and
a traceability matrix. When invoked with `--enforce`, ReqStream exits with a non-zero exit code
if any requirement lacks at least one passing test result, making incomplete traceability a
build-breaking condition.

ReqStream is chosen because it understands the project's YAML requirements format natively,
processes TRX files directly, and integrates with the `--enforce` flag as a quality gate without
requiring a separate CI plugin or external service.

### Integration

ReqStream is installed as a .NET local tool defined in `.config/dotnet-tools.json` under the
package name `demaconsulting.reqstream` and restored with `dotnet tool restore`. The CI pipeline
invokes it in the build-docs job after all test and self-validation TRX files have been
accumulated as workflow artifacts. Version constraint: `1.9.0` (pinned in
`.config/dotnet-tools.json`).

### Configuration

ReqStream is configured through the `requirements.yaml` root file, which uses an `includes:`
list to compose all subsystem and OTS requirements YAML files. Each YAML file defines a
hierarchy of sections and requirements where each requirement has an `id`, `title`,
`justification`, `tags`, and a list of `tests` that must pass. Requirements files are located
under `docs/reqstream/`.

### Interfaces

The project uses the following ReqStream command-line interface:

| Invocation                                                           | Effect                                         |
| :------------------------------------------------------------------- | :--------------------------------------------- |
| `dotnet reqstream --enforce <requirements.yaml> <trx-files...>`      | Generates reports; fails if coverage is broken |

The three generated Markdown files (`requirements.md`, `justifications.md`, `trace_matrix.md`)
are written to `docs/requirements/generated/` and consumed by Pandoc to produce the
Requirements PDF.

### Dependencies

ReqStream has no transitive NuGet dependencies that propagate to the main source project. It
requires TRX files produced by `dotnet test --logger trx` and by `fileassert --results`. The
tool operates entirely at the file system level and requires no network access.
