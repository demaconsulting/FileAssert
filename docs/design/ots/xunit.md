## xUnit OTS Design

xUnit is the .NET unit-testing framework used by the FileAssert test project. It provides test
discovery, execution, and result reporting including TRX output for requirements traceability.

### Purpose

xUnit discovers all test methods annotated with `[Fact]` in the test project, executes them, and
reports pass/fail results. The xunit.runner.visualstudio adapter generates TRX result files that
ReqStream consumes to verify requirements coverage. Passing tests provide continuous traceability
evidence that FileAssert's functional requirements are implemented correctly.

xUnit v3 is chosen because it provides a modern, self-contained test runner with
`OutputType: Exe` support for .NET 8/9/10, strong assertion APIs, and the
`xunit.runner.visualstudio` adapter for TRX output format that ReqStream requires.

### Integration

xUnit is integrated via NuGet package references in the test project
(`DemaConsulting.FileAssert.Tests.csproj`):

- `xunit.v3` version `3.2.2` — the core test framework providing `[Fact]`, assertions, and
  test runner infrastructure for .NET 8, 9, and 10.
- `xunit.runner.visualstudio` version `3.1.5` — the Visual Studio and `dotnet test` adapter
  that enables TRX result file output.

Tests are executed by `dotnet test` with the `--logger trx;LogFileName=<name>.trx` argument to
produce TRX files for ReqStream. The test project targets `net8.0`, `net9.0`, and `net10.0`
matching the supported runtime targets of the main project.

### Configuration

xUnit behavior is controlled through `dotnet test` command-line arguments. The test project
is configured with:

- `OutputType: Exe` — required for xUnit v3 self-contained test executables.
- `IsTestProject: true` — marks the project for MSBuild and the .NET test SDK.
- `TreatWarningsAsErrors: true` — enforces code quality at compile time.

No `xunit.runner.json` file is required; default discovery and execution settings are used.
`Microsoft.NET.Test.Sdk` version `18.5.1` provides the test SDK integration layer.

### Interfaces

xUnit exposes the following APIs consumed by the project:

| API                                  | Usage                                                          |
| :----------------------------------- | :------------------------------------------------------------- |
| `[Fact]` attribute                   | Marks a method as a test case for discovery and execution      |
| `[Collection]` attribute             | Groups tests that share a fixture or must not run in parallel  |
| `Assert.Equal`, `Assert.True`, etc.  | Assertion methods used throughout all test methods             |
| `dotnet test --logger trx`           | Produces TRX output consumed by ReqStream                      |

### Dependencies

xUnit brings the following dependencies into the test project:

- `xunit.v3.core` — the test execution engine and assertion library.
- `xunit.v3.common` — shared abstractions used by the xUnit framework.
- `xunit.runner.visualstudio` — the `dotnet test` integration adapter.
- `Microsoft.NET.Test.Sdk` version `18.5.1` — the test SDK integration layer.

All xUnit and runner dependencies are scoped to the test project via `PrivateAssets` settings
and do not propagate to the main `DemaConsulting.FileAssert` project or its NuGet package
consumers.
