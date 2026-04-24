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

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DemaConsulting.FileAssert.Cli;
using DemaConsulting.FileAssert.Utilities;
using DemaConsulting.TestResults.IO;

namespace DemaConsulting.FileAssert.SelfTest;

/// <summary>
///     Provides self-validation functionality for FileAssert.
/// </summary>
internal static partial class Validation
{
    /// <summary>
    ///     Runs self-validation tests and optionally writes results to a file.
    /// </summary>
    /// <param name="context">The context containing command line arguments and program state.</param>
    public static void Run(Context context)
    {
        // Validate input
        ArgumentNullException.ThrowIfNull(context);

        // Print validation header
        PrintValidationHeader(context);

        // Create test results collection
        var testResults = new DemaConsulting.TestResults.TestResults
        {
            Name = "FileAssert Self-Validation"
        };

        // Run core functionality tests
        RunVersionTest(context, testResults);
        RunHelpTest(context, testResults);
        RunResultsTest(context, testResults);
        RunExistsTest(context, testResults);
        RunContainsTest(context, testResults);

        // Calculate totals
        var totalTests = testResults.Results.Count;
        var passedTests = testResults.Results.Count(t => t.Outcome == DemaConsulting.TestResults.TestOutcome.Passed);
        var failedTests = testResults.Results.Count(t => t.Outcome == DemaConsulting.TestResults.TestOutcome.Failed);

        // Print summary
        context.WriteLine("");
        context.WriteLine($"Total Tests: {totalTests}");
        context.WriteLine($"Passed: {passedTests}");
        if (failedTests > 0)
        {
            context.WriteError($"Failed: {failedTests}");
        }
        else
        {
            context.WriteLine($"Failed: {failedTests}");
        }

        // Write results file if requested
        if (context.ResultsFile != null)
        {
            WriteResultsFile(context, testResults);
        }
    }

    /// <summary>
    ///     Prints the validation header with system information.
    /// </summary>
    /// <param name="context">The context for output.</param>
    private static void PrintValidationHeader(Context context)
    {
        var heading = new string('#', context.Depth);
        context.WriteLine($"{heading} DEMA Consulting FileAssert");
        context.WriteLine("");
        context.WriteLine("| Information         | Value                                              |");
        context.WriteLine("| :------------------ | :------------------------------------------------- |");
        context.WriteLine($"| Tool Version        | {Program.Version,-50} |");
        context.WriteLine($"| Machine Name        | {Environment.MachineName,-50} |");
        context.WriteLine($"| OS Version          | {RuntimeInformation.OSDescription,-50} |");
        context.WriteLine($"| DotNet Runtime      | {RuntimeInformation.FrameworkDescription,-50} |");
        context.WriteLine($"| Time Stamp          | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC{"",-29} |");
        context.WriteLine("");
    }

    /// <summary>
    ///     Runs a test for version display functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunVersionTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "FileAssert_VersionDisplay", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "version-test.log");

            // Run the program capturing output to a log file
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--log", logFile, "--version"]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            if (exitCode != 0)
            {
                return $"Program exited with code {exitCode}";
            }

            // Verify version string is present in the log (must match N.N.N semantic version format)
            var logContent = File.ReadAllText(logFile);
            return (!string.IsNullOrWhiteSpace(logContent) && VersionRegex().IsMatch(logContent))
                ? null : "Version string not found in log";
        });
    }

    /// <summary>
    ///     Runs a test for help display functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunHelpTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "FileAssert_HelpDisplay", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "help-test.log");

            // Run the program capturing output to a log file
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--log", logFile, "--help"]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            if (exitCode != 0)
            {
                return $"Program exited with code {exitCode}";
            }

            // Verify expected help headings are present in the log
            var logContent = File.ReadAllText(logFile);
            return (logContent.Contains("Usage:") && logContent.Contains("Options:"))
                ? null : "Help text not found in log";
        });
    }

    /// <summary>
    ///     Runs a test for results generation functionality with both passing and failing tests.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunResultsTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "FileAssert_Results", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var configFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, ".fileassert.yaml");
            var resultsFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "results.trx");

            // Create a file that will satisfy the passing test
            File.WriteAllText(PathHelpers.SafePathCombine(tempDir.DirectoryPath, "present.txt"), "present");

            // Write a config with one passing test (present.txt exists) and one failing test (absent.txt missing)
            File.WriteAllText(configFile,
                """
                tests:
                  - name: FileAssert_Results_Pass
                    files:
                      - pattern: "present.txt"
                        count: 1
                  - name: FileAssert_Results_Fail
                    files:
                      - pattern: "absent.txt"
                        count: 1
                """);

            // Run the program
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--config", configFile, "--results", resultsFile]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            // The failing test must produce a non-zero exit code and the --results flag must have
            // caused a results file to be created
            if (exitCode == 0)
            {
                return "Expected non-zero exit code for failing test configuration";
            }

            return File.Exists(resultsFile) ? null : "Results file was not created";
        });
    }

    /// <summary>
    ///     Runs a test for file-existence checking via glob pattern.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunExistsTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "FileAssert_Exists", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var configFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, ".fileassert.yaml");

            // Create a text file that the glob pattern should match
            File.WriteAllText(PathHelpers.SafePathCombine(tempDir.DirectoryPath, "hello.txt"), "Hello World");

            // Write a config that verifies exactly one .txt file exists in the directory
            File.WriteAllText(configFile,
                """
                tests:
                  - name: FileAssert_Exists_Test
                    files:
                      - pattern: "*.txt"
                        count: 1
                """);

            // Run the program
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--config", configFile]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            return exitCode == 0 ? null : $"Program exited with code {exitCode}";
        });
    }

    /// <summary>
    ///     Runs a test for file-contains checking.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunContainsTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "FileAssert_Contains", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var configFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, ".fileassert.yaml");

            // Create a text file with known content for the contains assertion
            File.WriteAllText(PathHelpers.SafePathCombine(tempDir.DirectoryPath, "hello.txt"), "Hello World");

            // Write a config that verifies the file contains the expected text
            File.WriteAllText(configFile,
                """
                tests:
                  - name: FileAssert_Contains_Test
                    files:
                      - pattern: "*.txt"
                        text:
                          - contains: "Hello World"
                """);

            // Run the program
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--config", configFile]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            return exitCode == 0 ? null : $"Program exited with code {exitCode}";
        });
    }

    /// <summary>
    ///     Runs a single validation test, recording the outcome in the test results collection.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    /// <param name="testName">The name of the test.</param>
    /// <param name="testBody">
    ///     A function that performs the test logic. Returns <c>null</c> on success, or an error
    ///     message string on failure.
    /// </param>
    private static void RunValidationTest(
        Context context,
        DemaConsulting.TestResults.TestResults testResults,
        string testName,
        Func<string?> testBody)
    {
        // Record when the test started so duration can be calculated at the end
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult(testName);

        try
        {
            // Execute the test body and interpret null as success, non-null as failure
            var errorMessage = testBody();
            if (errorMessage == null)
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                context.WriteLine($"✓ {testName} - Passed");
            }
            else
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                test.ErrorMessage = errorMessage;
                context.WriteError($"✗ {testName} - Failed: {errorMessage}");
            }
        }
        // Generic catch is justified here as this is a test framework - any exception should be
        // recorded as a test failure to ensure robust test execution and reporting.
        catch (Exception ex)
        {
            test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
            test.ErrorMessage = $"Exception: {ex.Message}";
            context.WriteError($"✗ {testName} - Failed: {ex.Message}");
        }

        test.Duration = DateTime.UtcNow - startTime;
        testResults.Results.Add(test);
    }

    /// <summary>
    ///     Writes test results to a file in TRX or JUnit format.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results to write.</param>
    private static void WriteResultsFile(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        if (context.ResultsFile == null)
        {
            return;
        }

        try
        {
            var extension = Path.GetExtension(context.ResultsFile).ToLowerInvariant();
            string content;

            if (extension == ".trx")
            {
                content = TrxSerializer.Serialize(testResults);
            }
            else if (extension == ".xml")
            {
                // Assume JUnit format for .xml extension
                content = JUnitSerializer.Serialize(testResults);
            }
            else
            {
                context.WriteError($"Error: Unsupported results file format '{extension}'. Use .trx or .xml extension.");
                return;
            }

            File.WriteAllText(context.ResultsFile, content);
            context.WriteLine($"Results written to {context.ResultsFile}");
        }
        // Generic catch is justified here as a top-level handler to log file write errors
        catch (Exception ex)
        {
            context.WriteError($"Error: Failed to write results file: {ex.Message}");
        }
    }

    /// <summary>
    ///     Creates a new test result object with common properties.
    /// </summary>
    /// <param name="testName">The name of the test.</param>
    /// <returns>A new test result object.</returns>
    private static DemaConsulting.TestResults.TestResult CreateTestResult(string testName)
    {
        return new DemaConsulting.TestResults.TestResult
        {
            Name = testName,
            ClassName = "Validation",
            CodeBase = "FileAssert"
        };
    }

    /// <summary>
    ///     Represents a temporary directory that is automatically deleted when disposed.
    /// </summary>
    private sealed class TemporaryDirectory : IDisposable
    {
        /// <summary>
        ///     Gets the path to the temporary directory.
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemporaryDirectory"/> class.
        /// </summary>
        public TemporaryDirectory()
        {
            DirectoryPath = PathHelpers.SafePathCombine(Path.GetTempPath(), $"fileassert_validation_{Guid.NewGuid()}");

            try
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                throw new InvalidOperationException($"Failed to create temporary directory: {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Deletes the temporary directory and all its contents.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Ignore cleanup errors during disposal
            }
        }
    }

    /// <summary>
    ///     Source-generated regex for matching semantic version strings (N.N.N format).
    /// </summary>
    [GeneratedRegex(@"\d+\.\d+\.\d+")]
    private static partial Regex VersionRegex();
}
