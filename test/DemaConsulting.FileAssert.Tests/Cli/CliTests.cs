// Copyright (c) DEMA Consulting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DemaConsulting.FileAssert.Cli;

namespace DemaConsulting.FileAssert.Tests.Cli;

/// <summary>
///     Subsystem tests for the Cli subsystem.
/// </summary>
[Collection("Sequential")]
public class CliTests
{
    /// <summary>
    ///     Verifies that the Cli subsystem correctly parses the --silent, --validate, and --log flags.
    /// </summary>
    [Fact]
    public void Cli_CreateContext_ParsesSilentValidateAndLogFlags()
    {
        // Arrange
        var tempDir = Directory.CreateTempSubdirectory("fileassert_cli_");
        try
        {
            var logPath = Path.Combine(tempDir.FullName, "out.log");

            // Act - create a context with the silent, validate, and log flags
            using (var context = Context.Create(
            [
                "--silent",
                "--validate",
                "--log", logPath
            ]))
            {
                // Assert - all flags are reflected in the context properties
                Assert.True(context.Silent);
                Assert.True(context.Validate);
                Assert.False(context.Version);
                Assert.False(context.Help);
                Assert.Equal(".fileassert.yaml", context.ConfigFile);
                Assert.Equal(0, context.ExitCode);
            }
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that the Cli subsystem correctly parses --version, --help, --config, and --results flags.
    /// </summary>
    [Fact]
    public void Cli_CreateContext_ParsesVersionHelpConfigResultsFlags()
    {
        // Arrange
        var tempDir = Directory.CreateTempSubdirectory("fileassert_cli_");
        try
        {
            var configPath = Path.Combine(tempDir.FullName, "custom.yaml");
            var resultsPath = Path.Combine(tempDir.FullName, "results.trx");

            // Act - create a context with the version, help, config, and results flags
            using var context = Context.Create(
            [
                "--version",
                "--help",
                "--config", configPath,
                "--results", resultsPath
            ]);

            // Assert - all flags are reflected in the context properties
            Assert.True(context.Version);
            Assert.True(context.Help);
            Assert.Equal(configPath, context.ConfigFile);
            Assert.Equal(resultsPath, context.ResultsFile);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that the Cli subsystem captures positional arguments as test name/tag filters.
    /// </summary>
    [Fact]
    public void Cli_CreateContext_WithFilters_ParsesPositionalArguments()
    {
        // Arrange & Act
        using var context = Context.Create(["--silent", "smoke", "regression"]);

        // Assert - positional arguments are captured in the Filters collection
        Assert.Equal(2, context.Filters.Count);
        Assert.Equal("smoke", context.Filters[0]);
        Assert.Equal("regression", context.Filters[1]);
    }

    /// <summary>
    ///     Verifies that the Cli subsystem throws ArgumentException for unknown flags.
    /// </summary>
    [Fact]
    public void Cli_CreateContext_UnknownArgument_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => Context.Create(["--unknown-flag"]));
    }

    /// <summary>
    ///     Verifies that WriteError changes the context exit code from 0 to 1.
    /// </summary>
    [Fact]
    public void Cli_WriteError_ChangesExitCodeToOne()
    {
        // Arrange
        using var context = Context.Create(["--silent"]);
        Assert.Equal(0, context.ExitCode);

        // Act
        context.WriteError("something went wrong");

        // Assert
        Assert.Equal(1, context.ExitCode);
    }

    /// <summary>
    ///     Verifies that the Cli subsystem routes both informational and error messages
    ///     through the log file when a log path is specified.
    /// </summary>
    [Fact]
    public void Cli_OutputPipeline_WritesMessagesToLogFile()
    {
        // Arrange
        var tempDir = Directory.CreateTempSubdirectory("fileassert_cli_");
        try
        {
            var logPath = Path.Combine(tempDir.FullName, "out.log");

            // Act - create a silent context with logging, write messages, dispose to flush
            using (var context = Context.Create(["--silent", "--log", logPath]))
            {
                context.WriteLine("informational message");
                context.WriteError("error message");
            }

            // Assert - both messages appear in the log file
            var logContent = File.ReadAllText(logPath);
            Assert.Contains("informational message", logContent);
            Assert.Contains("error message", logContent);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
