---
title: Command-line options
summary: Every option and argument, and the order in which a run handles them.
aliases:
  - options
  - flags
  - usage
  - arguments
tags:
  - reference
order: 2
see-also:
  - configuration
  - self-validation
examples:
  - name: usage
    kind: command
    language: shell
    description: Print the short usage text. It ends with a pointer to this help.
    content: '{{tool}} --help'
    expect: "Run '{{tool}} help' for topics and examples."
  - name: suite
    kind: source
    language: yaml
    save-as: suite.yaml
    description: A test suite with two tests and their tags.
    content: |
      tests:
        - name: Report
          tags:
            - docs
            - smoke
          files:
            - pattern: report.md
              count: 1
              text:
                - contains: 'Version: '
        - name: Data
          tags:
            - data
          files:
            - pattern: '*.json'
              min: 1
            - pattern: '**/*.tmp'
              max: 0
  - name: filter
    kind: command
    language: shell
    description: Run only the tests whose name or one of whose tags is smoke.
    content: '{{tool}} --config suite.yaml smoke'
  - name: results
    kind: command
    language: shell
    description: Also write the outcome of every test as a TRX file.
    content: '{{tool}} --config suite.yaml --results results.trx'
  - name: silent-log
    kind: command
    language: shell
    description: Hide the console output and keep it in a log file.
    content: '{{tool}} --silent --log output.log --config suite.yaml'
  - name: unknown-option
    kind: command
    language: shell
    description: An unknown option is an error, printed even with --silent. Exits 1.
    content: '{{tool}} --silent --unknown'
    exit: 1
    expect: "Error: Unsupported argument '--unknown'"
---
Options start with `-` and may be given in any order. Every other
argument is a filter: the name or a tag of the tests to run.

| Option | Meaning |
| --- | --- |
| `-v`, `--version` | Print only the version. |
| `-?`, `-h`, `--help` | Print the short usage text. |
| `--config <file>` | The test suite to run; default `.fileassert.yaml`. |
| `--results <file>` | Write the test outcomes as `.trx` or `.xml`. `--result` also works. |
| `--silent` | Hide console output. Failures still set exit code 1. |
| `--log <file>` | Also write the output to a file. |
| `--validate` | Run the self-tests; see [[self-validation]]. |
| `--depth <n>` | First heading level of the `--validate` report, 1 to 6; default 1. |

**Order.** `--version` wins over everything and prints only the version.
Otherwise the tool prints a banner, then handles the first of `--help`,
`--validate`, or the test suite. A passing suite prints nothing more;
every failed assertion prints one line, and the tool exits 1.

**Filters.** The command that [[#filter]] prints
runs only the tests named or tagged `smoke`; the case does not matter.
With several filters a test runs when it matches any of them. A filter
that matches no test runs nothing and exits 0.

**Configuration file.** When `.fileassert.yaml` is missing and `--config`
is not given, the tool says so and exits 0. A file that `--config` names
must exist, or the run fails with exit code 1.

**Results.** `--results` writes one result per test that ran, passed or
failed: `.trx` writes a TRX file and `.xml` a JUnit file. Any other
extension is an error after the tests ran.

**Errors.** An unknown option, an option without its value, a `--depth`
outside 1 to 6, or a log file that cannot be opened prints `Error: ...`
and exits 1 ([[#unknown-option]]). These errors are
found before `--silent` and `--log` take effect, so they are always
printed and never logged.

**Help.** `help` is a command, not an option, and must be the first
argument. `--silent` and `--log` do not apply to it, and an unknown topic
exits 2. To select a test or tag named `help`, put another argument
first, as in `{{tool}} --config suite.yaml help`.
