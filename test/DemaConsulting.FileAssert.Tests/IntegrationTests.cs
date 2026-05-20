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

using System.IO.Compression;
using System.Text.RegularExpressions;

using DemaConsulting.FileAssert.Utilities;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Writer;

namespace DemaConsulting.FileAssert.Tests;

/// <summary>
///     Integration tests that run the FileAssert application through dotnet.
/// </summary>
[Collection("Sequential")]
public partial class IntegrationTests
{
    [GeneratedRegex(@"\d+\.\d+\.\d+")]
    private static partial Regex SemanticVersionRegex();

    private readonly string _dllPath;

    /// <summary>
    ///     Initialize test by locating the FileAssert DLL.
    /// </summary>
    public IntegrationTests()
    {
        var baseDir = AppContext.BaseDirectory;
        _dllPath = PathHelpers.SafePathCombine(baseDir, "DemaConsulting.FileAssert.dll");
        Assert.True(File.Exists(_dllPath), $"Could not find FileAssert DLL at {_dllPath}");
    }

    /// <summary>
    ///     Test that version flag outputs version information.
    /// </summary>
    [Fact]
    public void IntegrationTest_VersionFlag_OutputsVersion()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--version");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.True(SemanticVersionRegex().IsMatch(output), $"Output did not contain a semantic version: {output}");
    }

    /// <summary>
    ///     Test that help flag outputs usage information.
    /// </summary>
    [Fact]
    public void IntegrationTest_HelpFlag_OutputsUsageInformation()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--help");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("Usage:", output);
        Assert.Contains("Options:", output);
        Assert.Contains("--version", output);
    }

    /// <summary>
    ///     Test that validate flag runs self-validation.
    /// </summary>
    [Fact]
    public void IntegrationTest_ValidateFlag_RunsValidation()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--validate");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("Total Tests:", output);
        Assert.Contains("Passed:", output);
    }

    /// <summary>
    ///     Test that validate with results flag generates TRX file.
    /// </summary>
    [Fact]
    public void IntegrationTest_ValidateWithResults_GeneratesTrxFile()
    {
        // Arrange
        var resultsFile = Path.Combine(Path.GetTempPath(), $"integration_test_{Guid.NewGuid()}.trx");

        try
        {
            // Act
            var exitCode = Runner.Run(
                out var _,
                "dotnet",
                _dllPath,
                "--validate",
                "--results",
                resultsFile);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(resultsFile), "Results file was not created");

            var trxContent = File.ReadAllText(resultsFile);
            Assert.Contains("<TestRun", trxContent);
            Assert.Contains("</TestRun>", trxContent);
        }
        finally
        {
            if (File.Exists(resultsFile))
            {
                File.Delete(resultsFile);
            }
        }
    }

    /// <summary>
    ///     Test that silent flag suppresses output.
    /// </summary>
    [Fact]
    public void IntegrationTest_SilentFlag_SuppressesOutput()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--silent");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, output);
    }

    /// <summary>
    ///     Test that log flag writes output to file.
    /// </summary>
    [Fact]
    public void IntegrationTest_LogFlag_WritesOutputToFile()
    {
        // Arrange
        var logFile = Path.GetTempFileName();

        try
        {
            // Act
            var exitCode = Runner.Run(
                out var _,
                "dotnet",
                _dllPath,
                "--log",
                logFile);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(logFile), "Log file was not created");

            var logContent = File.ReadAllText(logFile);
            Assert.Contains("FileAssert version", logContent);
        }
        finally
        {
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
        }
    }

    /// <summary>
    ///     Test that validate with results flag generates JUnit XML file.
    /// </summary>
    [Fact]
    public void IntegrationTest_ValidateWithResults_GeneratesJUnitFile()
    {
        // Arrange
        var resultsFile = Path.Combine(Path.GetTempPath(), $"integration_test_{Guid.NewGuid()}.xml");

        try
        {
            // Act
            var exitCode = Runner.Run(
                out var _,
                "dotnet",
                _dllPath,
                "--validate",
                "--results",
                resultsFile);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(resultsFile), "Results file was not created");

            var xmlContent = File.ReadAllText(resultsFile);
            Assert.Contains("<testsuites", xmlContent);
        }
        finally
        {
            if (File.Exists(resultsFile))
            {
                File.Delete(resultsFile);
            }
        }
    }

    /// <summary>
    ///     Test that unknown argument returns error.
    /// </summary>
    [Fact]
    public void IntegrationTest_UnknownArgument_ReturnsError()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--unknown");

        // Assert
        Assert.NotEqual(0, exitCode);
        Assert.Contains("Error", output);
    }

    /// <summary>
    ///     Test that positional name/tag filter arguments cause only matching tests to run.
    /// </summary>
    [Fact]
    public void IntegrationTest_TestFiltering_OnlyRunsMatchingTests()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create a file that the smoke test will assert against
        File.WriteAllText(tempDir.GetFilePath("smoke.txt"), "smoke content");

        // Write a config with a passing "smoke" test and a failing "regression" test
        // (regression test references a file that does not exist with min: 1)
        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "SmokeTest"
                tags: [smoke]
                files:
                  - pattern: "smoke.txt"
                    min: 1
              - name: "RegressionTest"
                tags: [regression]
                files:
                  - pattern: "missing.txt"
                    min: 1
            """);

        // Act - filter to only the "smoke" tag; the regression test should not run
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--config",
            configPath,
            "smoke");

        // Assert - exit code 0 because the failing regression test was skipped by the filter
        Assert.Equal(0, exitCode);

    }

    /// <summary>
    ///     Test that a valid configuration file causes the tool to run assertions and succeed.
    /// </summary>
    [Fact]
    public void IntegrationTest_ValidConfig_PassingAssertions_ReturnsZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create a file that satisfies the assertion
        File.WriteAllText(tempDir.GetFilePath("sample.txt"), "Copyright (c) DEMA Consulting");

        // Write a config that asserts the file exists and contains the expected text
        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "License Check"
                files:
                  - pattern: "*.txt"
                    min: 1
                    text:
                      - contains: "Copyright"
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--config",
            configPath);

        // Assert
        Assert.Equal(0, exitCode);

    }

    /// <summary>
    ///     Test that a configuration file with a failing assertion causes the tool to return non-zero.
    /// </summary>
    [Fact]
    public void IntegrationTest_ValidConfig_FailingAssertions_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create a file that does NOT satisfy the assertion
        File.WriteAllText(tempDir.GetFilePath("sample.txt"), "no license here");

        // Write a config that asserts the file contains text it does not contain
        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "License Check"
                files:
                  - pattern: "*.txt"
                    text:
                      - contains: "Copyright"
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--silent",
            "--config",
            configPath);

        // Assert - non-zero exit code indicates assertion failure
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that passing file assertions write a TRX results file with Passed outcomes.
    /// </summary>
    [Fact]
    public void IntegrationTest_PassingAssertions_WritesTrxWithPassedResults()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        var resultsFile = tempDir.GetFilePath("results.trx");
        // Create a file that satisfies the assertion
        File.WriteAllText(tempDir.GetFilePath("sample.txt"), "Copyright (c) DEMA Consulting");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "LicenseCheck"
                files:
                  - pattern: "*.txt"
                    min: 1
                    text:
                      - contains: "Copyright"
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--config",
            configPath,
            "--results",
            resultsFile);

        // Assert - exit code 0 and TRX file contains LicenseCheck with Passed outcome
        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(resultsFile), "Results file was not created");
        var trxContent = File.ReadAllText(resultsFile);
        Assert.Contains("LicenseCheck", trxContent);
        Assert.Contains("outcome=\"Passed\"", trxContent);

    }

    /// <summary>
    ///     Test that failing file assertions write a JUnit results file with failure entries.
    /// </summary>
    [Fact]
    public void IntegrationTest_FailingAssertions_WritesJUnitWithFailedResults()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        var resultsFile = tempDir.GetFilePath("results.xml");
        // Create a file that does NOT satisfy the assertion
        File.WriteAllText(tempDir.GetFilePath("sample.txt"), "no license here");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "LicenseCheck"
                files:
                  - pattern: "*.txt"
                    text:
                      - contains: "Copyright"
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--silent",
            "--config",
            configPath,
            "--results",
            resultsFile);

        // Assert - non-zero exit code and JUnit file contains LicenseCheck with a failure entry
        Assert.NotEqual(0, exitCode);
        Assert.True(File.Exists(resultsFile), "Results file was not created");
        var xmlContent = File.ReadAllText(resultsFile);
        Assert.Contains("LicenseCheck", xmlContent);
        Assert.Contains("failures=\"1\"", xmlContent);

    }

    /// <summary>
    ///     Test that a minimum file count constraint returns a non-zero exit code when too few files are found.
    /// </summary>
    [Fact]
    public void IntegrationTest_MinCountConstraint_TooFewFiles_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create no files when the test requires at least one
        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "RequiredFileCheck"
                files:
                  - pattern: "*.txt"
                    min: 1
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--silent",
            "--config",
            configPath);

        // Assert - non-zero exit code because the min count constraint was not met
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that a maximum file count constraint returns a non-zero exit code when exceeded.
    /// </summary>
    [Fact]
    public void IntegrationTest_MaxCountConstraint_TooManyFiles_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create two files when the test asserts at most one
        File.WriteAllText(tempDir.GetFilePath("a.txt"), "content a");
        File.WriteAllText(tempDir.GetFilePath("b.txt"), "content b");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "UniqueFileCheck"
                files:
                  - pattern: "*.txt"
                    max: 1
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--silent",
            "--config",
            configPath);

        // Assert - non-zero exit code because the max count constraint was exceeded
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that a regex rule returns a zero exit code when file content matches the pattern.
    /// </summary>
    [Fact]
    public void IntegrationTest_RegexRule_MatchingContent_ReturnsZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create a file whose content matches the regex pattern
        File.WriteAllText(tempDir.GetFilePath("version.txt"), "Version: 1.2.3");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "VersionFormatCheck"
                files:
                  - pattern: "version.txt"
                    text:
                      - matches: "\\d+\\.\\d+\\.\\d+"
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--config",
            configPath);

        // Assert
        Assert.Equal(0, exitCode);

    }

    /// <summary>
    ///     Test that a regex rule returns a non-zero exit code when file content does not match the pattern.
    /// </summary>
    [Fact]
    public void IntegrationTest_RegexRule_NonMatchingContent_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create a file whose content does NOT match the version regex
        File.WriteAllText(tempDir.GetFilePath("version.txt"), "no version here");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "VersionFormatCheck"
                files:
                  - pattern: "version.txt"
                    text:
                      - matches: "\\d+\\.\\d+\\.\\d+"
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--silent",
            "--config",
            configPath);

        // Assert - non-zero because the file does not match the version pattern
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that an exact count constraint returns a non-zero exit code when file count is wrong.
    /// </summary>
    [Fact]
    public void IntegrationTest_ExactCountConstraint_WrongCount_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create two files when the test asserts exactly one
        File.WriteAllText(tempDir.GetFilePath("a.txt"), "content a");
        File.WriteAllText(tempDir.GetFilePath("b.txt"), "content b");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "ExactCountCheck"
                files:
                  - pattern: "*.txt"
                    count: 1
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--silent",
            "--config",
            configPath);

        // Assert - non-zero exit code because the exact count constraint was not met
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that a min-size constraint returns a non-zero exit code when file is too small.
    /// </summary>
    [Fact]
    public void IntegrationTest_FileSizeConstraints_TooSmall_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create an empty file when the test requires at least 10 bytes
        File.WriteAllText(tempDir.GetFilePath("empty.txt"), string.Empty);

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "MinSizeCheck"
                files:
                  - pattern: "*.txt"
                    min-size: 10
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--silent",
            "--config",
            configPath);

        // Assert - non-zero exit code because the file is smaller than the minimum size
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that a max-size constraint returns a non-zero exit code when file is too large.
    /// </summary>
    [Fact]
    public void IntegrationTest_FileSizeConstraints_TooLarge_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create a file with content larger than 5 bytes
        File.WriteAllText(tempDir.GetFilePath("large.txt"), "this content is more than five bytes");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "MaxSizeCheck"
                files:
                  - pattern: "*.txt"
                    max-size: 5
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--silent",
            "--config",
            configPath);

        // Assert - non-zero exit code because the file exceeds the maximum size
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that a does-not-contain rule returns a non-zero exit code when forbidden text is present.
    /// </summary>
    [Fact]
    public void IntegrationTest_DoesNotContainRule_ForbiddenTextPresent_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create a file that contains the forbidden text
        File.WriteAllText(tempDir.GetFilePath("config.txt"), "password123=secret");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "NoSecretsCheck"
                files:
                  - pattern: "*.txt"
                    text:
                      - does-not-contain: "password123"
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--silent",
            "--config",
            configPath);

        // Assert - non-zero exit code because the forbidden text is present
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that a does-not-contain-regex rule returns a non-zero exit code when the forbidden pattern matches.
    /// </summary>
    [Fact]
    public void IntegrationTest_DoesNotContainRegexRule_ForbiddenPatternMatches_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Create a file that matches the forbidden pattern
        File.WriteAllText(tempDir.GetFilePath("app.log"), "FATAL: unexpected error occurred");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "NoFatalErrorsCheck"
                files:
                  - pattern: "*.log"
                    text:
                      - does-not-contain-regex: "FATAL|ERROR"
            """);

        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--silent",
            "--config",
            configPath);

        // Assert - non-zero exit code because the forbidden pattern matched
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that an XML assert with a passing query returns a zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_XmlAssert_PassingQuery_ReturnsZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("config.xml"), """
            <configuration>
              <setting key="env">production</setting>
            </configuration>
            """);

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "XmlCheck"
                files:
                  - pattern: "*.xml"
                    xml:
                      - query: "//configuration/setting"
                        min: 1
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.Equal(0, exitCode);

    }

    /// <summary>
    ///     Test that an XML assert with an invalid XML file returns a non-zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_XmlAssert_InvalidFile_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("config.xml"), "this is not xml <<>>");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "XmlCheck"
                files:
                  - pattern: "*.xml"
                    xml:
                      - query: "//configuration"
                        min: 1
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that an HTML assert with a passing query returns a zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_HtmlAssert_PassingQuery_ReturnsZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("index.html"), """
            <html>
              <head><title>Test Page</title></head>
              <body><p>Hello</p></body>
            </html>
            """);

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "HtmlCheck"
                files:
                  - pattern: "*.html"
                    html:
                      - query: "//head/title"
                        count: 1
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.Equal(0, exitCode);

    }

    /// <summary>
    ///     Test that a YAML assert with a passing dot-notation query returns a zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_YamlAssert_PassingQuery_ReturnsZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("appsettings.yaml"), """
            server:
              host: localhost
              port: 8080
            """);

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "YamlCheck"
                files:
                  - pattern: "appsettings.yaml"
                    yaml:
                      - query: "server.host"
                        count: 1
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.Equal(0, exitCode);

    }

    /// <summary>
    ///     Test that a JSON assert with a passing dot-notation query returns a zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_JsonAssert_PassingQuery_ReturnsZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("appsettings.json"), """
            {
              "ConnectionStrings": {
                "DefaultConnection": "Server=localhost"
              }
            }
            """);

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "JsonCheck"
                files:
                  - pattern: "appsettings.json"
                    json:
                      - query: "ConnectionStrings"
                        count: 1
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.Equal(0, exitCode);

    }

    /// <summary>
    ///     Test that a ZIP assert with a passing entry query returns a zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_ZipAssert_PassingQuery_ReturnsZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        var zipPath = tempDir.GetFilePath("archive.zip");
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            zip.CreateEntry("readme.txt");
        }

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: ZipCheck
                files:
                  - pattern: "*.zip"
                    zip:
                      entries:
                        - pattern: "*.txt"
                          min: 1
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.Equal(0, exitCode);

    }

    /// <summary>
    ///     Test that a ZIP assert with an invalid zip file returns a non-zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_ZipAssert_InvalidFile_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        var invalidZipPath = tempDir.GetFilePath("invalid.zip");
        File.WriteAllText(invalidZipPath, "this is not a zip file");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: ZipInvalidCheck
                files:
                  - pattern: "*.zip"
                    zip:
                      entries:
                        - pattern: "*.txt"
                          min: 1
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that an HTML assert with a non-existent XPath query and min:1 returns a non-zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_HtmlAssert_InvalidFile_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("index.html"), """
            <html>
              <head><title>Test Page</title></head>
              <body><p>Hello</p></body>
            </html>
            """);

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "HtmlInvalidCheck"
                files:
                  - pattern: "*.html"
                    html:
                      - query: "//nonexistentElement"
                        min: 1
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that a YAML assert with an invalid YAML file returns a non-zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_YamlAssert_InvalidFile_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("invalid.yaml"), ": invalid yaml\n  - bad");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "YamlInvalidCheck"
                files:
                  - pattern: "invalid.yaml"
                    yaml:
                      - query: "server.host"
                        count: 1
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that a JSON assert with an invalid JSON file returns a non-zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_JsonAssert_InvalidFile_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("invalid.json"), "{invalid json");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "JsonInvalidCheck"
                files:
                  - pattern: "invalid.json"
                    json:
                      - query: "ConnectionStrings"
                        count: 1
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that a PDF assert with a failing page-count assertion returns a non-zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_PdfAssert_FailingAssertion_ReturnsNonZero()
    {
        // Arrange - build a valid single-page PDF and assert a minimum of 2 pages (will fail)
        using var tempDir = new TemporaryDirectory();
        var pdfPath = tempDir.GetFilePath("report.pdf");
        using var builder = new PdfDocumentBuilder();
        builder.AddPage(PageSize.A4);
        File.WriteAllBytes(pdfPath, builder.Build());

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "PdfCheck"
                files:
                  - pattern: "*.pdf"
                    pdf:
                      pages:
                        min: 2
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.NotEqual(0, exitCode);

    }

    /// <summary>
    ///     Test that a PDF assert with an invalid file returns a non-zero exit code.
    /// </summary>
    [Fact]
    public void IntegrationTest_PdfAssert_InvalidFile_ReturnsNonZero()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("report.pdf"), "not a real pdf");

        var configPath = tempDir.GetFilePath(".fileassert.yaml");
        File.WriteAllText(configPath, """
            tests:
              - name: "PdfCheck"
                files:
                  - pattern: "*.pdf"
                    pdf:
                      pages:
                        min: 1
            """);

        // Act
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--silent", "--config", configPath);

        // Assert
        Assert.NotEqual(0, exitCode);

    }
}
