---
title: JSON and YAML assertions
summary: Count JSON and YAML values with dot-separated paths.
aliases:
  - json
  - yaml
tags:
  - reference
order: 6
see-also:
  - configuration
examples:
  - name: data
    kind: source
    language: yaml
    save-as: data.yaml
    description: Count values of a JSON file and of a YAML file.
    content: |
      tests:
        - name: Data
          files:
            - pattern: app.json
              json:
                - query: version
                  count: 1
                - query: files
                  count: 2
            - pattern: app.yaml
              yaml:
                - query: server.port
                  count: 1
                - query: features
                  min: 1
  - name: check
    kind: command
    language: shell
    description: Run the test suite. Exits 0 when every query passes.
    content: '{{tool}} --config data.yaml'
---
A `json` or a `yaml` block is a list of queries. A query is a path of
property names separated by dots, such as `server.port`, read from the
root of the document, and the number of values it must find:

| Key | Meaning |
| --- | --- |
| `query` | Required. The dot-separated path. |
| `count` | Exactly this many values. |
| `min`, `max` | At least, or at most, this many values. |

**Counting.** A path that does not exist counts 0, an array counts its
items, and any other value counts 1. A path cannot index an array, and a
path that starts or ends with a dot, or has two dots in a row, is an
error. `count` is checked first; when it fails, `min` and `max` are not
checked.

**JSON** files that cannot be parsed fail with
`could not be parsed as a JSON document`. A JSON query without `count`,
`min`, or `max` checks nothing.

**YAML** queries read the first document of the file; an empty file
counts 0 for every query. Every YAML query needs `count`, `min`, or
`max`, and a `yaml` block needs at least one query.
