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
///     Unit tests for the <see cref="FileAssertFile"/> class.
/// </summary>
[TestClass]
public class FileAssertFileTests
{
    /// <summary>
    ///     Verifies that Create succeeds given valid data.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Create_ValidData_CreatesFile()
    {
        // Arrange
        var data = new FileAssertFileData { Pattern = "**/*.txt", Min = 1, Max = 10 };

        // Act
        var file = FileAssertFile.Create(data);

        // Assert
        Assert.AreEqual("**/*.txt", file.Pattern);
        Assert.AreEqual(1, file.Min);
        Assert.AreEqual(10, file.Max);
        Assert.AreEqual(0, file.Rules.Count);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Create_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertFile.Create(null!));
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when Pattern is null.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Create_NullPattern_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new FileAssertFileData { Pattern = null };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => FileAssertFile.Create(data));
        Assert.Contains("pattern", exception.Message);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when Pattern is blank.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Create_BlankPattern_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new FileAssertFileData { Pattern = "   " };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => FileAssertFile.Create(data));
        Assert.Contains("pattern", exception.Message);
    }

    /// <summary>
    ///     Verifies that Run produces no error when there are no matching files and no constraints.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Run_NoMatchingFiles_NoConstraints_NoError()
    {
        // Arrange - use an empty temp directory so the pattern matches nothing
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            var data = new FileAssertFileData { Pattern = "*.txt" };
            var file = FileAssertFile.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            file.Run(context, tempDir.FullName);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when files are found and no constraints are set.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Run_WithMatchingFiles_NoConstraints_NoError()
    {
        // Arrange - create a temp file for the pattern to match
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "sample.txt"), "content");
            var data = new FileAssertFileData { Pattern = "*.txt" };
            var file = FileAssertFile.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            file.Run(context, tempDir.FullName);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when fewer files are found than the minimum.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Run_TooFewFiles_WritesError()
    {
        // Arrange - empty directory so zero files match, but min requires at least 1
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            var data = new FileAssertFileData { Pattern = "*.txt", Min = 1 };
            var file = FileAssertFile.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            file.Run(context, tempDir.FullName);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when more files are found than the maximum.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Run_TooManyFiles_WritesError()
    {
        // Arrange - create two files but constrain max to 1
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "a.txt"), "content a");
            File.WriteAllText(Path.Combine(tempDir.FullName, "b.txt"), "content b");
            var data = new FileAssertFileData { Pattern = "*.txt", Max = 1 };
            var file = FileAssertFile.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            file.Run(context, tempDir.FullName);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when a content rule is satisfied by the matching file.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Run_WithContentRule_ContentContainsValue_NoError()
    {
        // Arrange - create a file that satisfies the contains rule
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "check.txt"), "expected content here");
            var data = new FileAssertFileData
            {
                Pattern = "*.txt",
                Rules = [new FileAssertRuleData { Contains = "expected content" }]
            };
            var file = FileAssertFile.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            file.Run(context, tempDir.FullName);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when a content rule is not satisfied by the matching file.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Run_WithContentRule_ContentMissingValue_WritesError()
    {
        // Arrange - create a file that does NOT satisfy the contains rule
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "check.txt"), "unrelated content");
            var data = new FileAssertFileData
            {
                Pattern = "*.txt",
                Rules = [new FileAssertRuleData { Contains = "expected content" }]
            };
            var file = FileAssertFile.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            file.Run(context, tempDir.FullName);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the file count does not match the exact count constraint.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Run_WrongCount_WritesError()
    {
        // Arrange - create two files but constrain count to exactly 1
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "a.txt"), "content a");
            File.WriteAllText(Path.Combine(tempDir.FullName, "b.txt"), "content b");
            var data = new FileAssertFileData { Pattern = "*.txt", Count = 1 };
            var file = FileAssertFile.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            file.Run(context, tempDir.FullName);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when a file is smaller than the minimum size.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Run_TooSmall_WritesError()
    {
        // Arrange - create an empty file and require at least 10 bytes
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "small.txt"), string.Empty);
            var data = new FileAssertFileData { Pattern = "*.txt", MinSize = 10 };
            var file = FileAssertFile.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            file.Run(context, tempDir.FullName);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when a file exceeds the maximum size.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Run_TooLarge_WritesError()
    {
        // Arrange - create a file with content larger than 5 bytes
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "large.txt"), "this content is more than five bytes");
            var data = new FileAssertFileData { Pattern = "*.txt", MaxSize = 5 };
            var file = FileAssertFile.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            file.Run(context, tempDir.FullName);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run checks size constraints against every matched file, not just the first,
    ///     by confirming one error is reported per violating file regardless of enumeration order.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Run_MultipleFiles_MultipleViolateSizeConstraints_WritesErrorForEachViolation()
    {
        // Arrange - three files: one within bounds, one too small, one too large
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "ok.txt"), "valid");
            File.WriteAllText(Path.Combine(tempDir.FullName, "small.txt"), string.Empty);
            File.WriteAllText(Path.Combine(tempDir.FullName, "large.txt"), "this file is too large");
            var data = new FileAssertFileData { Pattern = "*.txt", MinSize = 2, MaxSize = 10 };
            var file = FileAssertFile.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            file.Run(context, tempDir.FullName);

            // Assert - both invalid files should trigger errors regardless of enumeration order
            Assert.AreEqual(1, context.ExitCode);
            Assert.AreEqual(2, context.ErrorCount);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that Run applies content rules to every matched file, not just the first,
    ///     by confirming one error is reported per violating file regardless of enumeration order.
    /// </summary>
    [TestMethod]
    public void FileAssertFile_Run_MultipleFiles_MultipleFailContentRule_WritesErrorForEachViolation()
    {
        // Arrange - three files: one with the required content, two without
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "good.txt"), "expected content here");
            File.WriteAllText(Path.Combine(tempDir.FullName, "bad1.txt"), "unrelated content");
            File.WriteAllText(Path.Combine(tempDir.FullName, "bad2.txt"), "also unrelated");
            var data = new FileAssertFileData
            {
                Pattern = "*.txt",
                Rules = [new FileAssertRuleData { Contains = "expected content" }]
            };
            var file = FileAssertFile.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            file.Run(context, tempDir.FullName);

            // Assert - both bad files should trigger errors regardless of enumeration order
            Assert.AreEqual(1, context.ExitCode);
            Assert.AreEqual(2, context.ErrorCount);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
