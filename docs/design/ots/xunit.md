## xUnit OTS Design

xUnit is the .NET unit-testing framework used by the FileAssert test project. It provides test
discovery, execution, and result reporting including TRX output for requirements traceability.

### Purpose

xUnit discovers the test methods declared in the test project, executes them, and reports pass/fail
results. The `xunit.runner.visualstudio` adapter generates TRX result files that ReqStream consumes
to verify requirements coverage. Passing tests provide continuous traceability evidence that
FileAssert's functional requirements are implemented correctly.

xUnit v3 is chosen because it provides a modern, self-contained test runner with `OutputType: Exe`
support for .NET 8/9/10, strong assertion APIs, and the `xunit.runner.visualstudio` adapter for the
TRX output format that ReqStream requires.

### Features Used

- Test discovery and execution of methods annotated with the `[Fact]` and `[Theory]` attributes.
- The xUnit assertion library (`Assert.Equal`, `Assert.True`, `Assert.Throws`, etc.) used throughout
  the test methods.
- The `[Collection]` attribute to serialize tests that share temporary file-system state.
- TRX result-file generation through the `xunit.runner.visualstudio` adapter, driven by
  `dotnet test --logger trx;LogFileName=<name>.trx`, for ReqStream consumption.

### Integration Pattern

xUnit is integrated via NuGet package references in the test project
(`DemaConsulting.FileAssert.Tests.csproj`):

- `xunit.v3` — the core test framework providing `[Fact]`, `[Theory]`, assertions, and the test
  runner infrastructure for .NET 8, 9, and 10.
- `xunit.runner.visualstudio` — the Visual Studio and `dotnet test` adapter that enables TRX result
  file output.
- `Microsoft.NET.Test.Sdk` — the test SDK integration layer required by the VSTest/`dotnet test`
  host for discovery.

The test project is configured with `OutputType: Exe` (required for xUnit v3 self-contained test
executables), `IsTestProject: true` (marks the project for MSBuild and the .NET test SDK), and
`TreatWarningsAsErrors: true`. No `xunit.runner.json` file is required; default discovery and
execution settings are used. Tests target `net8.0`, `net9.0`, and `net10.0`, matching the supported
runtime targets of the main project, and are executed with
`dotnet test --logger trx;LogFileName=<name>.trx` to produce TRX files for ReqStream.

xUnit and its runner are confined to the test assembly by the test project boundary: the test
project references the production project, but the production `DemaConsulting.FileAssert` project and
its published NuGet package never reference the test project, so the xUnit packages cannot flow to
production consumers. This confinement is a consequence of the dependency direction between the
projects, not of any `PrivateAssets` markup on the package references.
