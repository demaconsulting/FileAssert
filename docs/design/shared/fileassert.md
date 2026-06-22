## FileAssert Shared Package Design

DemaConsulting.FileAssert is the tool developed by this project. An earlier released version of it
is consumed within this project's own CI pipeline to validate generated documents and to produce
self-validation evidence. Because this repository uses its own published package, it is classified
as a **Shared Package** rather than OTS.

### Advertised Features Consumed

The following advertised features of the released FileAssert package are consumed by this project,
each corresponding to a requirement in the FileAssert Shared Package Software Requirements:

| Requirement                            | Feature Consumed                                               |
| :------------------------------------- | :------------------------------------------------------------- |
| `FileAssert-Shared-FileAssert-Results` | `--results <file>` writes TRX test result files for ReqStream  |
| `FileAssert-Shared-FileAssert-File`    | `count:` assertion confirms file existence via glob patterns   |
| `FileAssert-Shared-FileAssert-Text`    | `text: contains:` asserts text content of generated HTML files |
| `FileAssert-Shared-FileAssert-Html`    | `html: query:` asserts HTML document structure via XPath       |
| `FileAssert-Shared-FileAssert-Pdf`     | `pdf:` asserts PDF metadata fields, page count, and body text  |

### Integration Pattern

FileAssert is installed as a .NET local tool defined in `.config/dotnet-tools.json` under the
package name `demaconsulting.fileassert` and restored with `dotnet tool restore` at the start of
each CI job. No wrapper code is written; the tool is invoked directly via two command patterns:

- `dotnet fileassert --validate --results <trx-file>` — runs the built-in self-validation suite
  and writes TRX output consumed by ReqStream to satisfy the shared-package requirements.
- `dotnet fileassert --config <file> --results <trx-file>` — runs document assertions defined in
  `.fileassert.yaml` YAML configuration files checked in alongside the generated documents.

A non-zero exit code from either invocation causes the CI job to fail immediately.

### Assumptions

- The released package's self-validation test names (`FileAssert_Results`, `FileAssert_File`,
  `FileAssert_Text`, `FileAssert_Html`, `FileAssert_Pdf`) remain stable across patch versions and
  are present in the TRX output produced by `--validate`.
- The `--results` flag produces a valid TRX file parseable by ReqStream.
- The `count:`, `text:`, `html:`, and `pdf:` YAML assertion keys behave as documented in the
  FileAssert User Guide for the pinned version.
- The tool exits non-zero when any assertion fails, causing CI to fail immediately.
