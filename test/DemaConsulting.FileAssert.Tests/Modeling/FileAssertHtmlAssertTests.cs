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
using DemaConsulting.FileAssert.Utilities;

namespace DemaConsulting.FileAssert.Tests.Modeling;

/// <summary>
///     Unit tests for the <see cref="FileAssertHtmlAssert"/> class.
/// </summary>
[Collection("Sequential")]
public sealed class FileAssertHtmlAssertTests
{
    private const string SampleHtml = """
        <!DOCTYPE html>
        <html>
        <head><title>Test Page</title></head>
        <body>
          <h1>Header One</h1>
          <h2>Header Two</h2>
          <p>Paragraph one</p>
          <p>Paragraph two</p>
        </body>
        </html>
        """;

    /// <summary>
    ///     Verifies that Create succeeds given valid query data.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Create_ValidData_CreatesHtmlAssert()
    {
        // Arrange
        var data = new List<FileAssertQueryData>
        {
            new() { Query = "//p", Count = 2 }
        };

        // Act
        var htmlAssert = FileAssertHtmlAssert.Create(data);

        // Assert
        Assert.NotNull(htmlAssert);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Create_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertHtmlAssert.Create(null!));
    }

    /// <summary>
    ///     Verifies that Run produces no error when the XPath count matches exactly.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_ExactCount_Matches_NoError()
    {
        // Arrange - write sample HTML with 2 paragraph elements
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleHtml);
            var data = new List<FileAssertQueryData> { new() { Query = "//p", Count = 2 } };
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            htmlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the XPath count does not match exactly.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_ExactCount_Mismatch_WritesError()
    {
        // Arrange - sample HTML has 2 paragraphs but we assert count = 5
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleHtml);
            var data = new List<FileAssertQueryData> { new() { Query = "//p", Count = 5 } };
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            htmlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the XPath count is within min/max bounds.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_MinMaxCount_WithinBounds_NoError()
    {
        // Arrange - sample HTML has 2 paragraphs; assert min=1, max=4
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleHtml);
            var data = new List<FileAssertQueryData> { new() { Query = "//p", Min = 1, Max = 4 } };
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            htmlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run parses syntactically imperfect HTML (missing closing tags) leniently
    ///     and still evaluates XPath queries successfully.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_MalformedHtml_ParsesAndQueriesSuccessfully_NoError()
    {
        // Arrange - HTML with missing closing </li>, </ul>, </body>, and </html> tags
        const string malformedHtml = """
            <html>
            <body>
            <ul>
              <li>Item one
              <li>Item two
              <li>Item three
            """;
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, malformedHtml);
            var data = new List<FileAssertQueryData> { new() { Query = "//li", Count = 3 } };
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act - the lenient parser repairs the markup so the XPath query can run
            htmlAssert.Run(context, container, fileName);

            // Assert - all three list items are found despite the missing closing tags
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the file does not exist and cannot be parsed.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_NonExistentFile_WritesError()
    {
        // Arrange - use a filename that does not exist inside the temp directory to trigger a parse failure
        var missingFileName = $"does_not_exist_{Guid.NewGuid():N}.html";
        var data = new List<FileAssertQueryData> { new() { Query = "//p", Count = 1 } };
        var htmlAssert = FileAssertHtmlAssert.Create(data);
        using var context = Context.Create(["--silent"]);
        using var container = new DirectoryFileContainer(Path.GetTempPath());

        // Act
        htmlAssert.Run(context, container, missingFileName);

        // Assert
        Assert.Equal(1, context.ExitCode);
    }

    /// <summary>
    ///     Verifies that Run reports an error when the XPath query has invalid syntax.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_InvalidXPathQuery_WritesError()
    {
        // Arrange - valid HTML but an XPath expression with invalid syntax
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleHtml);
            var data = new List<FileAssertQueryData> { new() { Query = "//p[invalid", Count = 1 } };
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            htmlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when an XPath query selects HTML nodes by exact text.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_XPathExactTextMatch_Matches_NoError()
    {
        // Arrange - sample HTML has a <p> with text "Paragraph one"; query for exact match
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleHtml);
            var data = new List<FileAssertQueryData> { new() { Query = "//p[text()='Paragraph one']", Count = 1 } };
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            htmlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when an XPath exact text query finds no matching nodes.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_XPathExactTextMatch_NoMatch_WritesError()
    {
        // Arrange - no <p> has text "No such paragraph"; query should return 0 nodes
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleHtml);
            var data = new List<FileAssertQueryData> { new() { Query = "//p[text()='No such paragraph']", Min = 1 } };
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            htmlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when an XPath contains() predicate matches an HTML node.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_XPathContainsText_Matches_NoError()
    {
        // Arrange - sample HTML has paragraphs containing "Paragraph"; substring query returns 2
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleHtml);
            var data = new List<FileAssertQueryData> { new() { Query = "//p[contains(text(),'Paragraph')]", Count = 2 } };
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            htmlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when an XPath contains() predicate finds no matching nodes.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_XPathContainsText_NoMatch_WritesError()
    {
        // Arrange - no <p> contains "xyz"; contains() query returns 0 nodes
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleHtml);
            var data = new List<FileAssertQueryData> { new() { Query = "//p[contains(text(),'xyz')]", Min = 1 } };
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            htmlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the node count is below the minimum.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_MinCount_BelowMinimum_WritesError()
    {
        // Arrange - sample HTML has 2 paragraphs; assert min = 5
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleHtml);
            var data = new List<FileAssertQueryData> { new() { Query = "//p", Min = 5 } };
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            htmlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the node count exceeds the maximum.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_MaxCount_ExceedsMaximum_WritesError()
    {
        // Arrange - sample HTML has 2 paragraphs; assert max = 1
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleHtml);
            var data = new List<FileAssertQueryData> { new() { Query = "//p", Max = 1 } };
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            htmlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an IO error when the entry cannot be opened.
    /// </summary>
    [Fact]
    public void FileAssertHtmlAssert_Run_UnauthorizedAccess_WritesError()
    {
        // Arrange - a container whose OpenEntry raises an access-denied failure
        var data = new List<FileAssertQueryData> { new() { Query = "//p", Count = 1 } };
        var htmlAssert = FileAssertHtmlAssert.Create(data);
        var context = new CapturingContext();
        var container = new ThrowingFileContainer();

        // Act
        htmlAssert.Run(context, container, "page.html");

        // Assert: the IO failure is reported
        Assert.Single(context.Errors);
        Assert.Contains("could not be parsed as an HTML document", context.Errors[0]);
    }

    /// <summary>
    ///     Captures error messages written via <see cref="WriteError"/> for assertion in tests.
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
        public IContext WithPrefix(string prefix) => this;
    }

    /// <summary>
    ///     A file container whose <see cref="OpenEntry"/> raises an <see cref="UnauthorizedAccessException"/>
    ///     to simulate an IO failure while reading an entry.
    /// </summary>
    private sealed class ThrowingFileContainer : IFileContainer
    {
        /// <inheritdoc/>
        public IReadOnlyList<string> GetEntries() => ["page.html"];

        /// <inheritdoc/>
        public Stream OpenEntry(string entryPath) => throw new UnauthorizedAccessException("denied");

        /// <inheritdoc/>
        public long GetEntrySize(string entryPath) => 0;

        /// <inheritdoc/>
        public string GetDisplayPath(string entryPath) => entryPath;
    }
}
