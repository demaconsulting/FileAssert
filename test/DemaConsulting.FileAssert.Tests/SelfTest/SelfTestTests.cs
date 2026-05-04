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
using DemaConsulting.FileAssert.SelfTest;

namespace DemaConsulting.FileAssert.Tests.SelfTest;

/// <summary>
///     Subsystem tests for the SelfTest subsystem.
/// </summary>
[Collection("Sequential")]
public class SelfTestTests
{
    /// <summary>
    ///     Verifies that the SelfTest subsystem runs all built-in tests and produces
    ///     a summary that includes pass and fail counts.
    /// </summary>
    [Fact]
    public void SelfTest_Run_ExecutesBuiltInTestsAndProducesSummary()
    {
        // Arrange
        var tempDir = Directory.CreateTempSubdirectory("fileassert_selftest_");
        try
        {
            var logPath = Path.Combine(tempDir.FullName, "validation.log");
            int exitCode;

            using (var context = Context.Create(["--silent", "--log", logPath]))
            {
                // Act
                Validation.Run(context);

                // Capture exit code before disposal
                exitCode = context.ExitCode;
            }

            // Assert - context is disposed above so the log file is fully flushed and closed
            Assert.Equal(0, exitCode);

            var logContent = File.ReadAllText(logPath);
            Assert.Contains("Total Tests:", logContent);
            Assert.Contains("Passed:", logContent);
            Assert.Contains("Failed:", logContent);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that the SelfTest subsystem prints a system information header.
    /// </summary>
    [Fact]
    public void SelfTest_Run_WhenInvoked_PrintsSystemInfoHeader()
    {
        // Arrange
        var tempDir = Directory.CreateTempSubdirectory("fileassert_selftest_");
        try
        {
            var logPath = Path.Combine(tempDir.FullName, "validation.log");

            using (var context = Context.Create(["--silent", "--log", logPath]))
            {
                // Act
                Validation.Run(context);
            }

            // Assert - system information header must appear in the log
            var logContent = File.ReadAllText(logPath);
            Assert.Contains("Tool Version", logContent);
            Assert.Contains("Machine Name", logContent);
            Assert.Contains("OS Version", logContent);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that the SelfTest subsystem writes a TRX results file when --results is specified.
    /// </summary>
    [Fact]
    public void SelfTest_Run_WithResultsFile_WritesTrxResultsFile()
    {
        // Arrange
        var tempDir = Directory.CreateTempSubdirectory("fileassert_selftest_");
        try
        {
            var resultsPath = Path.Combine(tempDir.FullName, "results.trx");

            using (var context = Context.Create(["--silent", "--results", resultsPath]))
            {
                // Act
                Validation.Run(context);
            }

            // Assert - TRX results file must exist and contain test result content
            Assert.True(File.Exists(resultsPath), "TRX results file should be created");
            var content = File.ReadAllText(resultsPath);
            Assert.Contains("TestRun", content);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
