# Shared Package Verification

This section documents the verification evidence for the shared package used by the FileAssert
build pipeline. The shared package is an earlier released version of FileAssert itself, consumed
as a dogfooding dependency in CI.

## Verification Strategy

The shared package is verified by two complementary layers of evidence. First, the CI pipeline
runs `fileassert --validate --results artifacts/fileassert-self-validation.trx` after all
documents have been generated, exercising FileAssert's built-in self-validation suite and
recording functional test results for ReqStream.

Second, FileAssert is used throughout the pipeline to validate every generated document. If
FileAssert were non-functional, these validation steps would fail, causing `reqstream --enforce`
to report missing test coverage and fail the build. A passing CI build therefore constitutes
transitive evidence that FileAssert correctly asserted document content at each stage of the
pipeline.

Per-package verification details, test scenarios, and requirements mappings are documented in the
individual files under `docs/verification/shared/`.

## Qualification Evidence

The self-validation TRX file (`artifacts/fileassert-self-validation.trx`) produced by
`fileassert --validate --results` during CI is consumed by `reqstream --enforce` to confirm
that all named test scenarios have a recorded passing result.

All CI artifacts are retained as GitHub Actions run artifacts and are accessible from the
FileAssert releases page.

## Regression Approach

When the shared package version is updated, the following steps apply:

1. Update the version reference in the tool manifest (`.config/dotnet-tools.json`).
2. Run the full CI pipeline. All self-validation tests and FileAssert assertions must pass.
3. Confirm that `reqstream --enforce` reports no unmet requirements.
4. If any step fails, investigate whether the version change introduced a breaking change
   affecting the features used by this project, and resolve before merging the update.

## Shared Packages

The following shared package is used by FileAssert and is verified in this section:

- **FileAssert** — validates generated HTML and PDF documents for existence, structure, and
  content; verified by self-validation tests and transitive pipeline evidence.
