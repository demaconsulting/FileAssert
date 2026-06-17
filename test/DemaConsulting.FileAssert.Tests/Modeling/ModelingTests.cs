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

using DemaConsulting.FileAssert.Cli;
using DemaConsulting.FileAssert.Configuration;
using DemaConsulting.FileAssert.Modeling;

using DemaConsulting.FileAssert.Utilities;

namespace DemaConsulting.FileAssert.Tests.Modeling;

/// <summary>
///     Subsystem tests for the Modeling subsystem.
/// </summary>
[Collection("Sequential")]
public class ModelingTests
{
    /// <summary>
    ///     Verifies that the Modeling subsystem executes the full test → file → rule
    ///     chain without errors when all constraints are satisfied.
    /// </summary>
    [Fact]
    public void Modeling_ExecuteChain_PassesWhenAllConstraintsMet()
    {
        // Arrange - create a real file with content that satisfies all rules
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("sample.txt"), "Copyright (c) DEMA Consulting");

        var testData = new FileAssertTestData
        {
            Name = "License Check",
            Files =
            [
                new FileAssertFileData
                {
                    Pattern = "*.txt",
                    Min = 1,
                    Text =
                    [
                        new FileAssertRuleData { Contains = "Copyright" }
                    ]
                }
            ]
        };

        var test = FileAssertTest.Create(testData);
        using var context = Context.Create(["--silent"]);

        // Act
        test.Run(context, tempDir.DirectoryPath);

        // Assert - no errors reported
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    ///     Verifies that the Modeling subsystem reports failures through the context
    ///     when a content rule is not satisfied.
    /// </summary>
    [Fact]
    public void Modeling_ExecuteChain_ReportsFailuresThroughContext()
    {
        // Arrange - create a file that does NOT contain the required text
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("sample.txt"), "no license header here");

        var testData = new FileAssertTestData
        {
            Name = "License Check",
            Files =
            [
                new FileAssertFileData
                {
                    Pattern = "*.txt",
                    Text =
                    [
                        new FileAssertRuleData { Contains = "Copyright" }
                    ]
                }
            ]
        };

        var test = FileAssertTest.Create(testData);
        using var context = Context.Create(["--silent"]);

        // Act
        test.Run(context, tempDir.DirectoryPath);

        // Assert - an error was reported and the exit code is non-zero
        Assert.Equal(1, context.ExitCode);

    }

    /// <summary>
    ///     Verifies that the Modeling subsystem reports a parse error when a file-type
    ///     assertion block is declared but the file cannot be parsed as the declared format.
    /// </summary>
    [Fact]
    public void Modeling_FileTypeParsing_InvalidXml_ReportsParseError()
    {
        // Arrange - create a file with invalid XML content
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("config.xml"), "this is not valid xml <<>>");

        var testData = new FileAssertTestData
        {
            Name = "XmlCheck",
            Files =
            [
                new FileAssertFileData
                {
                    Pattern = "*.xml",
                    Xml =
                    [
                        new FileAssertQueryData { Query = "//root", Count = 1 }
                    ]
                }
            ]
        };

        var test = FileAssertTest.Create(testData);
        using var context = Context.Create(["--silent"]);

        // Act
        test.Run(context, tempDir.DirectoryPath);

        // Assert - an error was reported because the file could not be parsed as XML
        Assert.Equal(1, context.ExitCode);

    }

    /// <summary>
    ///     Verifies that the Modeling subsystem evaluates XPath query assertions against
    ///     a valid XML document and reports no error when the count constraint is satisfied.
    /// </summary>
    [Fact]
    public void Modeling_QueryAssertions_XmlQueryMeetsCount_NoError()
    {
        // Arrange - create a valid XML file with elements the query will match
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("config.xml"), """
            <configuration>
              <setting key="env">production</setting>
              <setting key="debug">false</setting>
            </configuration>
            """);

        var testData = new FileAssertTestData
        {
            Name = "XmlQueryCheck",
            Files =
            [
                new FileAssertFileData
                {
                    Pattern = "*.xml",
                    Xml =
                    [
                        new FileAssertQueryData { Query = "//configuration/setting", Min = 1 }
                    ]
                }
            ]
        };

        var test = FileAssertTest.Create(testData);
        using var context = Context.Create(["--silent"]);

        // Act
        test.Run(context, tempDir.DirectoryPath);

        // Assert - no errors reported because the query matched the expected count
        Assert.Equal(0, context.ExitCode);

    }

    /// <summary>
    ///     Verifies that the Modeling subsystem applies a text content assertion to a zip archive
    ///     entry and reports no error when the constraint is satisfied.
    /// </summary>
    [Fact]
    public void Modeling_ZipEntryContentAssertions_TextContentPassesWhenConstraintsMet()
    {
        // Arrange - create a zip file containing a text entry with known content
        using var tempDir = new TemporaryDirectory();
        var zipPath = tempDir.GetFilePath("archive.zip");
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            var entry = zip.CreateEntry("entry.txt");
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
            writer.Write("hello world");
        }

        var testData = new FileAssertTestData
        {
            Name = "ZipTextContentCheck",
            Files =
            [
                new FileAssertFileData
                {
                    Pattern = "*.zip",
                    Zip = new FileAssertZipData
                    {
                        Files =
                        [
                            new FileAssertFileData
                            {
                                Pattern = "entry.txt",
                                Text =
                                [
                                    new FileAssertRuleData { Contains = "hello world" }
                                ]
                            }
                        ]
                    }
                }
            ]
        };

        var test = FileAssertTest.Create(testData);
        using var context = Context.Create(["--silent"]);

        // Act
        test.Run(context, tempDir.DirectoryPath);

        // Assert - no errors reported; the text constraint was satisfied by the zip entry content
        Assert.Equal(0, context.ExitCode);

    }

    /// <summary>
    ///     Verifies that the Modeling subsystem reports a failure with breadcrumb-style error
    ///     messages when a text assertion on a zip entry does not pass.
    /// </summary>
    [Fact]
    public void Modeling_ZipEntryContentAssertions_FailureReportsWithBreadcrumbs()
    {
        // Arrange - create a zip file containing a text entry; the assertion will fail
        using var tempDir = new TemporaryDirectory();
        var zipPath = tempDir.GetFilePath("archive.zip");
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            var entry = zip.CreateEntry("entry.txt");
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
            writer.Write("hello world");
        }

        var testData = new FileAssertTestData
        {
            Name = "ZipBreadcrumbCheck",
            Files =
            [
                new FileAssertFileData
                {
                    Pattern = "*.zip",
                    Zip = new FileAssertZipData
                    {
                        Files =
                        [
                            new FileAssertFileData
                            {
                                Pattern = "entry.txt",
                                Text =
                                [
                                    new FileAssertRuleData { Contains = "not-present" }
                                ]
                            }
                        ]
                    }
                }
            ]
        };

        var test = FileAssertTest.Create(testData);
        var logPath = tempDir.GetFilePath("errors.log");

        // Act - run inside an explicit scope so the context (and its log writer) is disposed
        // before the log file is read; the exit code is captured before disposal
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logPath]))
        {
            test.Run(context, tempDir.DirectoryPath);
            exitCode = context.ExitCode;
        }

        // Assert - an error was reported, exit code is non-zero, and the log message carries
        // both the zip filename and the entry name as breadcrumbs
        Assert.NotEqual(0, exitCode);
        var logContents = File.ReadAllText(logPath);
        Assert.Contains("archive.zip", logContents);
        Assert.Contains("entry.txt", logContents);

    }
}
