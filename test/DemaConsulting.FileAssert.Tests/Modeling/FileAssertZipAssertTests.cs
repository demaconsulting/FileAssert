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
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace DemaConsulting.FileAssert.Tests.Modeling;

/// <summary>
///     Unit tests for the <see cref="FileAssertZipAssert"/> class.
/// </summary>
[Collection("Sequential")]
public sealed class FileAssertZipAssertTests
{
    /// <summary>
    ///     Sample XML content used across multiple XML assertion tests.
    /// </summary>
    private const string SampleXmlContent = """
        <?xml version="1.0" encoding="utf-8"?>
        <root>
          <item>one</item>
          <item>two</item>
        </root>
        """;

    /// <summary>
    ///     Sample YAML content used across multiple YAML assertion tests.
    /// </summary>
    private const string SampleYamlContent = """
        server:
          host: localhost
          port: 8080
        """;

    /// <summary>
    ///     Sample JSON content used across multiple JSON assertion tests.
    /// </summary>
    private const string SampleJsonContent = """
        {
          "server": {
            "host": "localhost",
            "port": 8080
          }
        }
        """;

    /// <summary>
    ///     Creates a zip file at <paramref name="path"/> containing the specified entry names,
    ///     each with a single placeholder byte of content.
    /// </summary>
    /// <param name="path">Destination path for the zip file. Any existing file is removed first.</param>
    /// <param name="entries">Entry names to add to the zip archive.</param>
    private static void CreateZipFile(string path, IEnumerable<string> entries)
    {
        // Remove the file first because ZipFile.Open in Create mode requires a non-existent path,
        // but Path.GetTempFileName() creates a zero-byte placeholder that must be deleted first.
        File.Delete(path);

        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (var entry in entries)
        {
            var archiveEntry = archive.CreateEntry(entry);
            using var stream = archiveEntry.Open();

            // Write a single placeholder byte so the entry is not an empty-stream edge case
            stream.WriteByte(0x00);
        }
    }

    /// <summary>
    ///     Creates a zip file at <paramref name="path"/> containing entries with the specified
    ///     names and text content. Content is written as UTF-8 without BOM so that
    ///     <see cref="System.IO.Compression.ZipArchiveEntry.Length"/> reflects the exact byte count.
    /// </summary>
    /// <param name="path">Destination path for the zip file. Any existing file is removed first.</param>
    /// <param name="entries">Entry names and text content to add to the zip archive.</param>
    private static void CreateZipFileWithContent(string path, IEnumerable<(string name, string content)> entries)
    {
        // Remove the file first because ZipFile.Open in Create mode requires a non-existent path
        File.Delete(path);

        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (var (name, content) in entries)
        {
            var archiveEntry = archive.CreateEntry(name);
            var entryStream = archiveEntry.Open();

            // StreamWriter takes ownership of the stream and closes it on disposal;
            // UTF-8 without BOM ensures the uncompressed size equals the exact character count
            using var writer = new StreamWriter(entryStream, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.Write(content);
        }
    }

    /// <summary>
    ///     Creates an outer zip file at <paramref name="outerPath"/> containing a single inner
    ///     zip entry named <paramref name="innerEntryName"/>. The inner zip is built in memory
    ///     and contains the specified text entries.
    /// </summary>
    /// <param name="outerPath">Destination path for the outer zip. Any existing file is removed first.</param>
    /// <param name="innerEntryName">Name of the inner zip entry within the outer archive.</param>
    /// <param name="innerEntries">Entry names and text content within the inner zip archive.</param>
    private static void CreateNestedZipFile(
        string outerPath,
        string innerEntryName,
        IEnumerable<(string name, string content)> innerEntries)
    {
        // Build the inner zip entirely in memory before writing it to the outer archive
        using var innerZipStream = new MemoryStream();
        using (var innerArchive = new ZipArchive(innerZipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in innerEntries)
            {
                var innerEntry = innerArchive.CreateEntry(name);
                var innerStream = innerEntry.Open();

                // StreamWriter takes ownership of the stream (leaveOpen: false default)
                using var writer = new StreamWriter(innerStream, new System.Text.UTF8Encoding(false));
                writer.Write(content);
            }
        }

        var innerZipBytes = innerZipStream.ToArray();

        // Write the outer zip with the in-memory inner zip as a single binary entry
        File.Delete(outerPath);
        using var outerArchive = ZipFile.Open(outerPath, ZipArchiveMode.Create);
        var outerEntry = outerArchive.CreateEntry(innerEntryName);
        using var outerEntryStream = outerEntry.Open();
        outerEntryStream.Write(innerZipBytes, 0, innerZipBytes.Length);
    }

    /// <summary>
    ///     Creates a zip file at <paramref name="path"/> containing a single entry named
    ///     <paramref name="entryName"/> whose content is the supplied raw bytes. Used for
    ///     binary entries such as PDF documents that cannot be written as text.
    /// </summary>
    /// <param name="path">Destination path for the zip file. Any existing file is removed first.</param>
    /// <param name="entryName">Name of the entry to add to the archive.</param>
    /// <param name="content">The raw bytes to write as the entry content.</param>
    private static void CreateZipFileWithBinaryEntry(string path, string entryName, byte[] content)
    {
        // Remove the file first because ZipFile.Open in Create mode requires a non-existent path
        File.Delete(path);

        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        var archiveEntry = archive.CreateEntry(entryName);
        using var stream = archiveEntry.Open();
        stream.Write(content, 0, content.Length);
    }

    /// <summary>
    ///     A test-only <see cref="IContext"/> implementation that captures all error messages
    ///     written via <see cref="WriteError"/> for inspection in breadcrumb tests.
    ///     <see cref="WithPrefix"/> chains a scoped wrapper that mirrors the behavior of
    ///     <c>Context.ScopedContext</c> so that the full breadcrumb path is accumulated.
    /// </summary>
    private sealed class CapturingContext : IContext
    {
        private readonly List<string> _errors = [];

        /// <summary>Gets all error messages captured since this context was created.</summary>
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();

        /// <inheritdoc/>
        public void WriteLine(string message) { }

        /// <inheritdoc/>
        public void WriteError(string message) => _errors.Add(message);

        /// <inheritdoc/>
        public IContext WithPrefix(string prefix) => new PrefixedContext(this, prefix);

        /// <summary>
        ///     Scoped wrapper that prepends a prefix to each error before delegating to the
        ///     parent context. Mirrors the behavior of <c>Context.ScopedContext</c>.
        /// </summary>
        private sealed class PrefixedContext : IContext
        {
            private readonly IContext _parent;
            private readonly string _prefix;

            internal PrefixedContext(IContext parent, string prefix)
            {
                _parent = parent;
                _prefix = prefix;
            }

            public void WriteLine(string message) => _parent.WriteLine(message);

            public void WriteError(string message) => _parent.WriteError($"{_prefix} > {message}");

            public IContext WithPrefix(string prefix) => new PrefixedContext(this, prefix);
        }
    }

    /// <summary>
    ///     Verifies that Create succeeds given valid data.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Create_ValidData_CreatesZipAssert()
    {
        // Arrange
        var data = new FileAssertZipData
        {
            Files =
            [
                new FileAssertFileData { Pattern = "lib/**/*.dll", Min = 1 }
            ]
        };

        // Act
        var zipAssert = FileAssertZipAssert.Create(data);

        // Assert
        Assert.NotNull(zipAssert);
        Assert.Single(zipAssert.Files);
        Assert.Equal("lib/**/*.dll", zipAssert.Files[0].Pattern);
        Assert.Equal(1, zipAssert.Files[0].Min);
        Assert.Null(zipAssert.Files[0].Max);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Create_NullData_ThrowsArgumentNullException()
    {
        // Act / Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertZipAssert.Create(null!));
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when an entry has
    ///     no pattern.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Create_EntryMissingPattern_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new FileAssertZipData
        {
            Files = [new FileAssertFileData { Min = 1 }]
        };

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => FileAssertZipAssert.Create(data));
    }

    /// <summary>
    ///     Verifies that Run produces no error when the zip archive contains entries that match
    ///     the pattern and satisfy the count constraints.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_MatchingEntriesMeetConstraints_NoError()
    {
        // Arrange - create a zip archive containing a matching entry
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFile(tempFile, ["lib/net8.0/MyLib.dll"]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData { Pattern = "lib/net8.0/MyLib.dll", Min = 1, Max = 1 }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
            Assert.Equal(0, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when a glob pattern matches multiple entries within
    ///     the zip archive and the count is within the declared bounds.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_GlobPatternMatchesMultipleEntries_NoError()
    {
        // Arrange - create a zip archive containing multiple dll entries under lib/
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFile(tempFile, ["lib/net8.0/MyLib.dll", "lib/net8.0/MyOther.dll"]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData { Pattern = "lib/**/*.dll", Min = 1 }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
            Assert.Equal(0, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when the number of matching entries is below
    ///     the declared minimum count.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_TooFewMatchingEntries_WritesError()
    {
        // Arrange - create an empty zip archive; the min constraint will be violated
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFile(tempFile, []);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData { Pattern = "lib/**/*.dll", Min = 1 }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when the number of matching entries exceeds
    ///     the declared maximum count.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_TooManyMatchingEntries_WritesError()
    {
        // Arrange - create a zip archive with two dll entries; max is set to 1
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFile(tempFile, ["lib/net8.0/MyLib.dll", "lib/net8.0/MyOther.dll"]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData { Pattern = "lib/**/*.dll", Max = 1 }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when the file is not a valid zip archive.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_InvalidZipFile_WritesError()
    {
        // Arrange - write arbitrary bytes that are not a valid zip archive
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, [0x00, 0x01, 0x02, 0x03]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData { Pattern = "**/*.dll", Min = 1 }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert - a single parse error should be reported
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when the zip file path does not exist.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_NonExistentFile_WritesError()
    {
        // Arrange - use a filename guaranteed not to exist in the temp directory
        var missingFileName = $"does_not_exist_{Guid.NewGuid():N}.zip";
        var data = new FileAssertZipData
        {
            Files =
            [
                new FileAssertFileData { Pattern = "**/*.dll", Min = 1 }
            ]
        };
        var zipAssert = FileAssertZipAssert.Create(data);
        using var context = Context.Create(["--silent"]);
        using var dirContainer = new DirectoryFileContainer(Path.GetTempPath());

        // Act
        zipAssert.Run(context, dirContainer, missingFileName);

        // Assert - a single I/O error should be reported
        Assert.Equal(1, context.ExitCode);
        Assert.Equal(1, context.ErrorCount);
    }

    // -----------------------------------------------------------------------
    // Content assertion tests — text, XML, YAML, JSON, size, nested zip, breadcrumbs
    // -----------------------------------------------------------------------

    /// <summary>
    ///     Verifies that Run produces no error when a zip entry contains the required text.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryContainsRequiredText_NoError()
    {
        // Arrange - create a zip archive whose text entry satisfies the contains rule
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("readme.txt", "hello world")]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "readme.txt",
                        Text = [new FileAssertRuleData { Contains = "hello" }]
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
            Assert.Equal(0, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when a zip entry does not contain the required text.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryMissingRequiredText_WritesError()
    {
        // Arrange - create a zip archive whose text entry does NOT satisfy the contains rule
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("readme.txt", "goodbye world")]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "readme.txt",
                        Text = [new FileAssertRuleData { Contains = "not-present" }]
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when a zip entry contains XML that satisfies
    ///     the XPath count constraint.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryXmlMatchesXPath_NoError()
    {
        // Arrange - create a zip with an XML entry containing exactly 2 item elements
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("config.xml", SampleXmlContent)]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "config.xml",
                        Xml = [new FileAssertQueryData { Query = "//item", Count = 2 }]
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
            Assert.Equal(0, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when a zip entry contains XML that does not satisfy
    ///     the XPath count constraint.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryXmlFailsXPath_WritesError()
    {
        // Arrange - XML has 2 items but we assert count = 5
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("config.xml", SampleXmlContent)]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "config.xml",
                        Xml = [new FileAssertQueryData { Query = "//item", Count = 5 }]
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when a zip entry contains HTML that satisfies
    ///     the XPath count constraint of the <c>html:</c> asserter.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryHtmlMatchesXPath_NoError()
    {
        // Arrange - create a zip with an HTML entry containing exactly one <title> element
        const string sampleHtml = """
            <!DOCTYPE html>
            <html>
            <head><title>Report</title></head>
            <body><p>one</p><p>two</p></body>
            </html>
            """;
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("report.html", sampleHtml)]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "report.html",
                        Html = [new FileAssertQueryData { Query = "//p", Count = 2 }]
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
            Assert.Equal(0, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when a zip entry contains a PDF that satisfies
    ///     the page-count constraint of the <c>pdf:</c> asserter.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryPdfMatchesConstraint_NoError()
    {
        // Arrange - build a single-page PDF with body text and store it as a zip entry
        byte[] pdfBytes;
        using (var builder = new PdfDocumentBuilder())
        {
            var page = builder.AddPage(PageSize.A4);
            var font = builder.AddStandard14Font(Standard14Font.Helvetica);
            page.AddText("Hello World", 12, new PdfPoint(50, 700), font);
            pdfBytes = builder.Build();
        }

        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithBinaryEntry(tempFile, "report.pdf", pdfBytes);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "report.pdf",
                        Pdf = new FileAssertPdfData
                        {
                            Pages = new FileAssertPdfPagesData { Min = 1, Max = 1 },
                            Text = [new FileAssertRuleData { Contains = "Hello" }]
                        }
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
            Assert.Equal(0, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when a zip entry contains YAML that satisfies
    ///     the dot-notation query count constraint.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryYamlMatchesQuery_NoError()
    {
        // Arrange - create a zip with a YAML entry whose server.host key matches count = 1
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("config.yaml", SampleYamlContent)]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "config.yaml",
                        Yaml = [new FileAssertQueryData { Query = "server.host", Count = 1 }]
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
            Assert.Equal(0, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when a zip entry contains YAML that does not satisfy
    ///     the dot-notation query count constraint.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryYamlFailsQuery_WritesError()
    {
        // Arrange - YAML has one server.host value but we assert count = 5
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("config.yaml", SampleYamlContent)]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "config.yaml",
                        Yaml = [new FileAssertQueryData { Query = "server.host", Count = 5 }]
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when a zip entry contains JSON that satisfies
    ///     the dot-notation query count constraint.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryJsonMatchesQuery_NoError()
    {
        // Arrange - create a zip with a JSON entry whose "server" key matches count = 1
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("config.json", SampleJsonContent)]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "config.json",
                        Json = [new FileAssertQueryData { Query = "server", Count = 1 }]
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
            Assert.Equal(0, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when a zip entry contains JSON that does not satisfy
    ///     the dot-notation query count constraint.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryJsonFailsQuery_WritesError()
    {
        // Arrange - JSON has no "missing" key but we assert count = 1
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("config.json", SampleJsonContent)]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "config.json",
                        Json = [new FileAssertQueryData { Query = "missing", Count = 1 }]
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when a zip entry's uncompressed size meets
    ///     the minimum-size constraint.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryMeetsMinSizeConstraint_NoError()
    {
        // Arrange - entry content is "hello world" (11 bytes UTF-8 without BOM)
        // and min-size is 5, which is satisfied by 11 bytes
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("data.bin", "hello world")]);
            var data = new FileAssertZipData
            {
                Files = [new FileAssertFileData { Pattern = "data.bin", MinSize = 5 }]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
            Assert.Equal(0, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when a zip entry's uncompressed size is below
    ///     the minimum-size constraint.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryBelowMinSizeConstraint_WritesError()
    {
        // Arrange - entry content is "hello world" (11 bytes UTF-8 without BOM)
        // and min-size is 20, which is NOT satisfied by 11 bytes
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("data.bin", "hello world")]);
            var data = new FileAssertZipData
            {
                Files = [new FileAssertFileData { Pattern = "data.bin", MinSize = 20 }]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when a zip entry's uncompressed size is within
    ///     the maximum-size constraint.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryMeetsMaxSizeConstraint_NoError()
    {
        // Arrange - entry content is "hello world" (11 bytes UTF-8 without BOM)
        // and max-size is 20, which is satisfied by 11 bytes
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("data.bin", "hello world")]);
            var data = new FileAssertZipData
            {
                Files = [new FileAssertFileData { Pattern = "data.bin", MaxSize = 20 }]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
            Assert.Equal(0, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when a zip entry's uncompressed size exceeds
    ///     the maximum-size constraint.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_EntryExceedsMaxSizeConstraint_WritesError()
    {
        // Arrange - entry content is "hello world" (11 bytes UTF-8 without BOM)
        // and max-size is 5, which is NOT satisfied by 11 bytes
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("data.bin", "hello world")]);
            var data = new FileAssertZipData
            {
                Files = [new FileAssertFileData { Pattern = "data.bin", MaxSize = 5 }]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run recursively evaluates a text-content assertion on an entry inside
    ///     a zip that is itself an entry in another zip (nested zip-in-zip).
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_NestedZipTextContent_InnerEntryContentMatches_NoError()
    {
        // Arrange - outer zip contains inner.zip which contains readme.txt with "hello world"
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateNestedZipFile(tempFile, "inner.zip", [("readme.txt", "hello world")]);

            // The inner zip assert checks the text entry and the outer zip assert locates inner.zip
            var innerZipData = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "readme.txt",
                        Min = 1,
                        Text = [new FileAssertRuleData { Contains = "hello" }]
                    }
                ]
            };
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "inner.zip",
                        Min = 1,
                        Zip = innerZipData
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
            Assert.Equal(0, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that when a content assertion fails inside a zip entry the error message
    ///     carries breadcrumbs that identify both the zip file and the failing entry path.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_ContentAssertionFails_ErrorContainsBreadcrumbs()
    {
        // Arrange - create a zip with a text entry whose content will NOT satisfy the rule
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFileWithContent(tempFile, [("readme.txt", "goodbye world")]);
            var data = new FileAssertZipData
            {
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "readme.txt",
                        Text = [new FileAssertRuleData { Contains = "not-present" }]
                    }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            var capturingContext = new CapturingContext();

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            zipAssert.Run(capturingContext, container, fileName);

            // Assert - the error contains the full breadcrumb path and the zip name is not doubled
            Assert.NotEmpty(capturingContext.Errors);
            var error = capturingContext.Errors[0];
            Assert.Contains($"{fileName} > readme.txt", error);
            Assert.Equal(1, error.Split(fileName).Length - 1);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
