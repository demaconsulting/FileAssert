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
using DemaConsulting.FileAssert.Modeling;
using DemaConsulting.TestResults.IO;
using YamlDotNet.Serialization;

namespace DemaConsulting.FileAssert.Configuration;

/// <summary>
///     Top-level configuration that loads a YAML file and runs its defined tests.
/// </summary>
internal sealed class FileAssertConfig
{
    /// <summary>
    ///     Path to the configuration file, used to compute the base directory for file patterns.
    /// </summary>
    private readonly string _configPath;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertConfig"/> class.
    /// </summary>
    /// <param name="configPath">Path to the configuration file.</param>
    /// <param name="tests">The tests loaded from the configuration.</param>
    private FileAssertConfig(string configPath, IReadOnlyList<FileAssertTest> tests)
    {
        // Store the config path so the base directory can be computed at run time
        _configPath = configPath;
        Tests = tests;
    }

    /// <summary>
    ///     Gets the list of tests defined in this configuration.
    /// </summary>
    internal IReadOnlyList<FileAssertTest> Tests { get; }

    /// <summary>
    ///     Reads and parses a YAML configuration file, returning a populated <see cref="FileAssertConfig"/>.
    /// </summary>
    /// <param name="path">Path to the YAML configuration file to load.</param>
    /// <returns>A new <see cref="FileAssertConfig"/> instance with all tests created.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified configuration file does not exist.</exception>
    internal static FileAssertConfig ReadFromFile(string path)
    {
        // Validate that a path was provided
        ArgumentNullException.ThrowIfNull(path);

        // Ensure the configuration file exists before attempting to read it
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Configuration file not found: {path}", path);
        }

        // Read the full YAML text from the configuration file
        var text = File.ReadAllText(path);

        // Build a deserializer that tolerates unknown properties for forward compatibility
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        // Deserialize the YAML into the data transfer object hierarchy
        var data = deserializer.Deserialize<FileAssertConfigData>(text) ?? new FileAssertConfigData();

        // Convert the raw data objects into fully-validated domain objects
        var tests = (data.Tests ?? [])
            .Select(FileAssertTest.Create)
            .ToList();

        // Return the configuration bound to its file path for base directory resolution
        return new FileAssertConfig(path, tests.AsReadOnly());
    }

    /// <summary>
    ///     Runs all tests that match the provided filters, reporting results via the context.
    /// </summary>
    /// <param name="context">The context used for reporting errors and output.</param>
    /// <param name="filters">
    ///     Names or tags used to select which tests to run.
    ///     An empty collection runs all tests.
    /// </param>
    internal void Run(Context context, IEnumerable<string> filters)
    {
        // Validate required parameters before executing any tests
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(filters);

        // Resolve the base directory from the absolute path of the configuration file
        var basePath = Path.GetDirectoryName(Path.GetFullPath(_configPath))
                       ?? Directory.GetCurrentDirectory();

        // Materialize the filter list to avoid repeated enumeration
        var filterList = filters.ToList();

        // Create a results collection when results serialization is requested
        DemaConsulting.TestResults.TestResults? testResults = context.ResultsFile != null
            ? new DemaConsulting.TestResults.TestResults { Name = "FileAssert" }
            : null;

        // Execute each test that matches the provided filters
        foreach (var test in Tests.Where(t => t.MatchesFilter(filterList)))
        {
            var startTime = DateTime.UtcNow;
            var errorsBefore = context.ErrorCount;

            test.Run(context, basePath);

            // Record the per-test outcome when results serialization is active
            testResults?.Results.Add(new DemaConsulting.TestResults.TestResult
            {
                Name = test.Name,
                ClassName = "FileAssert",
                CodeBase = "FileAssert",
                Outcome = context.ErrorCount > errorsBefore
                    ? DemaConsulting.TestResults.TestOutcome.Failed
                    : DemaConsulting.TestResults.TestOutcome.Passed,
                Duration = DateTime.UtcNow - startTime
            });
        }

        // Write the results file after all tests have completed
        if (testResults != null)
        {
            WriteResultsFile(context, testResults);
        }
    }

    /// <summary>
    ///     Writes test results to a file in TRX or JUnit format.
    /// </summary>
    /// <param name="context">The context for output and error reporting.</param>
    /// <param name="testResults">The collected test results to serialize.</param>
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
                context.WriteError(
                    $"Error: Unsupported results file format '{extension}'. Use .trx or .xml extension.");
                return;
            }

            File.WriteAllText(context.ResultsFile, content);
        }
        // Generic catch is justified here as a top-level handler to log file write errors
        catch (Exception ex)
        {
            context.WriteError($"Error: Failed to write results file: {ex.Message}");
        }
    }
}
