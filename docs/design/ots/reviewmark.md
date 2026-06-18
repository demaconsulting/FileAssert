## ReviewMark OTS Design

DemaConsulting.ReviewMark is a .NET dotnet global tool that reads a review configuration and a
review evidence store to generate a review plan and review report documenting formal file review
coverage.

### Purpose

ReviewMark provides continuous compliance evidence for formal code review. It reads the
`.reviewmark.yaml` configuration, which defines review-sets (named groups of files that must be
reviewed together), and a review evidence store to produce two Markdown documents: a review plan
that lists all files included in review-sets, and a review report that records which files have
been reviewed, by whom, and when.

ReviewMark is chosen because it integrates directly with the repository-level review evidence
pattern used by this project's Continuous Compliance methodology and produces Markdown output
compatible with the Pandoc pipeline.

### Features Used

- Review-set definition and file-coverage enforcement driven by the `.reviewmark.yaml` glob
  patterns under `needs-review` and `reviews`.
- Generation of a Markdown review plan via `dotnet reviewmark --plan <markdown-file>`.
- Generation of a Markdown review report via `dotnet reviewmark --report <markdown-file>`.
- Configuration linting via `dotnet reviewmark --lint` to validate the review configuration.
- URL-based review evidence retrieval: `.reviewmark.yaml` declares an `evidence-source` of
  `type: url` pointing at
  `https://raw.githubusercontent.com/demaconsulting/FileAssert/reviews/index.json`, so ReviewMark
  fetches the committed review records over the network rather than from a local directory.

### Integration Pattern

ReviewMark is installed as a .NET local tool defined in `.config/dotnet-tools.json` under the
package name `demaconsulting.reviewmark` and restored with `dotnet tool restore`. It reads its
configuration from `.reviewmark.yaml` at the repository root, which defines the review-sets and the
`evidence-source`.

Because the configured `evidence-source` is a URL on `raw.githubusercontent.com`, ReviewMark
requires outbound network access during its plan and report steps to fetch the review evidence
index; an environment without access to that host cannot retrieve current review records. This is a
deliberate change from a purely local evidence store and means the CI job running ReviewMark depends
on network availability.

The CI pipeline invokes ReviewMark in two separate steps, each producing a Markdown document that
Pandoc consumes to render the corresponding PDF:

- `dotnet reviewmark --plan docs/code_review_plan/generated/plan.md` — generates the review plan.
- `dotnet reviewmark --report docs/code_review_report/generated/report.md` — generates the review
  report.

ReviewMark has no transitive NuGet dependencies that propagate to the main source project; it is a
build-time tool only.
