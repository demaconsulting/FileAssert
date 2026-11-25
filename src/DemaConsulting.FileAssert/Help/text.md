---
title: Text assertions
summary: Check that a file contains, lacks, or matches text or a regular expression.
aliases:
  - contains
  - regex
  - matches
tags:
  - reference
order: 4
see-also:
  - configuration
  - pdf
  - zip
examples:
  - name: rules
    kind: source
    language: yaml
    save-as: text.yaml
    description: Text rules of every kind, in two tests.
    content: |
      tests:
        - name: Report
          files:
            - pattern: report.md
              text:
                - contains: 'Status: Passed'
                - does-not-contain: TODO
                - matches: '(?m)^Version: \d+\.\d+\.\d+'
        - name: Draft
          tags:
            - strict
          files:
            - pattern: report.md
              text:
                - does-not-contain-regex: 'Version: \d'
  - name: check
    kind: command
    language: shell
    description: Run the test Report. Exits 0 when every rule passes.
    content: '{{tool}} --config text.yaml Report'
  - name: forbidden
    kind: command
    language: shell
    description: Run the tests tagged strict. The report has a version, so this exits 1.
    content: '{{tool}} --config text.yaml strict'
    exit: 1
    expect: 'matches forbidden pattern'
---
A `text` block is a list of rules. Every matching file is read as UTF-8
text and checked against every rule.

| Rule | Passes when the file |
| --- | --- |
| `contains` | contains the text. |
| `does-not-contain` | does not contain the text. |
| `matches` | has a match for the regular expression. |
| `does-not-contain-regex` | has no match for the regular expression. |

**Text** is compared exactly: the case matters, and nothing is trimmed.

**Regular expressions** use the .NET syntax and can match anywhere in
the file. `^` and `$` are the start and the end of the whole file, unless
the expression starts with `(?m)`, which makes them match at every line.
Write expressions in single quotes in YAML, so that `\d` stays as it is.

**One key per rule.** Give every rule its own list item. When an item has
several keys, only the first of `contains`, `does-not-contain`,
`matches`, and `does-not-contain-regex` applies.

The same rules check the text of PDF pages and of the entries of zip
archives.
