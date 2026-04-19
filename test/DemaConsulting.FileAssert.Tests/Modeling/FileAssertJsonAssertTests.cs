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
///     Unit tests for the <see cref="FileAssertJsonAssert"/> class.
/// </summary>
[TestClass]
public sealed class FileAssertJsonAssertTests
{
    private const string SampleJson = """
        {
          "tools": [
            { "name": "tool1" },
            { "name": "tool2" },
            { "name": "tool3" }
          ],
          "version": "1.0.0"
        }
        """;

    /// <summary>
    ///     Verifies that Create succeeds given valid query data.
    /// </summary>
    [TestMethod]
    public void FileAssertJsonAssert_Create_ValidData_CreatesJsonAssert()
    {
        // Arrange
        var data = new List<FileAssertQueryData>
        {
            new() { Query = "tools", Count = 3 }
        };

        // Act
        var jsonAssert = FileAssertJsonAssert.Create(data);

        // Assert
        Assert.IsNotNull(jsonAssert);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [TestMethod]
    public void FileAssertJsonAssert_Create_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => FileAssertJsonAssert.Create(null!));
    }

    /// <summary>
    ///     Verifies that Run reports an error when the file cannot be parsed as JSON.
    /// </summary>
    [TestMethod]
    public void FileAssertJsonAssert_Run_InvalidFile_WritesError()
    {
        // Arrange - create a non-JSON file
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "this is not JSON");
            var data = new List<FileAssertQueryData> { new() { Query = "tools", Count = 1 } };
            var jsonAssert = FileAssertJsonAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            jsonAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the array count matches exactly.
    /// </summary>
    [TestMethod]
    public void FileAssertJsonAssert_Run_ArrayCount_Matches_NoError()
    {
        // Arrange - sample JSON has 3 tools entries; assert count = 3
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleJson);
            var data = new List<FileAssertQueryData> { new() { Query = "tools", Count = 3 } };
            var jsonAssert = FileAssertJsonAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            jsonAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the array count does not match exactly.
    /// </summary>
    [TestMethod]
    public void FileAssertJsonAssert_Run_ArrayCount_Mismatch_WritesError()
    {
        // Arrange - sample JSON has 3 tools but we assert count = 5
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleJson);
            var data = new List<FileAssertQueryData> { new() { Query = "tools", Count = 5 } };
            var jsonAssert = FileAssertJsonAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            jsonAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the array count is within min/max bounds.
    /// </summary>
    [TestMethod]
    public void FileAssertJsonAssert_Run_MinMaxCount_WithinBounds_NoError()
    {
        // Arrange - sample JSON has 3 tools entries; assert min=2, max=5
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleJson);
            var data = new List<FileAssertQueryData> { new() { Query = "tools", Min = 2, Max = 5 } };
            var jsonAssert = FileAssertJsonAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            jsonAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that a scalar value counts as 1.
    /// </summary>
    [TestMethod]
    public void FileAssertJsonAssert_Run_ScalarValue_CountsAsOne_NoError()
    {
        // Arrange - sample JSON has a scalar "version" key; assert count = 1
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleJson);
            var data = new List<FileAssertQueryData> { new() { Query = "version", Count = 1 } };
            var jsonAssert = FileAssertJsonAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            jsonAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
