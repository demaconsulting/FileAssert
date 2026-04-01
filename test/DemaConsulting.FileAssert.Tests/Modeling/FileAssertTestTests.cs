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
///     Unit tests for the <see cref="FileAssertTest"/> class.
/// </summary>
[TestClass]
public class FileAssertTestTests
{
    /// <summary>
    ///     Verifies that Create succeeds given valid data.
    /// </summary>
    [TestMethod]
    public void FileAssertTest_Create_ValidData_CreatesTest()
    {
        // Arrange
        var data = new FileAssertTestData
        {
            Name = "My Test",
            Tags = ["tag1", "tag2"]
        };

        // Act
        var test = FileAssertTest.Create(data);

        // Assert
        Assert.AreEqual("My Test", test.Name);
        Assert.AreEqual(2, test.Tags.Count);
        Assert.AreEqual(0, test.Files.Count);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [TestMethod]
    public void FileAssertTest_Create_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertTest.Create(null!));
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when Name is null.
    /// </summary>
    [TestMethod]
    public void FileAssertTest_Create_NullName_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new FileAssertTestData { Name = null };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => FileAssertTest.Create(data));
        Assert.Contains("name", exception.Message);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when Name is whitespace.
    /// </summary>
    [TestMethod]
    public void FileAssertTest_Create_WhitespaceName_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new FileAssertTestData { Name = "   " };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => FileAssertTest.Create(data));
        Assert.Contains("name", exception.Message);
    }

    /// <summary>
    ///     Verifies that MatchesFilter returns true when the filter list is empty.
    /// </summary>
    [TestMethod]
    public void FileAssertTest_MatchesFilter_EmptyFilters_ReturnsTrue()
    {
        // Arrange
        var test = FileAssertTest.Create(new FileAssertTestData { Name = "Alpha" });

        // Act
        var result = test.MatchesFilter([]);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Verifies that MatchesFilter returns true when a filter matches the test name.
    /// </summary>
    [TestMethod]
    public void FileAssertTest_MatchesFilter_MatchingName_ReturnsTrue()
    {
        // Arrange
        var test = FileAssertTest.Create(new FileAssertTestData { Name = "Alpha" });

        // Act
        var result = test.MatchesFilter(["Alpha"]);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Verifies that MatchesFilter returns true when a filter matches one of the test's tags.
    /// </summary>
    [TestMethod]
    public void FileAssertTest_MatchesFilter_MatchingTag_ReturnsTrue()
    {
        // Arrange
        var test = FileAssertTest.Create(new FileAssertTestData
        {
            Name = "Alpha",
            Tags = ["smoke", "regression"]
        });

        // Act
        var result = test.MatchesFilter(["smoke"]);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Verifies that MatchesFilter returns false when no filter matches the name or tags.
    /// </summary>
    [TestMethod]
    public void FileAssertTest_MatchesFilter_NonMatchingFilter_ReturnsFalse()
    {
        // Arrange
        var test = FileAssertTest.Create(new FileAssertTestData
        {
            Name = "Alpha",
            Tags = ["smoke"]
        });

        // Act
        var result = test.MatchesFilter(["Beta"]);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    ///     Verifies that MatchesFilter name comparison is case-insensitive.
    /// </summary>
    [TestMethod]
    public void FileAssertTest_MatchesFilter_CaseInsensitiveName_ReturnsTrue()
    {
        // Arrange
        var test = FileAssertTest.Create(new FileAssertTestData { Name = "Alpha Test" });

        // Act
        var result = test.MatchesFilter(["ALPHA TEST"]);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Verifies that MatchesFilter tag comparison is case-insensitive.
    /// </summary>
    [TestMethod]
    public void FileAssertTest_MatchesFilter_CaseInsensitiveTag_ReturnsTrue()
    {
        // Arrange
        var test = FileAssertTest.Create(new FileAssertTestData
        {
            Name = "Alpha",
            Tags = ["smoke"]
        });

        // Act
        var result = test.MatchesFilter(["SMOKE"]);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Verifies that Run executes all file assertions in the test.
    /// </summary>
    [TestMethod]
    public void FileAssertTest_Run_RunsAllFiles()
    {
        // Arrange - create a temp directory with a file matching the pattern
        var tempDir = Directory.CreateTempSubdirectory("fileassert_test_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "sample.txt"), "content");
            var data = new FileAssertTestData
            {
                Name = "Run Test",
                Files = [new FileAssertFileData { Pattern = "*.txt", Min = 1 }]
            };
            var test = FileAssertTest.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            test.Run(context, tempDir.FullName);

            // Assert - min=1 would have produced an error if the file had not been found
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
