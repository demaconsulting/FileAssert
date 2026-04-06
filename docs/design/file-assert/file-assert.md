# FileAssert System Design

## Overview

FileAssert is a .NET command-line tool for asserting file properties using YAML-defined test
suites. It is packaged as a .NET global tool and invoked as `fileassert`. The tool accepts a
configuration file, evaluates glob patterns against the file system, and reports failures when
files do not meet the declared constraints.

## System-Level Responsibilities

| Responsibility          | Description                                                               |
| :---------------------- | :------------------------------------------------------------------------ |
| Argument parsing        | Accept and validate command-line flags and positional filter arguments.   |
| Configuration loading   | Read and deserialize a YAML test-suite configuration file.                |
| Configuration file path | Accept a custom configuration file path via `--config`.                   |
| Test filtering          | Run only the tests whose name or tags match the positional filter args.   |
| Test execution          | Run selected tests, evaluating file patterns and content rules.           |
| File-type parsing       | Parse matched files as structured documents (PDF, XML, HTML, YAML, JSON)  |
|                         | when file-type assertion blocks are declared; report an immediate error   |
|                         | if the file cannot be parsed.                                             |
| Output and logging      | Report results to stdout/stderr and optionally to a log file.             |
| Self-validation         | Verify core functionality at run time via built-in tests.                 |
| Results serialization   | Write test outcome records to TRX or JUnit XML format.                    |

## Software Item Hierarchy

The system is decomposed into one top-level unit (Program) and five subsystems, each containing
one or more units:

| Item          | Level     | Units contained                                                               |
| :------------ | :-------- | :---------------------------------------------------------------------------- |
| FileAssert    | System    | —                                                                             |
| Program       | Unit      | —                                                                             |
| Cli           | Subsystem | Context                                                                       |
| Configuration | Subsystem | FileAssertConfig, FileAssertData                                              |
| Modeling      | Subsystem | FileAssertTest, FileAssertFile, FileAssertRule,                               |
|               |           | FileAssertTextAssert, FileAssertPdfAssert, FileAssertXmlAssert,               |
|               |           | FileAssertHtmlAssert, FileAssertYamlAssert, FileAssertJsonAssert              |
| Utilities     | Subsystem | PathHelpers                                                                   |
| SelfTest      | Subsystem | Validation                                                                    |

## Execution Flow

The following sequence describes the normal execution path:

1. `Program.Main` creates a `Context` instance by parsing command-line arguments.
2. `Program.Run` inspects context flags in priority order:
   a. `--version` — prints the version string and exits.
   b. `--help` — prints usage information and exits.
   c. `--validate` — delegates to `Validation.Run` for self-validation and exits.
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
8. Rule violations and parse failures are recorded via `context.WriteError`.
9. After all tests complete, `context.ExitCode` reflects whether any errors occurred.
10. When `--validate` is used with `--results`, `Validation.Run` writes TRX or JUnit XML
    results to the file specified by `context.ResultsFile`.

## Design Decisions

- **Single-assembly tool**: All logic is compiled into one assembly and published as a .NET
  global tool, simplifying installation and avoiding DLL management.
- **YAML configuration**: YAML is human-readable, widely supported, and natively handled by
  YamlDotNet. The `IgnoreUnmatchedProperties` setting provides forward compatibility.
- **Internal visibility**: All classes except test-facing members are `internal`, limiting the
  public API surface to what is strictly necessary.
- **Error accumulation**: Failures are accumulated via `Context.WriteError` rather than
  exceptions, so all assertions in a run are reported in a single pass.
- **Lazy file-type parsing**: A file is only parsed as a structured document (PDF, XML, HTML,
  YAML, or JSON) if the corresponding assertion block is declared in the YAML configuration.
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
