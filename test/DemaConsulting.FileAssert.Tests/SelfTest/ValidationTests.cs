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
using DemaConsulting.FileAssert.SelfTest;

namespace DemaConsulting.FileAssert.Tests.SelfTest;

/// <summary>
///     Unit tests for the Validation class.
/// </summary>
[TestClass]
public class ValidationTests
{
    /// <summary>
    ///     Test that Run throws ArgumentNullException when context is null.
    /// </summary>
    [TestMethod]
    public void Validation_Run_NullContext_ThrowsArgumentNullException()
    {
        // Arrange
        // No setup required — null is the input under test.

        // Act & Assert
        // Proves that Run guards against null context with ArgumentNullException.
        Assert.ThrowsExactly<ArgumentNullException>(() => Validation.Run(null!));
    }

    /// <summary>
    ///     Test that Run prints a summary containing total, passed, and failed counts.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithSilentContext_PrintsSummary()
    {
        // Arrange
        // A unique log file path is used to capture output from the silent context.
        var logFile = Path.Combine(Path.GetTempPath(), $"validation_test_{Guid.NewGuid()}.log");
        try
        {
            using (var context = Context.Create(["--silent", "--log", logFile]))
            {
                // Act
                Validation.Run(context);
            }

            // Assert
            // Proves that Run writes the three expected summary lines to the output.
            // The context is disposed above so the log file is fully flushed and closed.
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Total Tests:", logContent);
            Assert.Contains("Passed:", logContent);
            Assert.Contains("Failed:", logContent);
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
    ///     Test that Run exits with code zero when all self-validation tests pass.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithSilentContext_ExitCodeIsZero()
    {
        // Arrange
        using var context = Context.Create(["--silent"]);

        // Act
        Validation.Run(context);

        // Assert
        // Proves that a successful validation run leaves the exit code at 0.
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that Run writes a valid TRX file when the results path ends with .trx.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithTrxResultsFile_WritesTrxFile()
    {
        // Arrange
        var trxFile = Path.Combine(Path.GetTempPath(), $"validation_test_{Guid.NewGuid()}.trx");
        try
        {
            using var context = Context.Create(["--silent", "--results", trxFile]);

            // Act
            Validation.Run(context);

            // Assert
            // Proves that Run creates a TRX-format file at the requested path.
            Assert.IsTrue(File.Exists(trxFile));
            var content = File.ReadAllText(trxFile);
            Assert.Contains("<TestRun", content);
        }
        finally
        {
            if (File.Exists(trxFile))
            {
                File.Delete(trxFile);
            }
        }
    }

    /// <summary>
    ///     Test that Run writes a valid JUnit XML file when the results path ends with .xml.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithXmlResultsFile_WritesXmlFile()
    {
        // Arrange
        var xmlFile = Path.Combine(Path.GetTempPath(), $"validation_test_{Guid.NewGuid()}.xml");
        try
        {
            using var context = Context.Create(["--silent", "--results", xmlFile]);

            // Act
            Validation.Run(context);

            // Assert
            // Proves that Run creates a JUnit-format XML file at the requested path.
            Assert.IsTrue(File.Exists(xmlFile));
            var content = File.ReadAllText(xmlFile);
            Assert.Contains("<testsuites", content);
        }
        finally
        {
            if (File.Exists(xmlFile))
            {
                File.Delete(xmlFile);
            }
        }
    }

    /// <summary>
    ///     Test that Run does not write a results file when the extension is unsupported.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithUnsupportedResultsFormat_DoesNotWriteFile()
    {
        // Arrange
        var jsonFile = Path.Combine(Path.GetTempPath(), $"validation_test_{Guid.NewGuid()}.json");
        try
        {
            using var context = Context.Create(["--silent", "--results", jsonFile]);

            // Act
            Validation.Run(context);

            // Assert
            // Proves that Run does not create a file for unsupported formats.
            Assert.IsFalse(File.Exists(jsonFile));
        }
        finally
        {
            if (File.Exists(jsonFile))
            {
                File.Delete(jsonFile);
            }
        }
    }

    /// <summary>
    ///     Test that Run logs a passing FileAssert_Results result.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithSilentContext_LogContainsFileAssertResults()
    {
        // Act & Assert
        // Proves that the FileAssert_Results self-validation test is executed and passes.
        AssertLogContains("✓ FileAssert_Results - Passed");
    }

    /// <summary>
    ///     Test that Run logs a passing FileAssert_Exists result.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithSilentContext_LogContainsFileAssertExists()
    {
        // Act & Assert
        // Proves that the FileAssert_Exists self-validation test is executed and passes.
        AssertLogContains("✓ FileAssert_Exists - Passed");
    }

    /// <summary>
    ///     Test that Run logs a passing FileAssert_Contains result.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithSilentContext_LogContainsFileAssertContains()
    {
        // Act & Assert
        // Proves that the FileAssert_Contains self-validation test is executed and passes.
        AssertLogContains("✓ FileAssert_Contains - Passed");
    }

    /// <summary>
    ///     Runs <see cref="Validation.Run"/> with a silent context backed by a temporary log file,
    ///     then asserts that <paramref name="expectedText"/> appears in the log.
    /// </summary>
    /// <param name="expectedText">The text that must appear in the log output.</param>
    private static void AssertLogContains(string expectedText)
    {
        // Arrange
        // A unique log file path is used to capture output from the silent context.
        var logFile = Path.Combine(Path.GetTempPath(), $"validation_test_{Guid.NewGuid()}.log");
        try
        {
            using (var context = Context.Create(["--silent", "--log", logFile]))
            {
                // Act
                Validation.Run(context);
            }

            // Assert
            var logContent = File.ReadAllText(logFile);
            Assert.Contains(expectedText, logContent);
        }
        finally
        {
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
        }
    }
}
