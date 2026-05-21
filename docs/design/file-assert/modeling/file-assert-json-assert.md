### FileAssertJsonAssert Design

#### Overview

The `FileAssertJsonAssert` class attempts to parse a matched file as a JSON document using
`System.Text.Json.JsonDocument.Parse`. If parsing fails, an error is reported and no further
assertions are evaluated. Otherwise it evaluates each dot-notation path against the JSON
element tree and applies min, max, and exact count constraints to the number of matching
elements.

#### Class Structure

##### FileAssertJsonAssert

The main class coordinating dot-notation path assertions for a JSON file.

###### FileAssertJsonAssert Properties

| Field      | Type                          | Description                         |
| :--------- | :---------------------------- | :---------------------------------- |
| `_queries` | `IReadOnlyList<JsonQuery>`    | Dot-notation path query assertions. |

Each `JsonQuery` entry holds:

| Property | Type     | Description                      |
| :------- | :------- | :------------------------------- |
| `Query`  | `string` | Dot-notation path to evaluate.   |
| `Count`  | `int?`   | Exact number of matched nodes.   |
| `Min`    | `int?`   | Minimum number of matched nodes. |
| `Max`    | `int?`   | Maximum number of matched nodes. |

###### FileAssertJsonAssert Factory

```csharp
internal static FileAssertJsonAssert Create(IEnumerable<FileAssertQueryData> data)
```

###### FileAssertJsonAssert Run

```csharp
internal void Run(Context context, string fileName)
```

Execution proceeds in the following steps:

1. Reads the file content and calls `JsonDocument.Parse`.
2. If a `JsonException` is thrown, writes the error below and returns immediately.
3. For each query entry: traverses the JSON element tree following the dot-notation path
   segments, counts the matched properties or array elements, and applies `Count`, `Min`,
   and `Max` constraints against the match count.

###### FileAssertJsonAssert Parse Error Message

```text
File '<fileName>' could not be parsed as a JSON document
```

###### FileAssertJsonAssert Query Error Messages

```text
File '<fileName>' query '<query>' returned <n> result(s) which is below the minimum of <Min>
File '<fileName>' query '<query>' returned <n> result(s) which exceeds the maximum of <Max>
File '<fileName>' query '<query>' returned <n> result(s) but expected exactly <Count>
```

#### YAML Configuration

```yaml
files:
  - pattern: "appsettings.json"
    json:
      - query: "ConnectionStrings"
        min: 1
      - query: "Logging.LogLevel"
        count: 1
      - query: "AllowedHosts"
        count: 1
```

#### Design Decisions

- **No additional dependencies**: `System.Text.Json` is part of the .NET BCL, so JSON
  assertions require no additional NuGet packages.
- **Immediate failure on parse error**: Attempting to traverse a JSON document tree against
  a file that is not valid JSON would produce meaningless or misleading results. Reporting
  the parse failure immediately gives users a clear, actionable error message.
- **Dot-notation path traversal**: Segment-by-segment descent through JSON object properties.
  Array elements are counted at the terminal segment, allowing users to assert the presence
  and cardinality of array-valued keys.
- **Independent query model**: `JsonQuery` is private to this unit so that JSON
  assertion behavior can evolve independently of the other structured-document assert units.

#### Purpose

`FileAssertJsonAssert` is responsible for validating one JSON file against a list of
dot-notation path queries. It parses the file with `System.Text.Json.JsonDocument` and
enforces min, max, and exact element-count constraints per path.

#### Data Model

| Field / Property | Type                       | Description                                   |
| :--------------- | :------------------------- | :-------------------------------------------- |
| `_queries`       | `IReadOnlyList<JsonQuery>` | Ordered list of dot-notation path assertions. |

Each `JsonQuery` (private nested record) holds:

| Property | Type     | Description                                              |
| :------- | :------- | :------------------------------------------------------- |
| `Query`  | `string` | Dot-notation path to traverse.                           |
| `Count`  | `int?`   | Expected exact element count; `null` = N/A.              |
| `Min`    | `int?`   | Minimum element count; `null` = no bound.                |
| `Max`    | `int?`   | Maximum element count; `null` = no bound.                |

#### Key Methods

| Method                                          | Purpose                                                          |
| :---------------------------------------------- | :--------------------------------------------------------------- |
| `Create(IEnumerable<FileAssertQueryData> data)` | Factory: converts query DTOs to `JsonQuery` instances.           |
| `Run(Context context, string fileName)`         | Parses the JSON file and evaluates each dot-notation path query. |

#### Error Handling

| Scenario                                    | Handling                                                             |
| :------------------------------------------ | :------------------------------------------------------------------- |
| `JsonException` during `JsonDocument.Parse` | Error written via `context.WriteError`; `Run` returns immediately.   |
| Query result below `Min`                    | Error written via `context.WriteError`; subsequent queries continue. |
| Query result above `Max`                    | Error written via `context.WriteError`; subsequent queries continue. |
| Query result not equal to `Count`           | Error written via `context.WriteError`; subsequent queries continue. |

#### Interactions

- **Caller**: `FileAssertFile.Run` calls `JsonAssert.Run(context, fileName)` when the `json:`
  assertion block is declared.
- **Created by**: `FileAssertFile.Create` via `FileAssertJsonAssert.Create`.
- **OTS dependency**: `System.Text.Json.JsonDocument` (BCL) for parsing and element traversal.
- **Configuration dependency**: `FileAssertQueryData` DTOs from the Configuration subsystem.
