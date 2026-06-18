## ReqStream Verification

This document provides the verification evidence for the `ReqStream` OTS software item.

### Required Functionality

DemaConsulting.ReqStream processes requirements.yaml and the TRX test-result files to produce a
requirements report, justifications document, and traceability matrix. When run with `--enforce`, it
exits with a non-zero code if any requirement lacks test evidence, making unproven requirements a
build-breaking condition. A successful pipeline run with `--enforce` proves all requirements are
covered and that ReqStream is functioning.

### Verification Approach

ReqStream is verified by the CI pipeline invoking `reqstream --enforce` with `requirements.yaml`
and all TRX test-evidence files accumulated during the build. ReqStream generates
`requirements.md`, `justifications.md`, and `trace_matrix.md`. If ReqStream failed or produced no
output, the subsequent Pandoc step would fail, breaking the CI build. Additionally, `--enforce`
exits non-zero if any requirement lacks test evidence, which would also fail the build. A passing
CI build proves ReqStream correctly processed the project's real requirements and found complete
test coverage.

### Test Scenarios

#### ReqStream_EnforcementMode

**Scenario**: `reqstream --enforce` is run with `requirements.yaml` and all accumulated TRX files.

**Expected**: Exits 0; all requirements have passing test evidence; requirements documents are
generated.

### Acceptance Criteria

N/A - Acceptance criteria are managed at the system integration level. This OTS item is
considered verified when the integration test scenarios that exercise its functionality
pass in the CI pipeline.
