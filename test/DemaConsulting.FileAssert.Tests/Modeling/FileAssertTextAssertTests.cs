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
///     Unit tests for the <see cref="FileAssertTextAssert"/> class.
/// </summary>
[Collection("Sequential")]
public sealed class FileAssertTextAssertTests
{
    /// <summary>
    ///     Verifies that Create succeeds given valid data.
    /// </summary>
    [Fact]
    public void FileAssertTextAssert_Create_ValidData_CreatesTextAssert()
    {
        // Arrange
        var data = new List<FileAssertRuleData>
        {
            new() { Contains = "hello" }
        };

        // Act
        var textAssert = FileAssertTextAssert.Create(data);

        // Assert
        Assert.NotNull(textAssert);
        Assert.Single(textAssert.Rules);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [Fact]
    public void FileAssertTextAssert_Create_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertTextAssert.Create(null!));
    }

    /// <summary>
    ///     Verifies that Run produces no error when the file contains the required text.
    /// </summary>
    [Fact]
    public void FileAssertTextAssert_Run_FileContainsText_NoError()
    {
        // Arrange - create a temp file with content that satisfies the rule
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "hello world", System.Text.Encoding.UTF8);
            var data = new List<FileAssertRuleData> { new() { Contains = "hello" } };
            var textAssert = FileAssertTextAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            textAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the file does not contain the required text.
    /// </summary>
    [Fact]
    public void FileAssertTextAssert_Run_FileMissingText_WritesError()
    {
        // Arrange - create a temp file with content that does NOT satisfy the rule
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "goodbye world", System.Text.Encoding.UTF8);
            var data = new List<FileAssertRuleData> { new() { Contains = "hello" } };
            var textAssert = FileAssertTextAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            textAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the file cannot be read (I/O error).
    /// </summary>
    [Fact]
    public void FileAssertTextAssert_Run_NonExistentFile_WritesError()
    {
        // Arrange - use a path that does not exist to trigger an I/O failure
        var missingFile = Path.Combine(Path.GetTempPath(), $"does_not_exist_{Guid.NewGuid():N}.txt");
        var data = new List<FileAssertRuleData> { new() { Contains = "hello" } };
        var textAssert = FileAssertTextAssert.Create(data);
        using var context = Context.Create(["--silent"]);

        // Act
        textAssert.Run(context, missingFile);

        // Assert - an error was reported
        Assert.Equal(1, context.ExitCode);
        Assert.Equal(1, context.ErrorCount);
    }

    /// <summary>
    ///     Verifies that Run evaluates all rules and reports multiple errors without
    ///     short-circuiting on the first failure.
    /// </summary>
    [Fact]
    public void FileAssertTextAssert_Run_MultipleRulesMultipleViolations_WritesMultipleErrors()
    {
        // Arrange - create a temp file whose content satisfies neither rule
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "no matches here", System.Text.Encoding.UTF8);
            var data = new List<FileAssertRuleData>
            {
                new() { Contains = "first-missing" },
                new() { Contains = "second-missing" }
            };
            var textAssert = FileAssertTextAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            textAssert.Run(context, tempFile);

            // Assert - both rules must have been evaluated (no short-circuit)
            Assert.Equal(2, context.ErrorCount);
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
