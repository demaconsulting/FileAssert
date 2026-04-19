# DemaConsulting.FileAssert

[![GitHub forks][badge-forks]][link-forks]
[![GitHub stars][badge-stars]][link-stars]
[![GitHub contributors][badge-contributors]][link-contributors]
[![License][badge-license]][link-license]
[![Build][badge-build]][link-build]
[![Quality Gate][badge-quality]][link-quality]
[![Security][badge-security]][link-security]
[![NuGet][badge-nuget]][link-nuget]

FileAssert is a .NET CLI tool for asserting file properties using YAML-defined test suites. It validates
files against acceptance criteria such as size constraints, content requirements, and pattern matching,
making it ideal for CI/CD pipelines and compliance workflows.

## Features

- **File Assertion Testing**: YAML-defined test suites that validate file properties against acceptance criteria
- **Glob Pattern Matching**: Select files using glob patterns via Microsoft.Extensions.FileSystemGlobbing
- **Multiple Acceptance Criteria**: Validate size (`min-size`, `max-size`), content (`contains`,
  `does-not-contain`), regex patterns (`matches`, `does-not-contain-regex`), and file counts
  (`count`, `min`, `max`)
- **Tag-Based Test Filtering**: Run a targeted subset of tests by filtering on tags
- **TRX and JUnit Output**: Write test results to TRX or JUnit format via DemaConsulting.TestResults
- **Self-Validation**: Built-in validation tests confirm the tool is functioning correctly
- **Multi-Platform Support**: Builds and runs on Windows, Linux, and macOS
- **Multi-Runtime Support**: Targets .NET 8, 9, and 10
- **Comprehensive CI/CD**: GitHub Actions workflows with quality checks, builds, and integration tests
- **Continuous Compliance**: Compliance evidence generated automatically on every CI run, following
  the [Continuous Compliance][link-continuous-compliance] methodology
- **SonarCloud Integration**: Quality gate and security analysis on every build
- **Documentation Generation**: Automated build notes, user guide, code quality reports,
  requirements, justifications, and trace matrix
- **Requirements Traceability**: Requirements linked to passing tests with auto-generated trace matrix

## Installation

Install the tool globally using the .NET CLI:

```bash
dotnet tool install -g DemaConsulting.FileAssert
```

## Usage

```bash
# Display version
fileassert --version

# Display help
fileassert --help

# Run tests from the default .fileassert.yaml file
fileassert

# Run tests from a specific file
fileassert --config tests.yaml

# Run only tests matching specific names or tags
fileassert smoke release

# Write results to a TRX file
fileassert --results results.trx

# Write results to a JUnit XML file
fileassert --results results.xml

# Run self-validation
fileassert --validate

# Silent mode with logging
fileassert --silent --log output.log
```

## Command-Line Options

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

## FileAssert YAML Format

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

### Acceptance Criteria Reference

| Criterion                         | Description                                                  |
| --------------------------------- | ------------------------------------------------------------ |
| `count`                           | Exact number of files matching the pattern                   |
| `min`                             | Minimum number of files matching the pattern                 |
| `max`                             | Maximum number of files matching the pattern                 |
| `min-size`                        | Minimum file size in bytes                                   |
| `max-size`                        | Maximum file size in bytes                                   |
| `text:`                           | Text content assertions block                                |
| `text[].contains`                 | File must contain the specified text                         |
| `text[].does-not-contain`         | File must not contain the specified text                     |
| `text[].matches`                  | File must match the specified regular expression             |
| `text[].does-not-contain-regex`   | File must not match the specified regular expression         |
| `pdf:`                            | PDF document assertions (fails if file is not a valid PDF)   |
| `pdf.metadata[].field`            | PDF metadata field name to assert                            |
| `pdf.metadata[].contains`         | PDF metadata field must contain the specified text           |
| `pdf.metadata[].matches`          | PDF metadata field must match the regular expression         |
| `pdf.pages.min`                   | Minimum number of pages in the PDF document                  |
| `pdf.pages.max`                   | Maximum number of pages in the PDF document                  |
| `pdf.text[].contains`             | PDF body text must contain the specified text                |
| `pdf.text[].matches`              | PDF body text must match the specified regular expression    |
| `xml:`                            | XML document assertions (fails if file is not valid XML)     |
| `xml[].query`                     | XPath expression selecting nodes                             |
| `xml[].count`                     | Exact number of matched XML nodes                            |
| `xml[].min`                       | Minimum number of matched XML nodes                          |
| `xml[].max`                       | Maximum number of matched XML nodes                          |
| `html:`                           | HTML document assertions (fails if file is not valid HTML)   |
| `html[].query`                    | XPath expression selecting nodes                             |
| `html[].count`                    | Exact number of matched HTML nodes                           |
| `html[].min`                      | Minimum number of matched HTML nodes                         |
| `html[].max`                      | Maximum number of matched HTML nodes                         |
| `yaml:`                           | YAML document assertions (fails if file is not valid YAML)   |
| `yaml[].query`                    | Dot-notation path selecting YAML nodes (e.g. `server.host`)  |
| `yaml[].count`                    | Exact number of matched YAML nodes                           |
| `yaml[].min`                      | Minimum number of matched YAML nodes                         |
| `yaml[].max`                      | Maximum number of matched YAML nodes                         |
| `json:`                           | JSON document assertions (fails if file is not valid JSON)   |
| `json[].query`                    | Dot-notation path selecting JSON nodes (e.g. `app.version`)  |
| `json[].count`                    | Exact number of matched JSON nodes                           |
| `json[].min`                      | Minimum number of matched JSON nodes                         |
| `json[].max`                      | Maximum number of matched JSON nodes                         |

## Self Validation

Running self-validation produces a report demonstrating that FileAssert is functioning correctly:

```text
# DEMA Consulting FileAssert

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| Tool Version        | <version>                                          |
| Machine Name        | <machine-name>                                     |
| OS Version          | <os-version>                                       |
| DotNet Runtime      | <dotnet-runtime-version>                           |
| Time Stamp          | <timestamp> UTC                                    |

✓ FileAssert_VersionDisplay - Passed
✓ FileAssert_HelpDisplay - Passed
✓ FileAssert_Results - Passed
✓ FileAssert_Exists - Passed
✓ FileAssert_Contains - Passed

Total Tests: 5
Passed: 5
Failed: 0
```

Each test in the report proves:

- **`FileAssert_VersionDisplay`** - `--version` outputs a valid version string.
- **`FileAssert_HelpDisplay`** - `--help` outputs usage and options information.
- **`FileAssert_Results`** - results can be generated with passes and fails.
- **`FileAssert_Exists`** - file-existence can be checked via glob pattern.
- **`FileAssert_Contains`** - file-contains can be checked.

See the [User Guide][link-guide] for more details on the self-validation tests.

On validation failure the tool will exit with a non-zero exit code.

## Documentation

Generated documentation includes:

- **Build Notes**: Release information and changes
- **User Guide**: Comprehensive usage documentation
- **Code Quality Report**: CodeQL and SonarCloud analysis results
- **Requirements**: Functional and non-functional requirements
- **Requirements Justifications**: Detailed requirement rationale
- **Trace Matrix**: Requirements to test traceability

## License

Copyright (c) DEMA Consulting. Licensed under the MIT License. See [LICENSE][link-license] for details.

By contributing to this project, you agree that your contributions will be licensed under the MIT License.

<!-- Badge References -->
[badge-forks]: https://img.shields.io/github/forks/demaconsulting/FileAssert?style=plastic
[badge-stars]: https://img.shields.io/github/stars/demaconsulting/FileAssert?style=plastic
[badge-contributors]: https://img.shields.io/github/contributors/demaconsulting/FileAssert?style=plastic
[badge-license]: https://img.shields.io/github/license/demaconsulting/FileAssert?style=plastic
[badge-build]: https://img.shields.io/github/actions/workflow/status/demaconsulting/FileAssert/build_on_push.yaml?style=plastic
[badge-quality]: https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_FileAssert&metric=alert_status
[badge-security]: https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_FileAssert&metric=security_rating
[badge-nuget]: https://img.shields.io/nuget/v/DemaConsulting.FileAssert?style=plastic

<!-- Link References -->
[link-forks]: https://github.com/demaconsulting/FileAssert/network/members
[link-stars]: https://github.com/demaconsulting/FileAssert/stargazers
[link-contributors]: https://github.com/demaconsulting/FileAssert/graphs/contributors
[link-license]: https://github.com/demaconsulting/FileAssert/blob/main/LICENSE
[link-build]: https://github.com/demaconsulting/FileAssert/actions/workflows/build_on_push.yaml
[link-quality]: https://sonarcloud.io/dashboard?id=demaconsulting_FileAssert
[link-security]: https://sonarcloud.io/dashboard?id=demaconsulting_FileAssert
[link-nuget]: https://www.nuget.org/packages/DemaConsulting.FileAssert
[link-guide]: https://github.com/demaconsulting/FileAssert/blob/main/docs/user_guide/introduction.md
[link-continuous-compliance]: https://github.com/demaconsulting/ContinuousCompliance
