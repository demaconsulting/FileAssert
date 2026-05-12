### FileAssertZipAssert Design

#### Overview

The `FileAssertZipAssert` class validates the contents of a zip archive by matching entry names
against glob patterns and enforcing minimum and maximum count constraints. It is created from a
`FileAssertZipData` DTO and is invoked by `FileAssertFile` when a `zip:` assertion block is
declared. Wrapping zip entry validation in a dedicated unit keeps `FileAssertFile` free of
archive-inspection logic and makes the zip assertion pattern consistent with all other file-type
assert units.

#### Class Structure

##### Nested Class: Entry

The `Entry` nested class holds the compiled state for a single entry constraint:

| Property  | Type     | Description                                                         |
| :-------- | :------- | :------------------------------------------------------------------ |
| `Pattern` | `string` | Glob pattern used to match zip entry names.                         |
| `Min`     | `int?`   | Minimum number of entries that must match, or null for no bound.    |
| `Max`     | `int?`   | Maximum number of entries that may match, or null for no bound.     |

##### Properties

| Property  | Type                        | Description                                           |
| :-------- | :-------------------------- | :---------------------------------------------------- |
| `Entries` | `IReadOnlyList<Entry>`      | Entry constraints applied to the zip archive.         |

##### Factory Method

```csharp
internal static FileAssertZipAssert Create(FileAssertZipData data)
```

Converts each `FileAssertZipEntryData` DTO into an `Entry` instance after validating that a
pattern is specified.

| Parameter | Type                  | Description                                              |
| :-------- | :-------------------- | :------------------------------------------------------- |
| `data`    | `FileAssertZipData`   | Zip assertion block data from YAML configuration.        |

| Return / Exception           | Description                                                 |
| :--------------------------- | :---------------------------------------------------------- |
| Returns                      | A new `FileAssertZipAssert` instance.                       |
| `ArgumentNullException`      | Thrown when `data` is null.                                 |
| `InvalidOperationException`  | Thrown when any entry does not specify a pattern.           |

##### Run Method

```csharp
internal void Run(Context context, string fileName)
```

Opens the zip archive, collects all file entry names, and evaluates each entry constraint.

Execution proceeds in the following steps:

1. Attempts to open the zip archive with `ZipFile.OpenRead(fileName)`.
2. If an `IOException`, `InvalidDataException`, or `UnauthorizedAccessException` is thrown,
   writes the error below and returns immediately.
3. Enumerates all archive entries, normalizing separators to forward slashes and excluding
   directory entries (names ending with `/`).
4. For each configured `Entry`, uses `Matcher.Match(string.Empty, allEntries)` from
   `Microsoft.Extensions.FileSystemGlobbing` to count matched entries.
5. Writes an error if the match count is below `Min` or above `Max`.

###### Run Error Messages

```text
File '<fileName>' could not be read as a zip archive
```

```text
Zip '<fileName>' entry pattern '<pattern>' matched <count> entry(s),
but expected at least <min>
```

```text
Zip '<fileName>' entry pattern '<pattern>' matched <count> entry(s),
but expected at most <max>
```

| Parameter  | Type      | Description                            |
| :--------- | :-------- | :------------------------------------- |
| `context`  | `Context` | Reporting sink used to record errors.  |
| `fileName` | `string`  | Full path to the zip file to validate. |

#### YAML Configuration

Zip entry constraints are declared under the `zip:` key of a file entry:

```yaml
files:
  - pattern: "output/package.zip"
    zip:
      entries:
        - pattern: 'lib/net8.0/MyLib.dll'
          min: 1
          max: 1
        - pattern: 'lib/**/*.dll'
          min: 1
```

#### Design Decisions

- **Dedicated unit for zip validation**: Wrapping zip archive inspection in `FileAssertZipAssert`
  keeps `FileAssertFile` free of archive-handling logic and makes the pattern consistent with all
  other file-type assert units (`FileAssertTextAssert`, `FileAssertXmlAssert`, etc.).
- **Forward-slash normalization**: Zip entry names are normalized to forward slashes before
  matching so that glob patterns work consistently regardless of the creating platform.
- **Directory entry exclusion**: Entries whose names end with `/` are directory markers and are
  excluded from matching to avoid false counts from container entries.
- **Virtual root for Matcher**: `Matcher.Match(".", allEntries)` applies the glob
  pattern directly to the normalized entry name list without any filesystem path manipulation,
  because zip entry names are self-contained paths rather than paths relative to a directory root.
  The `"."` root is required because `InMemoryDirectoryInfo` rejects empty or null root paths.
- **Immediate failure on parse error**: If the file cannot be opened as a zip archive, an error
  is written immediately and no entry constraints are evaluated, consistent with the behavior of
  all other file-type assert units.
