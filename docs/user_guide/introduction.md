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
✓ FileAssert_Results - Passed
✓ FileAssert_Exists - Passed
✓ FileAssert_Contains - Passed

Total Tests: 5
Passed: 5
Failed: 0
```

### Validation Tests

Each test proves specific functionality works correctly:

- **`FileAssert_VersionDisplay`** - `--version` outputs a valid version string.
- **`FileAssert_HelpDisplay`** - `--help` outputs usage and options information.
- **`FileAssert_Results`** - results can be generated with passes and fails.
- **`FileAssert_Exists`** - file-existence can be checked via glob pattern.
- **`FileAssert_Contains`** - file-contains can be checked.

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

The tests file (`.fileassert.yaml` by default) defines one or more named tests. Each test specifies a
set of files using a glob pattern, optional tags for filtering, and one or more acceptance criteria.

```yaml
# .fileassert.yaml
tests:
  - name: TestProject_BinariesExist
    # Application binaries exist
    tags: [smoke, release]
    files:
      - pattern: "bin/**/*.exe"
        count: 1
      - pattern: "bin/**/*.dll"
        min: 1

  - name: TestProject_ConfigValid
    # Config file size is reasonable
    tags: [config]
    files:
      - pattern: "config/settings.json"
        min-size: 10
        max-size: 1048576
        text:
          - contains: '"ConnectionStrings"'
          - does-not-contain: "password123"

  - name: TestProject_LogsValid
    # Log files match expected pattern
    tags: [logs]
    files:
      - pattern: "logs/*.log"
        text:
          - matches: "\\d{4}-\\d{2}-\\d{2}"
          - does-not-contain-regex: "FATAL|CRITICAL"

  - name: TestProject_ReportValid
    # PDF report meets requirements
    tags: [report]
    files:
      - pattern: "output/report.pdf"
        pdf:
          metadata:
            - field: "Title"
              contains: "Annual Report"
          pages:
            min: 1
            max: 100
          text:
            - contains: "Executive Summary"

  - name: TestProject_XmlConfigValid
    # XML configuration has required elements
    tags: [config]
    files:
      - pattern: "config/settings.xml"
        xml:
          - query: "//configuration/setting"
            min: 1

  - name: TestProject_HtmlValid
    # HTML document has required structure
    tags: [web]
    files:
      - pattern: "docs/index.html"
        html:
          - query: "//head/title"
            count: 1

  - name: TestProject_AppYamlConfigValid
    # Application YAML config has required keys
    tags: [config]
    files:
      - pattern: "config/appsettings.yaml"
        yaml:
          - query: "server.host"
            count: 1

  - name: TestProject_AppJsonConfigValid
    # Application JSON config has required keys
    tags: [config]
    files:
      - pattern: "config/appsettings.json"
        json:
          - query: "ConnectionStrings"
            count: 1
```

## Acceptance Criteria Reference

| Criterion                           | Description                                                   |
| ----------------------------------- | ------------------------------------------------------------- |
| `count`                             | Exact number of files matching the pattern                    |
| `min`                               | Minimum number of files matching the pattern                  |
| `max`                               | Maximum number of files matching the pattern                  |
| `min-size`                          | Minimum file size in bytes                                    |
| `max-size`                          | Maximum file size in bytes                                    |
| `text:`                             | Text content assertions block                                 |
| `text[].contains`                   | File must contain the specified text                          |
| `text[].does-not-contain`           | File must not contain the specified text                      |
| `text[].matches`                    | File must match the specified regular expression              |
| `text[].does-not-contain-regex`     | File must not match the specified regular expression          |
| `pdf:`                              | PDF document assertions (fails if file is not a valid PDF)    |
| `pdf.metadata[].field`              | PDF metadata field name to assert                             |
| `pdf.metadata[].contains`           | PDF metadata field must contain the specified text            |
| `pdf.metadata[].matches`            | PDF metadata field must match the regular expression          |
| `pdf.pages.min`                     | Minimum number of pages in the PDF document                   |
| `pdf.pages.max`                     | Maximum number of pages in the PDF document                   |
| `pdf.text[].contains`               | PDF body text must contain the specified text                 |
| `pdf.text[].does-not-contain`       | PDF body text must not contain the specified text             |
| `pdf.text[].matches`                | PDF body text must match the specified regular expression     |
| `pdf.text[].does-not-contain-regex` | PDF body text must not match the specified regular expression |
| `xml:`                              | XML document assertions (fails if file is not valid XML)      |
| `xml[].query`                       | XPath expression selecting nodes                              |
| `xml[].count`                       | Exact number of matched XML nodes                             |
| `xml[].min`                         | Minimum number of matched XML nodes                           |
| `xml[].max`                         | Maximum number of matched XML nodes                           |
| `html:`                             | HTML document assertions (fails if file is not valid HTML)    |
| `html[].query`                      | XPath expression selecting nodes                              |
| `html[].count`                      | Exact number of matched HTML nodes                            |
| `html[].min`                        | Minimum number of matched HTML nodes                          |
| `html[].max`                        | Maximum number of matched HTML nodes                          |
| `yaml:`                             | YAML document assertions (fails if file is not valid YAML)    |
| `yaml[].query`                      | Dot-notation path selecting YAML nodes (e.g. `server.host`)   |
| `yaml[].count`                      | Exact number of matched YAML nodes                            |
| `yaml[].min`                        | Minimum number of matched YAML nodes                          |
| `yaml[].max`                        | Maximum number of matched YAML nodes                          |
| `json:`                             | JSON document assertions (fails if file is not valid JSON)    |
| `json[].query`                      | Dot-notation path selecting JSON nodes (e.g. `app.version`)   |
| `json[].count`                      | Exact number of matched JSON nodes                            |
| `json[].min`                        | Minimum number of matched JSON nodes                          |
| `json[].max`                        | Maximum number of matched JSON nodes                          |

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
