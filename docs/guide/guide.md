# Introduction

## Purpose

FileAssert is a .NET CLI tool for asserting file properties using YAML-defined test suites. It validates
files against acceptance criteria such as size constraints, content requirements, and pattern matching,
enabling automated file validation in CI/CD pipelines and regulated environments.

## Scope

This user guide covers:

- Installation instructions
- Running file assertion tests
- YAML test file format and acceptance criteria
- Command-line options reference
- Practical examples for various scenarios

# Continuous Compliance

This tool follows the
[Continuous Compliance](https://github.com/demaconsulting/ContinuousCompliance) methodology, which ensures
compliance evidence is generated automatically on every CI run.

## Key Practices

- **Requirements Traceability**: Every requirement is linked to passing tests, and a trace matrix is
  auto-generated on each release
- **Linting Enforcement**: markdownlint, cspell, and yamllint are enforced before any build proceeds
- **Automated Audit Documentation**: Each release ships with generated requirements, justifications,
  trace matrix, and quality reports
- **CodeQL and SonarCloud**: Security and quality analysis runs on every build

# Installation

Install the tool globally using the .NET CLI:

```bash
dotnet tool install -g DemaConsulting.FileAssert
```

# Usage

## Display Version

Display the tool version:

```bash
fileassert --version
```

## Display Help

Display usage information:

```bash
fileassert --help
```

## Running Tests

Run file assertion tests from the default `.fileassert.yaml` file:

```bash
fileassert
```

Run tests from a specific file:

```bash
fileassert --config tests.yaml
```

Run only tests matching specific names or tags:

```bash
fileassert smoke release
```

Write results to a TRX file:

```bash
fileassert --results results.trx
```

Write results to a JUnit XML file:

```bash
fileassert --results results.xml
```

## Self-Validation

Self-validation produces a report demonstrating that FileAssert is functioning correctly. This is useful in
regulated industries where tool validation evidence is required.

### Running Validation

To perform self-validation:

```bash
fileassert --validate
```

To save validation results to a file:

```bash
fileassert --validate --results results.trx
```

The results file format is determined by the file extension: `.trx` for TRX (MSTest) format,
or `.xml` for JUnit format.

### Validation Report

The validation report contains the tool version, machine name, operating system version,
.NET runtime version, timestamp, and test results.

Example validation report:

```text
# DEMA Consulting FileAssert

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| Tool Version        | 1.0.0                                              |
| Machine Name        | BUILD-SERVER                                       |
| OS Version          | Ubuntu 22.04.3 LTS                                 |
| DotNet Runtime      | .NET 10.0.0                                        |
| Time Stamp          | 2024-01-15 10:30:00 UTC                            |

✓ FileAssert_VersionDisplay - Passed
✓ FileAssert_HelpDisplay - Passed

Total Tests: 2
Passed: 2
Failed: 0
```

### Validation Tests

Each test proves specific functionality works correctly:

- **`FileAssert_VersionDisplay`** - `--version` outputs a valid version string.
- **`FileAssert_HelpDisplay`** - `--help` outputs usage and options information.

## Silent Mode

Suppress console output:

```bash
fileassert --silent
```

## Logging

Write output to a log file:

```bash
fileassert --log output.log
```

# FileAssert YAML Format

> **Proposed**: The FileAssert YAML test format described in this section is a planned design
> and has not yet been implemented. The format and option names are subject to change.

The tests file (`.fileassert.yaml` by default) defines one or more named tests. Each test specifies a
set of files using a glob pattern, optional tags for filtering, and one or more acceptance criteria.

```yaml
# .fileassert.yaml
tests:
  - name: TestProject_BinariesExist
    description: "Application binaries exist"
    tags: [smoke, release]
    files:
      - path: "bin/**/*.exe"
        count: 1
      - path: "bin/**/*.dll"
        count: 2

  - name: TestProject_ConfigValid
    description: "Config file size is reasonable"
    tags: [config]
    files:
      - path: "config/settings.json"
        min-size: 10
        max-size: 1048576
        contains: '"ConnectionStrings"'
        does-not-contain: "password123"

  - name: TestProject_LogsValid
    description: "Log files match expected pattern"
    tags: [logs]
    files:
      - path: "logs/*.log"
        contains-regex: "\\d{4}-\\d{2}-\\d{2}"
        does-not-contain-regex: "FATAL|CRITICAL"
```

## Acceptance Criteria Reference

| Criterion                | Description                                                      |
| ------------------------ | ---------------------------------------------------------------- |
| `count`                  | Number of files matching the path pattern (`min`, `max`)         |
| `min-size`               | Minimum file size in bytes                                       |
| `max-size`               | Maximum file size in bytes                                       |
| `contains`               | File must contain the specified text                             |
| `does-not-contain`       | File must not contain the specified text                         |
| `contains-regex`         | File must match the specified regular expression                 |
| `does-not-contain-regex` | File must not match the specified regular expression             |

# Command-Line Options

The following command-line options are supported:

| Option               | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| `-v`, `--version`    | Display version information                                  |
| `-?`, `-h`, `--help` | Display help message                                         |
| `--silent`           | Suppress console output                                      |
| `--validate`         | Run self-validation                                          |
| `--results <file>`   | Write test results to file (TRX or JUnit format)             |
| `--log <file>`       | Write output to log file                                     |
| `--config <file>`    | Path to the tests file (default: `.fileassert.yaml`)         |
| `<name-or-tag>`      | Test name or tag to run (any argument not starting with `--`)|

# Examples

## Example 1: Run Default Tests

```bash
fileassert
```

## Example 2: Run Smoke Tests with Results

```bash
fileassert --results smoke-results.trx smoke
```

## Example 3: Self-Validation with Results

```bash
fileassert --validate --results validation-results.trx
```

## Example 4: Silent Mode with Logging

```bash
fileassert --silent --log tool-output.log
```
