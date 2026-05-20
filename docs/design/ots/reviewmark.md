## ReviewMark OTS Design

DemaConsulting.ReviewMark is a .NET dotnet global tool that reads a review configuration and
evidence store to generate a review plan and review report documenting formal file review
coverage.

### Purpose

ReviewMark provides continuous compliance evidence for formal code review. It reads the
`.reviewmark.yaml` configuration, which defines review-sets (named groups of files that must be
reviewed together), and the review evidence store to produce two Markdown documents: a review
plan that lists all files included in review-sets, and a review report that records which files
have been reviewed, by whom, and when.

ReviewMark is chosen because it integrates directly with the repository-level review evidence
pattern used by this project's Continuous Compliance methodology and produces Markdown output
compatible with the Pandoc pipeline.

### Integration

ReviewMark is installed as a .NET local tool defined in `.config/dotnet-tools.json` under the
package name `demaconsulting.reviewmark` and restored with `dotnet tool restore`. The CI
pipeline invokes ReviewMark in two separate steps:

- `dotnet reviewmark --plan docs/code_review_plan/generated/plan.md` — generates the review plan
- `dotnet reviewmark --report docs/code_review_report/generated/report.md` — generates the review
  report

### Configuration

ReviewMark reads its configuration from `.reviewmark.yaml` at the repository root. This file
defines review-sets, each consisting of a name, description, and a list of file glob patterns
identifying the files that belong to the set. The review evidence store, which contains committed
review records in the repository, provides the historical evidence that ReviewMark uses to
determine which files have been reviewed and when.

### Interfaces

The project uses the following ReviewMark command-line interfaces:

| Invocation                                         | Effect                                    |
| :------------------------------------------------- | :---------------------------------------- |
| `dotnet reviewmark --plan <markdown-file>`         | Generates a Markdown review plan          |
| `dotnet reviewmark --report <markdown-file>`       | Generates a Markdown review report        |

Both generated Markdown files are consumed by Pandoc to produce the Review Plan PDF and the
Review Report PDF, respectively.

### Dependencies

ReviewMark has no transitive NuGet dependencies that propagate to the main source project. It
reads the `.reviewmark.yaml` configuration and the repository's review evidence directory. No
network access is required.
