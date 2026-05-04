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
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace DemaConsulting.FileAssert.Tests.Modeling;

/// <summary>
///     Unit tests for the <see cref="FileAssertPdfAssert"/> class.
/// </summary>
[Collection("Sequential")]
public sealed class FileAssertPdfAssertTests
{
    /// <summary>
    ///     Verifies that Create succeeds given valid data.
    /// </summary>
    [Fact]
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
        Assert.NotNull(pdfAssert);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Create_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertPdfAssert.Create(null!));
    }

    /// <summary>
    ///     Verifies that Run reports an error when the file cannot be parsed as PDF.
    /// </summary>
    [Fact]
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
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the PDF satisfies all page count constraints.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Run_ValidPdf_PageCountSatisfied_NoError()
    {
        // Arrange - build a single-page PDF and assert min=1, max=5
        var tempFile = Path.GetTempFileName();
        try
        {
            using var builder = new PdfDocumentBuilder();
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
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the PDF has fewer pages than the minimum.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Run_ValidPdf_TooFewPages_WritesError()
    {
        // Arrange - build a single-page PDF but require at least 5 pages
        var tempFile = Path.GetTempFileName();
        try
        {
            using var builder = new PdfDocumentBuilder();
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
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the PDF has more pages than the maximum.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Run_ValidPdf_TooManyPages_WritesError()
    {
        // Arrange - build a three-page PDF but allow at most 2 pages
        var tempFile = Path.GetTempFileName();
        try
        {
            using var builder = new PdfDocumentBuilder();
            builder.AddPage(PageSize.A4);
            builder.AddPage(PageSize.A4);
            builder.AddPage(PageSize.A4);
            File.WriteAllBytes(tempFile, builder.Build());

            var data = new FileAssertPdfData
            {
                Pages = new FileAssertPdfPagesData { Max = 2 }
            };
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when a metadata contains assertion fails.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Run_MetadataContainsRule_FieldMissing_WritesError()
    {
        // Arrange - build a PDF without metadata; assert Title contains "Test"
        var tempFile = Path.GetTempFileName();
        try
        {
            using var builder = new PdfDocumentBuilder();
            builder.AddPage(PageSize.A4);
            File.WriteAllBytes(tempFile, builder.Build());

            var data = new FileAssertPdfData
            {
                Metadata =
                [
                    new FileAssertPdfMetadataRuleData { Field = "Title", Contains = "Test" }
                ]
            };
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when a text contains rule fails.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Run_TextRule_ContentMissing_WritesError()
    {
        // Arrange - build a PDF with no text content; assert text contains "Hello"
        var tempFile = Path.GetTempFileName();
        try
        {
            using var builder = new PdfDocumentBuilder();
            builder.AddPage(PageSize.A4);
            File.WriteAllBytes(tempFile, builder.Build());

            var data = new FileAssertPdfData
            {
                Text =
                [
                    new FileAssertRuleData { Contains = "Hello" }
                ]
            };
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the PDF metadata Title contains the required string.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Run_MetadataContainsRule_TitleMatches_NoError()
    {
        // Arrange - build a PDF with Title metadata set and assert it contains "Annual"
        var tempFile = Path.GetTempFileName();
        try
        {
            using var builder = new PdfDocumentBuilder();
            builder.DocumentInformation.Title = "Annual Report 2024";
            builder.AddPage(PageSize.A4);
            File.WriteAllBytes(tempFile, builder.Build());

            var data = new FileAssertPdfData
            {
                Metadata =
                [
                    new FileAssertPdfMetadataRuleData { Field = "Title", Contains = "Annual" }
                ]
            };
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the PDF metadata Author field is checked.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Run_MetadataContainsRule_AuthorField_NoError()
    {
        // Arrange - build a PDF with Author metadata and assert that field contains expected text
        var tempFile = Path.GetTempFileName();
        try
        {
            using var builder = new PdfDocumentBuilder();
            builder.DocumentInformation.Author = "DEMA Consulting";
            builder.AddPage(PageSize.A4);
            File.WriteAllBytes(tempFile, builder.Build());

            var data = new FileAssertPdfData
            {
                Metadata =
                [
                    new FileAssertPdfMetadataRuleData { Field = "Author", Contains = "DEMA" }
                ]
            };
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when a PDF metadata matches regex rule succeeds.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Run_MetadataMatchesRule_Matches_NoError()
    {
        // Arrange - build a PDF with Title set; assert it matches regex pattern
        var tempFile = Path.GetTempFileName();
        try
        {
            using var builder = new PdfDocumentBuilder();
            builder.DocumentInformation.Title = "Report 2024";
            builder.AddPage(PageSize.A4);
            File.WriteAllBytes(tempFile, builder.Build());

            var data = new FileAssertPdfData
            {
                Metadata =
                [
                    new FileAssertPdfMetadataRuleData { Field = "Title", Matches = @"Report \d{4}" }
                ]
            };
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when a PDF metadata matches regex rule does not match.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Run_MetadataMatchesRule_NoMatch_WritesError()
    {
        // Arrange - build a PDF with Title set; assert it matches a pattern it does not satisfy
        var tempFile = Path.GetTempFileName();
        try
        {
            using var builder = new PdfDocumentBuilder();
            builder.DocumentInformation.Title = "Annual Report";
            builder.AddPage(PageSize.A4);
            File.WriteAllBytes(tempFile, builder.Build());

            var data = new FileAssertPdfData
            {
                Metadata =
                [
                    new FileAssertPdfMetadataRuleData { Field = "Title", Matches = @"^\d{4}$" }
                ]
            };
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the PDF text contains the required string.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Run_TextContainsRule_ContentPresent_NoError()
    {
        // Arrange - build a PDF with text "Hello World" and assert text contains "Hello"
        var tempFile = Path.GetTempFileName();
        try
        {
            using var builder = new PdfDocumentBuilder();
            var page = builder.AddPage(PageSize.A4);
            var font = builder.AddStandard14Font(Standard14Font.Helvetica);
            page.AddText("Hello World", 12, new PdfPoint(50, 700), font);
            File.WriteAllBytes(tempFile, builder.Build());

            var data = new FileAssertPdfData
            {
                Text =
                [
                    new FileAssertRuleData { Contains = "Hello" }
                ]
            };
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the PDF text matches a regex pattern.
    /// </summary>
    [Fact]
    public void FileAssertPdfAssert_Run_TextMatchesRule_PatternMatches_NoError()
    {
        // Arrange - build a PDF with text "Hello World 2024" and assert it matches a regex
        var tempFile = Path.GetTempFileName();
        try
        {
            using var builder = new PdfDocumentBuilder();
            var page = builder.AddPage(PageSize.A4);
            var font = builder.AddStandard14Font(Standard14Font.Helvetica);
            page.AddText("Hello World 2024", 12, new PdfPoint(50, 700), font);
            File.WriteAllBytes(tempFile, builder.Build());

            var data = new FileAssertPdfData
            {
                Text =
                [
                    new FileAssertRuleData { Matches = @"Hello World \d{4}" }
                ]
            };
            var pdfAssert = FileAssertPdfAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            pdfAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
