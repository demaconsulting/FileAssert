# Platform Support Design

## Overview

FileAssert is distributed as a single .NET global tool assembly that runs without modification
on Windows, Linux, and macOS. The tool targets multiple .NET runtime versions to support a wide
range of developer and CI/CD environments.

## Target Platforms

| Platform | Supported |
| :------- | :-------- |
| Windows  | Yes       |
| Linux    | Yes       |
| macOS    | Yes       |

## Target Runtimes

| Runtime  | Supported |
| :------- | :-------- |
| .NET 8   | Yes       |
| .NET 9   | Yes       |
| .NET 10  | Yes       |

## Multi-Platform Design Approach

The tool is compiled as a single framework-dependent assembly and published as a .NET global
tool via NuGet. The following design decisions ensure cross-platform compatibility:

- **Single-assembly tool**: All logic is compiled into one assembly with no native dependencies,
  allowing the .NET runtime to provide the platform abstraction layer.
- **Path handling**: All file system operations use `Path.Combine`, `Path.GetFullPath`, and
  related BCL methods that normalize separators per the host operating system.
- **Glob evaluation**: File pattern matching uses `Microsoft.Extensions.FileSystemGlobbing`,
  which handles platform path separators transparently.
- **No platform guards**: The production code contains no `OperatingSystem.IsWindows()` or
  similar guards; platform differences are handled by the BCL and the .NET runtime.

## Testing Strategy

Correctness across all supported platform and runtime combinations is validated through a
CI/CD matrix that runs the full test suite on every combination:

| Dimension        | Values                             |
| :--------------- | :--------------------------------- |
| Operating system | Windows, Linux, macOS              |
| .NET version     | .NET 8, .NET 9, .NET 10            |
| Matrix size      | 3 OS × 3 runtimes = 9 combinations |

Each matrix leg runs `dotnet test` and publishes a TRX result file. The result file names
include the OS and runtime identifiers so that platform-specific failures are immediately
visible in the CI/CD output.

## Platform-Specific Test Result Files

Test results are written to platform-specific TRX files using the naming convention:

```text
TestResults/{os}-{runtime}/DemaConsulting.FileAssert.Tests.trx
```

ReqStream uses source filters to attribute test results to the correct platform leg when
verifying requirements coverage. A test that must pass on all platforms is referenced
without a source filter; a test that is platform-specific includes a source filter such
as `windows@TestName` in the requirements YAML.
