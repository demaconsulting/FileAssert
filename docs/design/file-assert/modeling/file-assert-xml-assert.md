### FileAssertXmlAssert Design

#### Overview

The `FileAssertXmlAssert` class attempts to parse a matched file as an XML document using
`System.Xml.Linq` (`XDocument.Load`). If parsing fails, an error is reported and no further
assertions are evaluated. Otherwise it evaluates each XPath query against the document and
applies min, max, and exact count constraints to the number of matching nodes.

#### Class Structure

##### FileAssertXmlAssert

The main class coordinating XPath-based node count assertions for an XML file.

###### FileAssertXmlAssert Properties

| Property  | Type                                | Description             |
| :-------- | :---------------------------------- | :---------------------- |
| `Queries` | `IReadOnlyList<FileAssertXmlQuery>` | XPath query assertions. |

Each `FileAssertXmlQuery` entry holds:

| Property | Type     | Description                      |
| :------- | :------- | :------------------------------- |
| `Query`  | `string` | XPath expression to evaluate.    |
| `Count`  | `int?`   | Exact number of matched nodes.   |
| `Min`    | `int?`   | Minimum number of matched nodes. |
| `Max`    | `int?`   | Maximum number of matched nodes. |

###### FileAssertXmlAssert Factory

```csharp
internal static FileAssertXmlAssert Create(IEnumerable<FileAssertQueryData> data)
```

###### FileAssertXmlAssert Run

```csharp
internal void Run(Context context, string fileName)
```

Execution proceeds in the following steps:

1. Attempts to load the file using `XDocument.Load(fileName)`.
2. If an exception is thrown, writes the error below and returns immediately.
3. For each query entry: evaluates the XPath expression against the document using
   `System.Xml.XPath` extension methods, counts the matching nodes, and applies
   `Count`, `Min`, and `Max` constraints against the match count.

###### FileAssertXmlAssert Parse Error Message

```text
File '<fileName>' could not be parsed as an XML document
```

###### FileAssertXmlAssert Query Error Messages

```text
File '<fileName>' query '<query>' returned <n> result(s) which is below the minimum of <Min>
File '<fileName>' query '<query>' returned <n> result(s) which exceeds the maximum of <Max>
File '<fileName>' query '<query>' returned <n> result(s) but expected exactly <Count>
```

#### YAML Configuration

```yaml
files:
  - pattern: "**/*.xml"
    xml:
      - query: "//dependency"
        min: 1
      - query: "//plugin[artifactId='maven-compiler-plugin']"
        count: 1
```

#### Design Decisions

- **No additional dependencies**: `System.Xml.Linq` and `System.Xml.XPath` are part of the
  .NET BCL, so XML assertions require no additional NuGet packages.
- **Immediate failure on parse error**: Attempting to evaluate XPath queries against a file
  that is not valid XML would produce meaningless or misleading results. Reporting the parse
  failure immediately gives users a clear, actionable error message.
- **Independent query model**: `FileAssertXmlQuery` is private to this unit so that XML
  assertion behavior can evolve independently of the other structured-document assert units.

#### Purpose

`FileAssertXmlAssert` is responsible for validating one XML file against a list of XPath
queries. It parses the file with `System.Xml.Linq.XDocument` and enforces min, max, and
exact node-count constraints per query.

#### Data Model

| Field / Property | Type                                | Description                             |
| :--------------- | :---------------------------------- | :-------------------------------------- |
| `Queries`        | `IReadOnlyList<FileAssertXmlQuery>` | Ordered list of XPath query assertions. |

Each `FileAssertXmlQuery` (private nested record) holds:

| Property | Type     | Description                              |
| :------- | :------- | :--------------------------------------- |
| `Query`  | `string` | XPath expression to evaluate.            |
| `Count`  | `int?`   | Expected exact node count; `null` = N/A. |
| `Min`    | `int?`   | Minimum node count; `null` = no bound.   |
| `Max`    | `int?`   | Maximum node count; `null` = no bound.   |

#### Key Methods

| Method                                          | Purpose                                                       |
| :---------------------------------------------- | :------------------------------------------------------------ |
| `Create(IEnumerable<FileAssertQueryData> data)` | Converts query DTOs to `FileAssertXmlQuery` instances.        |
| `Run(Context context, string fileName)`         | Loads the XML file and evaluates each XPath query against it. |

#### Error Handling

| Scenario                                    | Handling                                                              |
| :------------------------------------------ | :-------------------------------------------------------------------- |
| `XDocument.Load` throws on parse failure    | Error written via `context.WriteError`; `Run` returns immediately.    |
| Query result below `Min`                    | Error written via `context.WriteError`; subsequent queries continue.  |
| Query result above `Max`                    | Error written via `context.WriteError`; subsequent queries continue.  |
| Query result not equal to `Count`           | Error written via `context.WriteError`; subsequent queries continue.  |

#### Interactions

- **Caller**: `FileAssertFile.Run` calls `XmlAssert.Run(context, fileName)` when the `xml:`
  assertion block is declared.
- **Created by**: `FileAssertFile.Create` via `FileAssertXmlAssert.Create`.
- **OTS dependency**: `System.Xml.Linq.XDocument` and `System.Xml.XPath` extension methods (BCL).
- **Configuration dependency**: `FileAssertQueryData` DTOs from the Configuration subsystem.
