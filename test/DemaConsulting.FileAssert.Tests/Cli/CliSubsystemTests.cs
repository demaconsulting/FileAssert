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
[TestClass]
public class CliSubsystemTests
{
    /// <summary>
    ///     Verifies that the Cli subsystem correctly parses all supported flags
    ///     into structured properties on a single context.
    /// </summary>
    [TestMethod]
    public void CliSubsystem_CreateContext_ParsesAllSupportedFlags()
    {
        // Arrange - build an argument list with all supported flags
        var tempDir = Directory.CreateTempSubdirectory("fileassert_cli_");
        try
        {
            var logPath = Path.Combine(tempDir.FullName, "out.log");

            // Act - create a context with the silent, validate, and log flags
            using var context = Context.Create(
            [
                "--silent",
                "--validate",
                "--log", logPath
            ]);

            // Assert - all flags are reflected in the context properties
            Assert.IsTrue(context.Silent);
            Assert.IsTrue(context.Validate);
            Assert.IsFalse(context.Version);
            Assert.IsFalse(context.Help);
            Assert.AreEqual(".fileassert.yaml", context.ConfigFile);
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that the Cli subsystem routes both informational and error messages
    ///     through the log file when a log path is specified.
    /// </summary>
    [TestMethod]
    public void CliSubsystem_OutputPipeline_WritesMessagesToLogFile()
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
            Assert.IsTrue(logContent.Contains("informational message"));
            Assert.IsTrue(logContent.Contains("error message"));
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
