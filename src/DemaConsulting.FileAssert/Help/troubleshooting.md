---
title: Troubleshooting
summary: Error messages, checks that pass silently, and fixes for common mistakes.
aliases:
  - errors
  - problems
  - faq
tags:
  - troubleshooting
order: 10
see-also:
  - configuration
  - command-line
examples:
  - name: missing-config
    kind: command
    language: shell
    description: A file that --config names must exist. Exits 1.
    content: '{{tool}} --config missing.yaml'
    exit: 1
    expect: "Configuration file not found: 'missing.yaml'"
  - name: typo
    kind: source
    language: yaml
    save-as: broken.yaml
    description: A test suite with two misspelled keys, counts and contain.
    content: |
      tests:
        - name: Report
          files:
            - pattern: report.md
              counts: 1
              text:
                - contain: Passed
  - name: typo-run
    kind: command
    language: shell
    description: Run it. The rule has no known key, which is an error. Exits 1.
    content: '{{tool}} --config broken.yaml'
    exit: 1
    expect: 'Rule must specify'
  - name: namespace
    kind: source
    language: yaml
    save-as: ns.yaml
    description: An XML query that ignores the namespace of an SVG file.
    content: |
      tests:
        - name: Diagram
          files:
            - pattern: diagram.svg
              xml:
                - query: //rect
                  min: 1
  - name: namespace-run
    kind: command
    language: shell
    description: Run it. The query selects no element. Exits 1.
    content: '{{tool}} --config ns.yaml'
    exit: 1
    expect: 'returned 0 result(s)'
  - name: no-match
    kind: command
    language: shell
    description: A filter that matches no test runs nothing. Exits 0.
    content: '{{tool}} --config ns.yaml nightly'
---
**`Configuration file not found`.** The file that `--config` names does
not exist ([[#missing-config]]). Without `--config`, a missing
`.fileassert.yaml` is not an error: the tool prints
`No configuration file found` and exits 0, so check the folder that the
build runs in.

**A check passes that should fail.**

- Patterns are relative to the folder of the configuration file.
- A pattern without `min` or `count` passes when no file matches.
- Unknown keys are ignored: in the suite that [[#typo]]
  prints, the misspelled `counts` checks nothing. A rule whose only key
  is misspelled is an error.
- A filter that matches no test runs nothing and exits 0.
- An XML query in a document with a default namespace, such as SVG,
  needs `local-name()`; see [[xml-html]].

**`Error:` even with `--silent`.** A mistake in the configuration, such
as a test without `name`, an entry without `pattern`, or a rule without
a known key, stops the run with exit code 1. The message is printed even
with `--silent` and is not written to the `--log` file, like the errors
in the arguments.

**`Unexpected error:`.** The configuration file is not valid YAML. The
tool stops with a stack trace, whose first line names the line and the
column, and a nonzero exit code. Check the indentation, and quote values
that start with `*`.

**Regular expressions** need single quotes in YAML: in double quotes a
backslash starts an escape.

**`help` runs no tests.** `help` as the first argument is the help
command; put another argument first to select a test named `help`.
