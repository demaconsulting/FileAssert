### IContext Design

#### Overview

`IContext` is the output contract interface for reporting assertion results and errors within
FileAssert. It is implemented by `Context` (the root context) and by `Context.ScopedContext` (a
scoped wrapper that prepends a path prefix to every error message).

Accepting `IContext` in asserter `Run` methods and in `FileAssertFile.Run` allows
`FileAssertZipAssert` to pass a scoped context to nested asserters when processing zip archive
entries, without those asserters requiring any knowledge of the scoping mechanism.

#### Interface Members

```csharp
internal interface IContext
{
    void WriteLine(string message);
    void WriteError(string message);
    IContext WithPrefix(string prefix);
}
```

| Member                         | Description                                                                       |
| :----------------------------- | :-------------------------------------------------------------------------------- |
| `WriteLine(string message)`    | Writes an informational output line. Does not affect the error state.             |
| `WriteError(string message)`   | Writes an error message and marks the context as having errors.                   |
| `WithPrefix(string prefix)`    | Returns a new scoped context that prepends `"{prefix} > "` to all error messages. |

#### ScopedContext

`ScopedContext` is a private sealed nested class inside `Context`. It implements `IContext` and holds
a reference to its parent `IContext`. All calls are delegated:

- `WriteLine` — delegated unchanged to the parent.
- `WriteError` — the message is prefixed with `"{_prefix} > "` before being passed to the parent.
- `WithPrefix` — creates a further-nested `ScopedContext` wrapping `this`, enabling arbitrary breadcrumb depth.

This chain ensures that all scoped errors ultimately reach the root `Context` and increment its
`ErrorCount` and `ExitCode`.

#### Design Rationale

- **Interface not abstract class**: Using an interface rather than a base class avoids inheritance
  hierarchies and allows `ScopedContext` to be a lightweight private nested class with no
  instance state beyond a prefix string and a parent reference.
- **Prefix as breadcrumb**: Scoped error messages produced while asserting a zip entry appear as
  `"archive.zip > entry.xml > error message"`, giving users immediate context about which archive
  and entry caused the failure without requiring any special formatting in the asserter itself.
- **`WithPrefix` on `IContext`**: Declaring `WithPrefix` on the interface rather than only on
  `Context` means that asserters can scope further without a cast, supporting arbitrarily deep
  nesting (e.g., zip-in-zip-in-zip).

#### Data Model

`IContext` carries no instance data. `ScopedContext` holds:

| Field     | Type       | Description                                           |
| :-------- | :--------- | :---------------------------------------------------- |
| `_parent` | `IContext` | The parent context to delegate all calls to.          |
| `_prefix` | `string`   | The prefix prepended to every `WriteError` message.   |

#### Key Methods

| Method                          | Description                                       |
| :------------------------------ | :------------------------------------------------ |
| `WriteLine(string)`             | Informational output, no error state change.      |
| `WriteError(string)`            | Error message with prefix, delegates to parent.   |
| `WithPrefix(string) → IContext` | Returns a new nested `ScopedContext`.             |

#### Error Handling

| Scenario                           | Handling                                                              |
| :--------------------------------- | :-------------------------------------------------------------------- |
| Null prefix passed to `WithPrefix` | `ArgumentNullException` thrown before construction of scoped context. |

#### Dependencies

- No external dependencies. `IContext` and `ScopedContext` are self-contained within the `Cli`
  namespace.

#### Callers

- `FileAssertFile.Run` — accepts `IContext` instead of `Context`.
- All 7 asserters (`FileAssertTextAssert`, `FileAssertXmlAssert`, `FileAssertHtmlAssert`,
  `FileAssertYamlAssert`, `FileAssertJsonAssert`, `FileAssertPdfAssert`,
  `FileAssertZipAssert`) — each `Run` method accepts `IContext`.
- `FileAssertZipAssert.Run` — calls `context.WithPrefix(displayPath)` before passing the
  scoped context to nested file assertions inside a zip archive.
- `FileAssertTest.Run` — accepts `IContext` and passes it down the assertion chain.
