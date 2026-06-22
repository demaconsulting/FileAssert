# Shared Package Integration Design

FileAssert uses one shared package from within the same program: an earlier released version of
itself. This section describes the integration strategy and usage design for this package.

## Integration Strategy

A **Shared Package** is a software package produced by the same program (repository), consumed as
a pinned released version rather than built from source. Unlike an OTS item, a shared package is
developed in-house; unlike in-scope source code, the consuming repository references its advertised
features rather than its internal design.

FileAssert consumes a previous release of itself as a dotnet global tool installed via
`.config/dotnet-tools.json`. This is a deliberate dogfooding practice: the CI pipeline uses a
stable released build of the tool to validate the in-development build's documentation outputs,
while the in-development build is what produces the next release.

## Shared Package Summary

| Package     | Role                                                                         |
| :---------- | :--------------------------------------------------------------------------- |
| FileAssert  | Validates generated HTML and PDF documents against acceptance criteria       |

## Per-Package Design

Detailed integration design is provided in the following section of this document:

- See _FileAssert Shared Package Design_ for the document assertion tool.
