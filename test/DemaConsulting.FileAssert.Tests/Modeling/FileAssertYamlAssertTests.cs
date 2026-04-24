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
///     Unit tests for the <see cref="FileAssertYamlAssert"/> class.
/// </summary>
[TestClass]
public sealed class FileAssertYamlAssertTests
{
    private const string SampleYaml = """
        tools:
          - name: tool1
          - name: tool2
          - name: tool3
        version: "1.0.0"
        """;

    /// <summary>
    ///     Verifies that Create succeeds given valid query data.
    /// </summary>
    [TestMethod]
    public void FileAssertYamlAssert_Create_ValidData_CreatesYamlAssert()
    {
        // Arrange
        var data = new List<FileAssertQueryData>
        {
            new() { Query = "tools", Count = 3 }
        };

        // Act
        var yamlAssert = FileAssertYamlAssert.Create(data);

        // Assert
        Assert.IsNotNull(yamlAssert);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [TestMethod]
    public void FileAssertYamlAssert_Create_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => FileAssertYamlAssert.Create(null!));
    }

    /// <summary>
    ///     Verifies that Run reports an error when the file cannot be parsed as YAML.
    /// </summary>
    [TestMethod]
    public void FileAssertYamlAssert_Run_InvalidFile_WritesError()
    {
        // Arrange - write malformed YAML content that will cause a parse error
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "key: [unclosed");
            var data = new List<FileAssertQueryData> { new() { Query = "key", Count = 1 } };
            var yamlAssert = FileAssertYamlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            yamlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the sequence count matches exactly.
    /// </summary>
    [TestMethod]
    public void FileAssertYamlAssert_Run_SequenceCount_Matches_NoError()
    {
        // Arrange - sample YAML has 3 tools entries; assert count = 3
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleYaml);
            var data = new List<FileAssertQueryData> { new() { Query = "tools", Count = 3 } };
            var yamlAssert = FileAssertYamlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            yamlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the sequence count does not match exactly.
    /// </summary>
    [TestMethod]
    public void FileAssertYamlAssert_Run_SequenceCount_Mismatch_WritesError()
    {
        // Arrange - sample YAML has 3 tools but we assert count = 5
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleYaml);
            var data = new List<FileAssertQueryData> { new() { Query = "tools", Count = 5 } };
            var yamlAssert = FileAssertYamlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            yamlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the sequence count is within min/max bounds.
    /// </summary>
    [TestMethod]
    public void FileAssertYamlAssert_Run_MinMaxCount_WithinBounds_NoError()
    {
        // Arrange - sample YAML has 3 tools entries; assert min=2, max=5
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleYaml);
            var data = new List<FileAssertQueryData> { new() { Query = "tools", Min = 2, Max = 5 } };
            var yamlAssert = FileAssertYamlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            yamlAssert.Run(context, tempFile);

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
    public void FileAssertYamlAssert_Run_ScalarValue_CountsAsOne_NoError()
    {
        // Arrange - sample YAML has a scalar "version" key; assert count = 1
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleYaml);
            var data = new List<FileAssertQueryData> { new() { Query = "version", Count = 1 } };
            var yamlAssert = FileAssertYamlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            yamlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the count is below the minimum.
    /// </summary>
    [TestMethod]
    public void FileAssertYamlAssert_Run_MinCount_BelowMinimum_WritesError()
    {
        // Arrange - sample YAML has 3 tools; assert min=5 (3 < 5, should fail)
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleYaml);
            var data = new List<FileAssertQueryData> { new() { Query = "tools", Min = 5 } };
            var yamlAssert = FileAssertYamlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            yamlAssert.Run(context, tempFile);

            // Assert - min violation produces an error
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the count exceeds the maximum.
    /// </summary>
    [TestMethod]
    public void FileAssertYamlAssert_Run_MaxCount_ExceedsMaximum_WritesError()
    {
        // Arrange - sample YAML has 3 tools; assert max=2 (3 > 2, should fail)
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleYaml);
            var data = new List<FileAssertQueryData> { new() { Query = "tools", Max = 2 } };
            var yamlAssert = FileAssertYamlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            yamlAssert.Run(context, tempFile);

            // Assert - max violation produces an error
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when query string is empty.
    /// </summary>
    [TestMethod]
    public void FileAssertYamlAssert_Create_EmptyQuery_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new List<FileAssertQueryData> { new() { Query = "   " } };

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => FileAssertYamlAssert.Create(data));
    }
}
