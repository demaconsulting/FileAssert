---
title: Zip archive assertions
summary: Check the entries of zip archives with the same assertions as files.
aliases:
  - archive
  - nupkg
tags:
  - reference
order: 8
see-also:
  - configuration
examples:
  - name: package
    kind: source
    language: yaml
    save-as: zip.yaml
    description: Check the documents and the data of a package, and look for libraries.
    content: |
      tests:
        - name: Package
          files:
            - pattern: package.zip
              count: 1
              zip:
                files:
                  - pattern: 'docs/*.md'
                    count: 1
                    text:
                      - contains: 'Status: Passed'
                  - pattern: app.json
                    count: 1
                    json:
                      - query: version
                        count: 1
        - name: Binaries
          tags:
            - release
          files:
            - pattern: package.zip
              zip:
                files:
                  - pattern: '**/*.dll'
                    min: 1
  - name: check
    kind: command
    language: shell
    description: Run the test Package. Exits 0 when every entry passes.
    content: '{{tool}} --config zip.yaml Package'
  - name: missing-entry
    kind: command
    language: shell
    description: Run the tests tagged release. The package has no library, so this exits 1.
    content: '{{tool}} --config zip.yaml release'
    exit: 1
    expect: 'matched 0 file(s), but expected at least 1'
---
A `zip` block checks the entries of each matching archive. Its `files`
list has the same file entries as a test: a `pattern`, counts, sizes,
and content blocks, even a `zip` block for an archive inside the archive.
The list must have at least one entry.

**Patterns** match the paths of the entries inside the archive, such as
`docs/*.md`. Sizes are the uncompressed sizes of the entries.

**Any zip format** works, so the same block checks a NuGet package
(`.nupkg`) or another format that is a zip archive. A file that is not a
zip archive fails with `could not be read as a zip archive`.

**Messages** about an entry name the archive and the entry, separated by
`>`. A wrong number of entries names only the pattern.
