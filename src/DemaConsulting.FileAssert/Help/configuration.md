---
title: Configuration file
summary: 'The YAML test suite: tests, tags, file patterns, counts, and sizes.'
aliases:
  - config
  - format
  - suite
  - tests
tags:
  - reference
order: 3
see-also:
  - getting-started
  - troubleshooting
examples:
  - name: sample
    kind: source
    language: yaml
    save-as: fileassert.yaml
    description: A test suite with two tests, counts, sizes, and content checks.
    content: |
      tests:
        - name: Documentation
          tags:
            - docs
          files:
            - pattern: report.md
              count: 1
              min-size: 10
              text:
                - contains: 'Status: Passed'
            - pattern: '**/*.tmp'
              max: 0
        - name: Package
          tags:
            - release
          files:
            - pattern: app.json
              count: 1
              json:
                - query: version
                  count: 1
  - name: run
    kind: command
    language: shell
    description: Run the test suite. Exits 0 when every assertion passes.
    content: '{{tool}} --config fileassert.yaml'
  - name: size-limit
    kind: source
    language: yaml
    save-as: strict.yaml
    description: A test suite with a size limit that the report exceeds.
    content: |
      tests:
        - name: Small
          files:
            - pattern: report.md
              max-size: 10
  - name: too-large
    kind: command
    language: shell
    description: Run it. The failure names the file and its size. Exits 1.
    content: '{{tool}} --config strict.yaml'
    exit: 1
    expect: 'exceeds the maximum 10 bytes'
---
A test suite is a YAML file with one key, `tests`, which holds a list of
tests. [[#sample]] prints a complete one.

| Key | Meaning |
| --- | --- |
| `name` | Required. The name of the test, which a filter can select. |
| `tags` | A list of tags, which a filter can also select. |
| `files` | The list of file entries of the test. |
| `pattern` | Required in a file entry. A glob, such as `docs/**/*.md`. |
| `min`, `max`, `count` | At least, at most, or exactly this many files match. |
| `min-size`, `max-size` | The size limits of every matching file, in bytes. |

**Patterns.** A pattern is relative to the folder of the configuration
file, not to the current folder. `*` matches within one folder and `**`
matches any number of folders. A `\` in a pattern is read as `/`.

**Counts.** `min`, `max`, and `count` are checked in that order. The first
that fails reports the number of matches and skips the other checks of the
entry. A pattern without `min` or `count` passes when no file matches.

**Content.** A file entry can also check the content of each matching
file with these blocks:

- `text`: text and regular expressions; see [[text]].
- `xml` and `html`: XPath queries; see [[xml-html]].
- `json` and `yaml`: dot-separated paths; see [[json-yaml]].
- `pdf`: metadata, pages, and text; see [[pdf]].
- `zip`: the entries of an archive; see [[zip]].

**Results.** A test fails when any of its checks fails, and `--results`
writes one result per test.

**Unknown keys** are ignored, so a misspelled key checks nothing; see
[[troubleshooting]].
