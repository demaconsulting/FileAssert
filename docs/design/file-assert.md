# FileAssert System Design

![FileAssert System Structure](FileAssertView.svg)

## Overview

FileAssert is a .NET command-line tool for asserting file properties using YAML-defined test
suites. It is packaged as a .NET global tool and invoked as `fileassert`. The tool accepts a
configuration file, evaluates glob patterns against the file system, and reports failures when
files do not meet the declared constraints.

## System-Level Responsibilities

| Responsibility | Description |
| :--- | :--- |
| Argument parsing | Accept and validate command-line flags and positional filter arguments. |
| Configuration loading | Read and deserialize a YAML test-suite configuration file. |
| Configuration file path | Accept a custom configuration file path via `--config`. |
| Test filtering | Run only the tests whose name or tags match the positional filter args. |
| Test execution | Run selected tests, evaluating file patterns and content rules. |
| File-type parsing | Parse matched files as structured documents (PDF, XML, HTML, YAML, JSON) |
| | when file-type assertion blocks are declared; report an immediate error |
| | if the file cannot be parsed. |
| Output and logging | Report results to stdout/stderr and optionally to a log file. |
| Built-in help | Answer the `help` command from help cards embedded in the tool. |
| Self-validation | Verify core functionality at run time via built-in tests. |
| Validation output depth | Accept a `--depth` value (1-6) controlling the Markdown heading level of |
| | self-validation output, defaulting to 1. |
| Results serialization | Write test outcome records to TRX or JUnit XML format. |

## Software Item Hierarchy

The system is decomposed into one top-level unit (Program) and five subsystems, each containing
one or more units:

| Item | Level | Units contained |
| :--- | :--- | :--- |
| FileAssert | System | — |
| Program | Unit | — |
| Cli | Subsystem | IContext, Context, HelpCommand |
| Configuration | Subsystem | FileAssertConfig, FileAssertData |
| Modeling | Subsystem | FileAssertTest, FileAssertFile, FileAssertRule, |
| | | FileAssertTextAssert, FileAssertPdfAssert, FileAssertXmlAssert, |
| | | FileAssertHtmlAssert, FileAssertYamlAssert, FileAssertJsonAssert, |
| | | FileAssertZipAssert |
| Utilities | Subsystem | IFileContainer, DirectoryFileContainer, ZipFileContainer, |
| | | PathHelpers, TemporaryDirectory |
| SelfTest | Subsystem | Validation |

## Execution Flow

The following sequence describes the normal execution path:

1. `Program.Main` offers the arguments to `HelpCommand.TryRun`. When the first argument is
   `help`, the help command builds the card catalog, writes its answer to the console without
   a banner, and `Program.Main` returns its exit code. Any other first argument returns at once
   without building the catalog, and `Program.Main` creates a `Context` instance by parsing the
   command-line arguments.
2. `Program.Run` inspects context flags in priority order:
   a. `--version` — prints the version string and exits.
   b. `--help` — prints usage information, ending with the pointer to the help command, and exits.
   c. `--validate` — delegates to `Validation.Run` for self-validation and exits. The optional
      `--depth` value (default 1, range 1-6) sets the Markdown heading level used for the
      self-validation report so it can be embedded at an appropriate level within a larger document.
   d. Default — delegates to `Program.RunToolLogic`.
3. `RunToolLogic` resolves the configuration file from `context.ConfigFile` (default:
   `.fileassert.yaml`; overridden by `--config`). If absent, it prints guidance (default
   path) or an error (explicit path) and exits.
4. `FileAssertConfig.ReadFromFile` deserializes the YAML configuration into a hierarchy of
   `FileAssertTest`, `FileAssertFile`, `FileAssertTextAssert`, and `FileAssertRule` instances.
5. `FileAssertConfig.Run` filters the test list against `context.Filters` (the positional
   name-or-tag arguments) and executes each matching test. An empty filter list runs all tests.
6. Each `FileAssertTest.Run` iterates its `FileAssertFile` list.
7. Each `FileAssertFile.Run` discovers files via a glob matcher, validates count constraints,
   and per matched file:
   a. Validates size constraints (`MinSize`, `MaxSize`).
   b. If a `text:` block is defined, delegates to `FileAssertTextAssert` which reads the
      file as text and applies each `FileAssertRule`.
   c. If a `pdf:` block is defined, attempts to parse the file using PdfPig; reports an immediate
      error if parsing fails, otherwise applies metadata, page, and body text assertions.
   d. If an `xml:` block is defined, attempts to parse the file using `System.Xml.Linq`; reports
      an immediate error if parsing fails, otherwise applies XPath node count assertions.
   e. If an `html:` block is defined, attempts to parse the file using HtmlAgilityPack; reports
      an immediate error if parsing fails, otherwise applies XPath node count assertions.
   f. If a `yaml:` block is defined, attempts to parse the file using YamlDotNet; reports an
      immediate error if parsing fails, otherwise applies dot-notation path count assertions.
   g. If a `json:` block is defined, attempts to parse the file using `System.Text.Json`; reports
      an immediate error if parsing fails, otherwise applies dot-notation path count assertions.
    h. If a `zip:` block is defined, attempts to open the file as a zip archive using
       `System.IO.Compression.ZipFile`; reports an immediate error if the archive cannot be
       opened, otherwise matches entry names against each configured glob pattern and enforces
       the declared count constraints.
8. Rule violations and parse failures are recorded via `context.WriteError`.
9. After all tests complete, if `context.ResultsFile` is set, `FileAssertConfig.Run` writes
   TRX or JUnit XML results (format determined by the file extension) to the specified path.
   `context.ExitCode` reflects whether any errors occurred.
10. When `--validate` is used with `--results`, `Validation.Run` writes TRX or JUnit XML
    results to the file specified by `context.ResultsFile`.

## Architecture

The FileAssert system contains one top-level unit and five subsystems. There is no
system-level code; the system boundary is defined by the combination of its parts.

| Item | Level | Responsibility |
| :--- | :--- | :--- |
| Program | Unit | Entry point; offers `help` to `HelpCommand`; creates `Context`; dispatches. |
| Cli | Subsystem | Contains `IContext`/`Context`; owns arg parsing, I/O references, filter list, exit. |
| | | Contains `HelpCommand`; answers the `help` command from the built-in help cards. |
| Configuration | Subsystem | Contains `FileAssertConfig`/`FileAssertData`; YAML deserialization, tests. |
| Modeling | Subsystem | Contains assertion classes; pure domain objects evaluating file rules. |
| Utilities | Subsystem | Contains `IFileContainer`, `DirectoryFileContainer`, `ZipFileContainer`, |
| | | `PathHelpers`, and `TemporaryDirectory`; shared utilities. |
| SelfTest | Subsystem | Contains `Validation`; runs built-in assertions when `--validate` passed. |

All subsystems receive a `Context` instance (created by `Program`) rather than reading
command-line arguments directly. This removes argument-parsing concerns from every
subsystem and makes error recording consistent — all violations flow through `Context.WriteError`.

## External Interfaces

| Interface | Direction | Description |
| :--- | :--- | :--- |
| Command-line arguments | Input | POSIX flags and positional filters; unrecognized flags produce an error. |
| Help command | Input/Output | `fileassert help [topic]`; prints help cards to stdout; see Help Command. |
| YAML configuration file | Input | YAML matching `FileAssertData` schema; default `.fileassert.yaml`. |
| File system (asserted files) | Input | Any file type; glob patterns relative to config file. Read-only. |
| Results file | Output | TRX or JUnit XML; written only when `--results` is specified. |
| Standard output | Output | UTF-8 text; one line per test pass or summary. |
| Standard error | Output | UTF-8 text; one line per rule violation or parse failure. |
| Log file | Output | Plain text mirroring stdout/stderr; written when `--log` is specified. |

Argument errors (an unknown option, an option without its value, a `--depth` outside 1-6, or a
log file that cannot be opened) are found before `--silent` and `--log` take effect, so they are
always printed to stderr and never logged.

**Help Command**: Built-in help, answered from help cards embedded in the tool assembly.

- *Type*: CLI.
- *Role*: Provider.
- *Contract*: `fileassert help` prints the topic index and `fileassert help <topic>` prints a
  topic card with its examples; HelpCards adds `--example`, `--search`, and `--json`. Returns exit
  code 0 for success, 2 for a wrong help argument or an unknown topic, and 70 for a defective card
  catalog.
- *Constraints*: `help` must be the first argument. It is answered before the options are parsed,
  so `--silent` and `--log` do not apply and no banner is printed. A test or tag named `help` is
  selected only when another argument comes first, such as `--config <file> help`.

## Dependencies

Runtime library dependencies used by the FileAssert system:

- **YamlDotNet**: YAML configuration file deserialization and YAML-document content assertions.
- **PdfPig**: PDF document parsing for metadata, page count, and body text extraction.
- **HtmlAgilityPack**: lenient HTML document parsing for XPath node-count assertions.
- **Microsoft.Extensions.FileSystemGlobbing**: cross-platform glob pattern evaluation for file discovery.
- **DemaConsulting.TestResults**: serialization of test results to TRX and JUnit XML formats.
- **DemaConsulting.HelpCards**: loads, validates, and serves the built-in help cards for the
  `help` command (see *HelpCards*, the OTS integration design).
- **System.Xml.Linq / System.Xml.XPath** (.NET BCL): XML document parsing and XPath node-count assertions.
- **System.Text.Json** (.NET BCL): JSON document parsing for dot-notation path assertions.
- **System.IO.Compression** (.NET BCL): zip archive entry enumeration and glob-based count assertions.

Test-project dependency:

- **xUnit**: unit test discovery, execution, and TRX reporting — see *xUnit Integration Design*.

## Risk Control Measures

N/A — FileAssert is a development and CI tool. It carries no safety-critical function
and has no requirement for hardware or software item segregation for risk control
purposes. The tool runs with standard user-level operating-system permissions and
imposes no memory or process isolation boundaries beyond standard .NET process containment.

## Data Flow

```text
Command-line arguments
  ├─► help [topic] (first argument) ─► HelpCommand ──────────► stdout/stderr → exit 0 or 2
  │
  └─► Context (parsed flags, filter list, ResultsFile path)
        │
        ├─► --version / --help ──────────────────────────────► stdout → exit 0
        │
        ├─► --validate ──────────────────────────────────────► Validation.Run
        │                                                           │
        │                                                           └─► (same assertion pipeline below)
        │
        └─► RunToolLogic
              │
              ├─► config file path (from Context.ConfigFile)
              │
              └─► FileAssertConfig.ReadFromFile
                    │   YAML file ──────────────────────────► FileAssertData deserialization
                    │                                              │
                    │                              FileAssertTest[] hierarchy
                    │
                    └─► FileAssertConfig.Run (filtered by Context.Filters)
                          │
                          └─► per test: FileAssertTest.Run
                                │
                                └─► per file pattern: FileAssertFile.Run
                                      │
                                      ├─► glob match ──────► file system (read-only)
                                      │                           │
                                      │                       matched file paths
                                      │
                                      ├─► size check ──────► Context.WriteError (on violation)
                                      └─► content assertion (text / pdf / xml / html / yaml /
                                          json / zip) ──────► Context.WriteError (on violation)

Outputs:
  Context.WriteError ─────────────────────────────────────► stderr (per violation)
  pass/fail lines ─────────────────────────────────────────► stdout
  Context.ResultsFile (optional) ──────────────────────────► TRX or JUnit XML file
  Context.ExitCode ────────────────────────────────────────► process exit code (0 = pass, 1 = fail)
```

## Design Constraints

- **Platform**: .NET (cross-platform); supported on Windows, macOS, and Linux via the .NET runtime. No
  platform-specific code paths; all file system operations use `Path.Combine` and
  `Microsoft.Extensions.FileSystemGlobbing` to normalize path separators per the host OS.
- **Distribution**: packaged as a .NET global tool; installed with `dotnet tool install` and invoked
  as `fileassert` on the `PATH`.
- **Assembly scope**: all production logic is compiled into a single assembly. No external native DLLs
  beyond those shipped with the .NET runtime.
- **Permissions**: runs with standard user-level operating-system permissions. Does not require
  elevated privileges.
- **File system access**: read-only for the configuration file and all asserted files. Write access is
  required only for the optional results file and optional log file.
- **Memory**: no explicit memory limit is defined. PDF, XML, HTML, YAML, and JSON files are parsed
  in-process; very large files may exhaust available heap, which is an accepted limitation of the
  current design.
- **Exit code contract**: `0` when all assertions pass or `--help`/`--version` is used; `1` when at
  least one assertion violation or self-validation failure is recorded. The `help` command has its
  own exit codes; see Help Command under External Interfaces.
- **Missing default configuration file**: When no `--config` flag is provided and the
  default `.fileassert.yaml` file does not exist, the tool prints guidance and exits with
  code 0 (not an error condition). When an explicit `--config` path is missing, the tool
  reports an error and exits with code 1.

## Design Decisions

- **Cross-platform portability**: All file system operations use `Path.Combine`, `Path.GetFullPath`,
  and `Microsoft.Extensions.FileSystemGlobbing`, which normalize path separators per the host OS.
  No `OperatingSystem.IsWindows()` or similar guards appear in production code; platform differences
  are handled entirely by the BCL and the .NET runtime.
- **Single-assembly tool**: All logic is compiled into one assembly and published as a .NET
  global tool, simplifying installation and avoiding DLL management.
- **YAML configuration**: YAML is human-readable, widely supported, and natively handled by
  YamlDotNet. The `IgnoreUnmatchedProperties` setting provides forward compatibility.
- **Internal visibility**: All classes except test-facing members are `internal`, limiting the
  public API surface to what is strictly necessary.
- **Error accumulation**: Failures are accumulated via `Context.WriteError` rather than
  exceptions, so all assertions in a run are reported in a single pass.
- **Lazy file-type parsing**: A file is only parsed as a structured document (PDF, XML, HTML,
  YAML, JSON, or zip archive) if the corresponding assertion block is declared in the YAML configuration.
  This avoids unnecessary I/O and third-party library invocations for files that are only
  checked for size or text content.
- **Immediate failure on parse error**: If a file-type assertion block is declared and the file
  cannot be parsed as the declared format, an error is written immediately and no further
  assertions for that file are evaluated. This prevents misleading partial results.
- **Library selection**: PdfPig is chosen for PDF parsing because it exposes metadata, pages,
  and extractable text with a managed .NET API. `System.Xml.Linq` and `System.Xml.XPath` are
  used for XML because they are part of the .NET BCL and require no additional dependencies.
  HtmlAgilityPack is chosen for HTML because it is the de-facto standard for lenient HTML parsing
  in .NET. YamlDotNet is already a project dependency and is reused for YAML parsing.
  `System.Text.Json` is part of the .NET BCL and is used for JSON parsing.
- **Zip archive inspection**: `System.IO.Compression.ZipFile` is part of the .NET BCL and is used
  to open zip archives and enumerate their entries, requiring no additional dependencies.
