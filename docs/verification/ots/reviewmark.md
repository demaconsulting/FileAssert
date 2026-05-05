## ReviewMark Verification

This document provides the verification evidence for the ReviewMark OTS software item. Requirements
for this OTS item are defined in the ReviewMark OTS Software Requirements document.

### Required Functionality

DemaConsulting.ReviewMark reads the `.reviewmark.yaml` configuration and the review evidence store
to produce a review plan and review report documenting file review coverage and currency. It runs in
the same CI pipeline that produces the TRX test results, so a successful pipeline run is evidence
that ReviewMark executed without error.

### Verification Approach

ReviewMark is verified by the CI pipeline invoking ReviewMark to generate
`docs/code_review_plan/generated/plan.md` and `docs/code_review_report/generated/report.md`.
Pandoc converts each to HTML; if either file were absent or malformed, Pandoc would fail.
WeasyPrint renders both to PDF and FileAssert asserts their content
(`WeasyPrint_ReviewPlanPdf`, `WeasyPrint_ReviewReportPdf`). A CI build failure at any step is
evidence that ReviewMark did not produce the required review documents.

### Test Scenarios

#### ReviewMark_ReviewPlanGeneration

**Scenario**: ReviewMark is invoked with `--plan` to generate a review plan from the
`.reviewmark.yaml` configuration.

**Expected**: Exits 0 and produces a non-empty review plan markdown file.

**Requirement coverage**: `FileAssert-OTS-ReviewMark`.

#### ReviewMark_ReviewReportGeneration

**Scenario**: ReviewMark is invoked with `--report` to generate a review report from the
`.reviewmark.yaml` configuration and evidence store.

**Expected**: Exits 0 and produces a non-empty review report.

**Requirement coverage**: `FileAssert-OTS-ReviewMark`.

### Requirements Coverage

- **`FileAssert-OTS-ReviewMark`**: ReviewMark_ReviewPlanGeneration, ReviewMark_ReviewReportGeneration
