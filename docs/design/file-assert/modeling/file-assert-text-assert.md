# FileAssertTextAssert Design

## Overview

The `FileAssertTextAssert` class applies a collection of `FileAssertRule` instances to the
text content of a matched file. It is created from a list of `FileAssertRuleData` DTOs and
delegates rule application to `FileAssertRule.Apply`. Wrapping text rules in a dedicated
unit keeps `FileAssertFile` free of rule-application logic and makes the text assertion
pattern consistent with all other file-type assert units.

## Class Structure

### Properties

| Property | Type                            | Description                             |
| :------- | :------------------------------ | :-------------------------------------- |
| `Rules`  | `IReadOnlyList<FileAssertRule>` | Content rules applied to the file text. |

### Factory Method

```csharp
internal static FileAssertTextAssert Create(IEnumerable<FileAssertRuleData> data)
```

Creates a `FileAssertRule` for each entry in the data list via `FileAssertRule.Create`.

### Run Method

```csharp
internal void Run(Context context, string fileName)
```

Reads the entire file content as a UTF-8 string and applies each rule via
`rule.Apply(context, fileName, content)`.

## YAML Configuration

Text rules are declared under the `text:` key of a file entry:

```yaml
files:
  - pattern: "**/*.cs"
    text:
      - contains: "Copyright (c) DEMA Consulting"
      - does-not-contain: "password123"
      - matches: "Copyright \\(c\\) \\d{4}"
      - does-not-contain-regex: "FATAL|ERROR"
```

## Design Decisions

- **Dedicated unit for text rules**: Wrapping text rules in `FileAssertTextAssert` keeps
  `FileAssertFile` free of rule-application logic and makes the text assertion pattern
  consistent with all other file-type assert units (`FileAssertPdfAssert`,
  `FileAssertXmlAssert`, etc.).
- **Delegates to `FileAssertRule.Apply`**: Each rule is applied independently via the
  abstract `Apply` method, so all rule violations in a single file are reported in one pass
  without short-circuiting on the first failure.
