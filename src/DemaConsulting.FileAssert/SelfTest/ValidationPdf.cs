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
using DemaConsulting.FileAssert.Utilities;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace DemaConsulting.FileAssert.SelfTest;

/// <summary>
///     Provides the PDF self-validation test for FileAssert.
/// </summary>
internal static partial class Validation
{
    /// <summary>
    ///     Runs a test for PDF metadata and text assertions against a programmatically created PDF file.
    /// </summary>
    /// <remarks>
    ///     Verifies that the FileAssert_Pdf assertion correctly reads document metadata and
    ///     extracted page text from a PDF file, confirming PDF assertions work end-to-end.
    /// </remarks>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunPdfTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "FileAssert_Pdf", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var configFile = tempDir.GetFilePath(".fileassert.yaml");

            // Build a minimal PDF in memory using PdfPig's writer API
            var builder = new PdfDocumentBuilder();
            builder.DocumentInformation.Title = "Test PDF";
            var font = builder.AddStandard14Font(Standard14Font.Helvetica);
            var page = builder.AddPage(PageSize.A4);
            page.AddText("Hello PDF", 12, new PdfPoint(25, 700), font);
            var pdfBytes = builder.Build();
            File.WriteAllBytes(tempDir.GetFilePath("test.pdf"), pdfBytes);

            // Write a config that verifies the PDF metadata title and body text
            File.WriteAllText(configFile,
                """
                tests:
                  - name: FileAssert_Pdf
                    files:
                      - pattern: "*.pdf"
                        count: 1
                        pdf:
                          metadata:
                            - field: "Title"
                              contains: "Test PDF"
                          text:
                            - contains: "Hello PDF"
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
}
