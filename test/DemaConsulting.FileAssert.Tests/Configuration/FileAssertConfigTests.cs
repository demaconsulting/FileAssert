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
using DemaConsulting.FileAssert.Configuration;
using DemaConsulting.FileAssert.Modeling;

namespace DemaConsulting.FileAssert.Tests.Configuration;

/// <summary>
///     Unit tests for the <see cref="FileAssertConfig"/> class.
/// </summary>
[TestClass]
public class FileAssertConfigTests
{
    /// <summary>
    ///     Minimal valid YAML configuration used across multiple tests.
    /// </summary>
    private const string SimpleYaml = """
        tests:
          - name: "Test Alpha"
            files:
              - pattern: "alpha.txt"
          - name: "Test Beta"
            files:
              - pattern: "beta.txt"
        """;

    /// <summary>
    ///     Verifies that ReadFromFile successfully parses a valid YAML configuration file.
    /// </summary>
    [TestMethod]
    public void FileAssertConfig_ReadFromFile_ValidFile_ReturnsConfig()
    {
        // Arrange - write a minimal config to a temp file
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            var configPath = Path.Combine(tempDir.FullName, "config.yaml");
            File.WriteAllText(configPath, """
                tests:
                  - name: "Sample Test"
                    files:
                      - pattern: "*.txt"
                """);

            // Act
            var config = FileAssertConfig.ReadFromFile(configPath);

            // Assert
            Assert.AreEqual(1, config.Tests.Count);
            Assert.AreEqual("Sample Test", config.Tests[0].Name);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that ReadFromFile throws <see cref="FileNotFoundException"/> for a missing file.
    /// </summary>
    [TestMethod]
    public void FileAssertConfig_ReadFromFile_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange - construct a path that does not exist
        var missingPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.yaml");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => FileAssertConfig.ReadFromFile(missingPath));
    }

    /// <summary>
    ///     Verifies that ReadFromFile throws <see cref="ArgumentNullException"/> when path is null.
    /// </summary>
    [TestMethod]
    public void FileAssertConfig_ReadFromFile_NullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertConfig.ReadFromFile(null!));
    }

    /// <summary>
    ///     Verifies that Run with no filter executes all tests.
    /// </summary>
    [TestMethod]
    public void FileAssertConfig_Run_WithNoFilter_RunsAllTests()
    {
        // Arrange - create temp directory with files that satisfy both test patterns
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "alpha.txt"), "alpha content");
            File.WriteAllText(Path.Combine(tempDir.FullName, "beta.txt"), "beta content");

            var configPath = Path.Combine(tempDir.FullName, "config.yaml");
            File.WriteAllText(configPath, SimpleYaml);

            var config = FileAssertConfig.ReadFromFile(configPath);
            using var context = Context.Create(["--silent"]);

            // Act
            config.Run(context, []);

            // Assert - both tests ran and found their files with no errors
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run with a matching filter only executes the matching test.
    /// </summary>
    [TestMethod]
    public void FileAssertConfig_Run_WithMatchingFilter_RunsMatchingTest()
    {
        // Arrange - only create alpha.txt; beta.txt missing would cause an error if Test Beta ran
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "alpha.txt"), "alpha content");

            // beta.txt is intentionally absent; Test Beta with min=1 would fail if executed
            var configPath = Path.Combine(tempDir.FullName, "config.yaml");
            File.WriteAllText(configPath, """
                tests:
                  - name: "Test Alpha"
                    files:
                      - pattern: "alpha.txt"
                  - name: "Test Beta"
                    files:
                      - pattern: "beta.txt"
                        min: 1
                """);

            var config = FileAssertConfig.ReadFromFile(configPath);
            using var context = Context.Create(["--silent"]);

            // Act - filter to only "Test Alpha"
            config.Run(context, ["Test Alpha"]);

            // Assert - no error because Test Beta was skipped
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run with a non-matching filter skips all tests.
    /// </summary>
    [TestMethod]
    public void FileAssertConfig_Run_WithNonMatchingFilter_SkipsTests()
    {
        // Arrange - both patterns would fail if executed (files are absent with min=1)
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            var configPath = Path.Combine(tempDir.FullName, "config.yaml");
            File.WriteAllText(configPath, """
                tests:
                  - name: "Test Alpha"
                    files:
                      - pattern: "alpha.txt"
                        min: 1
                  - name: "Test Beta"
                    files:
                      - pattern: "beta.txt"
                        min: 1
                """);

            var config = FileAssertConfig.ReadFromFile(configPath);
            using var context = Context.Create(["--silent"]);

            // Act - filter that matches nothing
            config.Run(context, ["No Match"]);

            // Assert - no error because all tests were skipped by the filter
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run writes a TRX results file with Passed outcome when all tests pass.
    /// </summary>
    [TestMethod]
    public void FileAssertConfig_Run_WithResultsFile_WritesTrxWithPassedOutcome()
    {
        // Arrange
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        var trxFile = Path.Combine(Path.GetTempPath(), $"config_test_{Guid.NewGuid()}.trx");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "sample.txt"), "Copyright");

            var configPath = Path.Combine(tempDir.FullName, "config.yaml");
            File.WriteAllText(configPath, """
                tests:
                  - name: "LicenseCheck"
                    files:
                      - pattern: "sample.txt"
                        text:
                          - contains: "Copyright"
                """);

            var config = FileAssertConfig.ReadFromFile(configPath);
            using var context = Context.Create(["--silent", "--results", trxFile]);

            // Act
            config.Run(context, []);

            // Assert - TRX file contains the test name with Passed outcome
            Assert.AreEqual(0, context.ExitCode);
            Assert.IsTrue(File.Exists(trxFile));
            var trxContent = File.ReadAllText(trxFile);
            Assert.Contains("LicenseCheck", trxContent);
            Assert.Contains("outcome=\"Passed\"", trxContent);
        }
        finally
        {
            tempDir.Delete(recursive: true);
            if (File.Exists(trxFile))
            {
                File.Delete(trxFile);
            }
        }
    }

    /// <summary>
    ///     Verifies that Run writes a JUnit XML results file with failure entries when tests fail.
    /// </summary>
    [TestMethod]
    public void FileAssertConfig_Run_WithResultsFile_WritesJUnitWithFailedOutcome()
    {
        // Arrange
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        var xmlFile = Path.Combine(Path.GetTempPath(), $"config_test_{Guid.NewGuid()}.xml");
        try
        {
            // File does NOT contain the expected text - assertion will fail
            File.WriteAllText(Path.Combine(tempDir.FullName, "sample.txt"), "no license here");

            var configPath = Path.Combine(tempDir.FullName, "config.yaml");
            File.WriteAllText(configPath, """
                tests:
                  - name: "LicenseCheck"
                    files:
                      - pattern: "sample.txt"
                        text:
                          - contains: "Copyright"
                """);

            var config = FileAssertConfig.ReadFromFile(configPath);
            using var context = Context.Create(["--silent", "--results", xmlFile]);

            // Act
            config.Run(context, []);

            // Assert - JUnit file contains the test name with a failure entry
            Assert.AreNotEqual(0, context.ExitCode);
            Assert.IsTrue(File.Exists(xmlFile));
            var xmlContent = File.ReadAllText(xmlFile);
            Assert.Contains("LicenseCheck", xmlContent);
            Assert.Contains("failures=\"1\"", xmlContent);
        }
        finally
        {
            tempDir.Delete(recursive: true);
            if (File.Exists(xmlFile))
            {
                File.Delete(xmlFile);
            }
        }
    }
}
