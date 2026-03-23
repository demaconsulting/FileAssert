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
  `does-not-contain`), regex patterns (`contains-regex`, `does-not-contain-regex`), and file counts
  (`count-of`)
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
        count-min: 1

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

### Acceptance Criteria Reference

| Criterion                | Description                                                      |
| ------------------------ | ---------------------------------------------------------------- |
| `count`                  | Exact number of files matching the path pattern                  |
| `count-min`              | Minimum number of files matching the path pattern                |
| `count-max`              | Maximum number of files matching the path pattern                |
| `min-size`               | Minimum file size in bytes                                       |
| `max-size`               | Maximum file size in bytes                                       |
| `contains`               | File must contain the specified text                             |
| `does-not-contain`       | File must not contain the specified text                         |
| `contains-regex`         | File must match the specified regular expression                 |
| `does-not-contain-regex` | File must not match the specified regular expression             |

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

Total Tests: 2
Passed: 2
Failed: 0
```

Each test in the report proves:

- **`FileAssert_VersionDisplay`** - `--version` outputs a valid version string.
- **`FileAssert_HelpDisplay`** - `--help` outputs usage and options information.

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
[link-guide]: https://github.com/demaconsulting/FileAssert/blob/main/docs/guide/guide.md
[link-continuous-compliance]: https://github.com/demaconsulting/ContinuousCompliance
