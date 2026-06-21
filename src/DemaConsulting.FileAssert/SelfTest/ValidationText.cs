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
///     Provides the plain-text self-validation test for FileAssert.
/// </summary>
internal static partial class Validation
{
    /// <summary>
    ///     Runs a test for file-contains checking against plain-text files.
    /// </summary>
    /// <remarks>
    ///     Verifies that the FileAssert_Text assertion correctly detects a known substring
    ///     inside a matching file, confirming text-content assertions work end-to-end.
    /// </remarks>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunTextTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "FileAssert_Text", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var configFile = tempDir.GetFilePath(".fileassert.yaml");

            // Create a text file with known content for the contains assertion
            File.WriteAllText(tempDir.GetFilePath("hello.txt"), "Hello World");

            // Write a config that verifies the file contains the expected text
            File.WriteAllText(configFile,
                """
                tests:
                  - name: FileAssert_Text
                    files:
                      - pattern: "*.txt"
                        text:
                          - contains: "Hello World"
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
