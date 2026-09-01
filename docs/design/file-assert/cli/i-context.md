### IContext Design

![Cli Structure](CliView.svg)

#### Overview

`IContext` is the output contract interface for reporting assertion results and errors within
FileAssert. It is implemented by `Context` and accepted by all asserters so that their reporting
logic is decoupled from the concrete `Context` implementation.

#### Purpose

`IContext` exists to decouple the asserters from the concrete output and error-tracking
implementation. By depending only on this interface, each asserter can write informational
output and errors without knowing the concrete implementation it holds (console, log file, or a
future alternative).

#### Interface Members

```csharp
internal interface IContext
{
    void WriteLine(string message);
    void WriteError(string message);
}
```

| Member                         | Description                                                                       |
| :----------------------------- | :-------------------------------------------------------------------------------- |
| `WriteLine(string message)`    | Writes an informational output line. Does not affect the error state.             |
| `WriteError(string message)`   | Writes an error message and marks the context as having errors.                   |

#### Design Rationale

- **Interface not abstract class**: Using an interface rather than a base class avoids inheritance
  hierarchies and keeps the contract minimal.

#### Data Model

`IContext` carries no instance data.

#### Dependencies

- No external dependencies. `IContext` is self-contained within the `Cli` namespace.

#### Callers

- `FileAssertFile.Run` — accepts `IContext` instead of `Context`.
- All 7 asserters (`FileAssertTextAssert`, `FileAssertXmlAssert`, `FileAssertHtmlAssert`,
  `FileAssertYamlAssert`, `FileAssertJsonAssert`, `FileAssertPdfAssert`,
  `FileAssertZipAssert`) — each `Run` method accepts `IContext`.
- `FileAssertTest.Run` — accepts `IContext` and passes it down the assertion chain.
