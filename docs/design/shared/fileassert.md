## FileAssert Shared Package Design

DemaConsulting.FileAssert is the tool developed by this project. An earlier released version of it
is also consumed within this project's own CI pipeline to validate generated documents and to
produce self-validation evidence. Because this repository uses its own published package, it is
classified as a **Shared Package** rather than OTS.

### Purpose

FileAssert validates HTML and PDF documents produced during the build pipeline, asserting that
each document exists, has a non-trivial size, is structurally valid, and contains expected
content. Its built-in self-validation suite (`--validate`) is run as a CI step to produce TRX
test evidence satisfying the `FileAssert-Shared-FileAssert` requirement.

FileAssert is chosen because it directly implements the project's own document-assertion
capability, making its CI use a natural dogfooding exercise that simultaneously validates the
tool and provides compliance evidence for the document pipeline.

### Integration

FileAssert is installed as a .NET local tool defined in `.config/dotnet-tools.json` under the
package name `demaconsulting.fileassert` and restored with `dotnet tool restore`. The CI
pipeline uses it in two ways:

- **Self-validation**: `fileassert --validate --results artifacts/fileassert-self-validation.trx`
  runs the built-in self-validation suite and writes TRX output for ReqStream consumption.
- **Document assertion**: `fileassert --config <file> --results <trx-file>` validates specific
  generated HTML and PDF files throughout the pipeline using YAML configuration files.

### Configuration

When used for document validation, FileAssert reads `.fileassert.yaml` configuration files that
define the glob patterns and acceptance criteria for each document type. These YAML files are
checked in alongside the documents they validate. When run with `--validate`, no configuration
file is required; the built-in test suite is used.

### Interfaces

The project uses the following FileAssert command-line interfaces:

| Invocation                                                   | Effect                                             |
| :----------------------------------------------------------- | :------------------------------------------------- |
| `dotnet fileassert --validate --results <trx-file>`          | Runs self-validation and writes TRX results        |
| `dotnet fileassert --config <file> --results <trx-file>`     | Runs document assertions from a YAML configuration |

The TRX output from `--results` is consumed by ReqStream to satisfy `FileAssert-Shared-FileAssert`.

### Dependencies

FileAssert operates as an isolated tool process. Its NuGet dependencies — PdfPig,
HtmlAgilityPack, YamlDotNet, Microsoft.Extensions.FileSystemGlobbing, and
DemaConsulting.TestResults — are bundled within the tool and do not affect the main project's
dependency graph or the published NuGet package.
