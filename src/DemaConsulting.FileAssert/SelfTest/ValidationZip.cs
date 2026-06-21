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

namespace DemaConsulting.FileAssert.SelfTest;

/// <summary>
///     Provides the ZIP self-validation test for FileAssert.
/// </summary>
internal static partial class Validation
{
    /// <summary>
    ///     Runs a test for ZIP entry assertions against a programmatically created ZIP archive.
    /// </summary>
    /// <remarks>
    ///     Verifies that the FileAssert_Zip assertion correctly locates a named entry inside a
    ///     ZIP archive and checks its text content, confirming ZIP assertions work end-to-end.
    /// </remarks>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunZipTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "FileAssert_Zip", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var configFile = tempDir.GetFilePath(".fileassert.yaml");

            // Create a ZIP archive containing a single text entry with known content
            var zipPath = tempDir.GetFilePath("test.zip");
            using (var zip = System.IO.Compression.ZipFile.Open(zipPath, System.IO.Compression.ZipArchiveMode.Create))
            {
                var entry = zip.CreateEntry("hello.txt");
                using var writer = new System.IO.StreamWriter(entry.Open());
                writer.Write("Hello Zip");
            }

            // Write a config that verifies the ZIP contains hello.txt with the expected content
            File.WriteAllText(configFile,
                """
                tests:
                  - name: FileAssert_Zip
                    files:
                      - pattern: "*.zip"
                        count: 1
                        zip:
                          files:
                            - pattern: "hello.txt"
                              count: 1
                              text:
                                - contains: "Hello Zip"
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
