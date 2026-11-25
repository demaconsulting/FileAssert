---
title: PDF assertions
summary: Check PDF metadata, page counts, and the text of the pages.
aliases:
  - metadata
tags:
  - reference
order: 7
see-also:
  - text
  - configuration
examples:
  - name: manual
    kind: source
    language: yaml
    save-as: pdf.yaml
    description: Check the title, the page count, and the text of a manual.
    content: |
      tests:
        - name: Manual
          files:
            - pattern: manual.pdf
              count: 1
              pdf:
                metadata:
                  - field: Title
                    contains: User Manual
                pages:
                  min: 2
                text:
                  - contains: Installation
                  - contains: Troubleshooting
  - name: check
    kind: command
    language: shell
    description: Run the test suite. Exits 0 when every check passes.
    content: '{{tool}} --config pdf.yaml'
---
A `pdf` block has up to three parts, and every matching file must be a
PDF document, or it fails with `could not be parsed as a PDF document`.

**`metadata`** is a list of rules on the document information. Each rule
has a `field`, one of `Title`, `Author`, `Subject`, `Keywords`, `Creator`,
or `Producer` (the case matters), and `contains` (text, compared exactly)
or `matches` (a .NET regular expression), or both. A field that is not
set, or not in that list, fails every rule.

**`pages`** has `min` and `max`, the limits of the number of pages.

**`text`** is a list of text rules, as in a `text` block (see [[text]]),
checked against the text of all pages, joined with line breaks. Only text
that the PDF stores as text is found: a scanned page has none.
