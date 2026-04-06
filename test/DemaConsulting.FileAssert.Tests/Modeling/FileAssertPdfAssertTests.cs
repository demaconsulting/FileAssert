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
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Writer;

namespace DemaConsulting.FileAssert.Tests.Modeling;

/// <summary>
///     Unit tests for the <see cref="FileAssertPdfAssert"/> class.
/// </summary>
[TestClass]
public sealed class FileAssertPdfAssertTests
{
    /// <summary>
    ///     Verifies that Create succeeds given valid data.
    /// </summary>
    [TestMethod]
    public void FileAssertPdfAssert_Create_ValidData_CreatesPdfAssert()
    {
        // Arrange
        var data = new FileAssertPdfData
        {
            Pages = new FileAssertPdfPagesData { Min = 1 }
        };

        // Act
        var pdfAssert = FileAssertPdfAssert.Create(data);

        // Assert
        Assert.IsNotNull(pdfAssert);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [TestMethod]
    public void FileAssertPdfAssert_Create_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertPdfAssert.Create(null!));
    }

    /// <summary>
    ///     Verifies that Run reports an error when the file cannot be parsed as PDF.
    /// </summary>
    [TestMethod]
    public void FileAssertPdfAssert_Run_InvalidFile_WritesError()
    {
        // Arrange - create a temp file with non-PDF content
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "this is not a PDF file");
            var data = new FileAssertPdfData();
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the PDF satisfies all page count constraints.
    /// </summary>
    [TestMethod]
    public void FileAssertPdfAssert_Run_ValidPdf_PageCountSatisfied_NoError()
    {
        // Arrange - build a single-page PDF and assert min=1, max=5
        var tempFile = Path.GetTempFileName();
        try
        {
            var builder = new PdfDocumentBuilder();
            builder.AddPage(PageSize.A4);
            File.WriteAllBytes(tempFile, builder.Build());

            var data = new FileAssertPdfData
            {
                Pages = new FileAssertPdfPagesData { Min = 1, Max = 5 }
            };
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the PDF has fewer pages than the minimum.
    /// </summary>
    [TestMethod]
    public void FileAssertPdfAssert_Run_ValidPdf_TooFewPages_WritesError()
    {
        // Arrange - build a single-page PDF but require at least 5 pages
        var tempFile = Path.GetTempFileName();
        try
        {
            var builder = new PdfDocumentBuilder();
            builder.AddPage(PageSize.A4);
            File.WriteAllBytes(tempFile, builder.Build());

            var data = new FileAssertPdfData
            {
                Pages = new FileAssertPdfPagesData { Min = 5 }
            };
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
