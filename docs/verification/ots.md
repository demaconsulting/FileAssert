# OTS Verification

This section documents the verification evidence for off-the-shelf (OTS) software items used by
the FileAssert build pipeline. OTS items are third-party tools and frameworks that are not developed
in-house; each is verified by demonstrating that it performs its required functionality within the
FileAssert CI pipeline.

## OTS Items

The following OTS software items are used by FileAssert and are verified in this section:

- **BuildMark** — generates a markdown build-notes document from GitHub Actions workflow metadata,
  verified by CI pipeline success and downstream document validation.

- **FileAssert** — validates generated HTML and PDF documents for existence, size, structure, and
  content; verified by self-validation tests and transitive pipeline evidence.

- **Pandoc** — converts Markdown source documents to HTML as part of the documentation build;
  verified by FileAssert assertions on each generated HTML document.

- **ReqStream** — processes requirements and TRX test results to enforce requirement traceability;
  verified by `--enforce` mode passing with complete test coverage.

- **ReviewMark** — generates review plan and review report documents from the `.reviewmark.yaml`
  configuration; verified by Pandoc compilation and FileAssert PDF assertions.

- **SarifMark** — reads CodeQL SARIF output and renders a human-readable markdown code-quality
  report; verified by Pandoc compilation and FileAssert PDF assertions.

- **SonarMark** — retrieves quality-gate and metrics data from SonarCloud and renders a markdown
  quality report; verified by Pandoc compilation and FileAssert PDF assertions.

- **VersionMark** — captures tool-version metadata from CI jobs and publishes a versions markdown
  document; verified by its inclusion in the Build Notes compilation.

- **WeasyPrint** — converts HTML documents to PDF; verified by FileAssert assertions on each
  generated PDF document.

- **xUnit** — discovers and executes unit tests and writes TRX result files; verified by
  self-validation test scenarios that confirm test discovery, execution, and reporting.
