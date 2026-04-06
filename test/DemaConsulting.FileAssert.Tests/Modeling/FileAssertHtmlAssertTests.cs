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

namespace DemaConsulting.FileAssert.Tests.Modeling;

/// <summary>
///     Unit tests for the <see cref="FileAssertHtmlAssert"/> class.
/// </summary>
[TestClass]
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
    [TestMethod]
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
        Assert.IsNotNull(htmlAssert);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [TestMethod]
    public void FileAssertHtmlAssert_Create_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertHtmlAssert.Create(null!));
    }

    /// <summary>
    ///     Verifies that Run produces no error when the XPath count matches exactly.
    /// </summary>
    [TestMethod]
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

            // Act
            htmlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the XPath count does not match exactly.
    /// </summary>
    [TestMethod]
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

            // Act
            htmlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the XPath count is within min/max bounds.
    /// </summary>
    [TestMethod]
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

            // Act
            htmlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the file does not exist and cannot be parsed.
    /// </summary>
    [TestMethod]
    public void FileAssertHtmlAssert_Run_NonExistentFile_WritesError()
    {
        // Arrange - use a path that does not exist to trigger a parse failure
        var missingFile = Path.Combine(Path.GetTempPath(), $"does_not_exist_{Guid.NewGuid():N}.html");
        var data = new List<FileAssertQueryData> { new() { Query = "//p", Count = 1 } };
        var htmlAssert = FileAssertHtmlAssert.Create(data);
        using var context = Context.Create(["--silent"]);

        // Act
        htmlAssert.Run(context, missingFile);

        // Assert
        Assert.AreEqual(1, context.ExitCode);
    }
}
