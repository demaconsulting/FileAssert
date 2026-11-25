---
title: XML and HTML assertions
summary: Count XML and HTML elements with XPath queries.
aliases:
  - xml
  - html
  - xpath
  - svg
tags:
  - reference
order: 5
see-also:
  - configuration
  - troubleshooting
examples:
  - name: markup
    kind: source
    language: yaml
    save-as: markup.yaml
    description: Count the rectangles of an SVG file and the links of an HTML page.
    content: |
      tests:
        - name: Markup
          files:
            - pattern: diagram.svg
              xml:
                - query: "//*[local-name()='rect']"
                  count: 3
            - pattern: index.html
              html:
                - query: //head/title
                  count: 1
                - query: '//a[@href]'
                  min: 2
  - name: check
    kind: command
    language: shell
    description: Run the test suite. Exits 0 when every query passes.
    content: '{{tool}} --config markup.yaml'
---
An `xml` or an `html` block is a list of queries. A query is an XPath
expression and the number of elements that it must select:

| Key | Meaning |
| --- | --- |
| `query` | Required. An XPath expression that selects elements. |
| `count` | Exactly this many elements. |
| `min`, `max` | At least, or at most, this many elements. |

`count` is checked first; when it fails, `min` and `max` are not checked.
A query without `count`, `min`, or `max` checks nothing.

**XML** files, which include SVG and other XML formats, must be well
formed, or the file fails with `could not be parsed as an XML document`.
A query must select elements: a query that selects attributes or text
stops the run with an error. No namespace prefixes are defined, so in a
document with a default namespace, such as SVG, `//rect` selects nothing;
write `//*[local-name()='rect']` instead.

**HTML** is read leniently, like a browser does, so broken markup is not
an error.

A query that is not valid XPath fails the file with
`is not a valid XPath expression`.
