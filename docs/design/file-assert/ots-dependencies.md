# OTS Dependencies Design

## Overview

FileAssert relies on several off-the-shelf (OTS) tools and libraries that are integrated
into the build, quality, and traceability pipeline. This document describes each OTS
dependency, its purpose, and how its correct operation is verified.

## OTS Tools

### ReqStream

| Attribute    | Value                                                              |
| :----------- | :----------------------------------------------------------------- |
| Purpose      | Requirements traceability enforcement                              |
| Role         | Verifies every requirement is covered by at least one passing test |
| Verification | Build fails if any requirement has no passing test evidence        |

ReqStream reads requirements from `docs/reqstream/**/*.yaml` and cross-references them
against TRX test result files. It enforces that every requirement listed is traceable to a
passing test, providing continuous compliance evidence for formal reviews.

### ReviewMark

| Attribute    | Value                                                               |
| :----------- | :------------------------------------------------------------------ |
| Purpose      | File review status enforcement                                      |
| Role         | Ensures all source files have been formally reviewed                |
| Verification | Build fails if any file in a review-set lacks a valid review record |

ReviewMark reads review records from the repository and validates that every file
included in a review-set has a recorded review. This provides a continuous audit trail
for compliance with formal code review requirements.

### SonarMark

| Attribute    | Value                                                      |
| :----------- | :--------------------------------------------------------- |
| Purpose      | SonarCloud quality gate reporting                          |
| Role         | Reports the SonarCloud quality gate status in build output |
| Verification | Build fails if the SonarCloud quality gate is not passing  |

SonarMark queries the SonarCloud API and surfaces the quality gate result as a build
step. Failures in code quality metrics (coverage, duplications, maintainability) are
caught before merging.

### SarifMark

| Attribute    | Value                                                            |
| :----------- | :--------------------------------------------------------------- |
| Purpose      | CodeQL SARIF report generation                                   |
| Role         | Converts CodeQL SARIF output into a human-readable build summary |
| Verification | Build fails if SARIF results contain open security alerts        |

SarifMark processes the SARIF files produced by CodeQL and generates a Markdown
summary report. Any open high-severity alerts cause the build to fail, enforcing
zero-tolerance for unaddressed security findings.

### BuildMark

| Attribute    | Value                                                                       |
| :----------- | :-------------------------------------------------------------------------- |
| Purpose      | Tool version documentation                                                  |
| Role         | Captures and records the versions of all build tools used in a pipeline run |
| Verification | Version records are included in the build artifact for audit purposes       |

BuildMark interrogates installed tool versions and writes a version manifest to the
build output. This ensures that the exact tool versions used to produce a release are
permanently recorded and reproducible.

### VersionMark

| Attribute    | Value                                                               |
| :----------- | :------------------------------------------------------------------ |
| Purpose      | Version tracking                                                    |
| Role         | Injects the current version into build output and assembly metadata |
| Verification | The published NuGet package version matches the repository tag      |

VersionMark reads the version from a central configuration file and propagates it to
all artifacts produced by the build. This eliminates manual version updates and ensures
consistency between the NuGet package version, assembly version, and release tag.

### xUnit

| Attribute    | Value                                                                                |
| :----------- | :----------------------------------------------------------------------------------- |
| Purpose      | Unit test framework                                                                  |
| Role         | Provides the test runner, assertion library, and TRX result output used by all tests |
| Verification | All tests must pass; TRX files are consumed by ReqStream for traceability            |

xUnit (version 3) is the unit test framework for all C# tests in this repository.
It provides `[Fact]`, `[Collection]`, and the assertion methods used throughout
the test suite. TRX output format is enabled so that ReqStream can parse test results
and verify requirements coverage.
