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

using System.Collections.ObjectModel;
using DemaConsulting.FileAssert.Cli;
using DemaConsulting.FileAssert.Configuration;
using DemaConsulting.FileAssert.Modeling;
using DemaConsulting.FileAssert.Utilities;

namespace DemaConsulting.FileAssert.Tests.Modeling;

/// <summary>
///     Unit tests for the <see cref="FileAssertYamlAssert"/> class.
/// </summary>
[Collection("Sequential")]
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
    [Fact]
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
        Assert.NotNull(yamlAssert);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [Fact]
    public void FileAssertYamlAssert_Create_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertYamlAssert.Create(null!));
    }

    /// <summary>
    ///     Verifies that Run reports an error when the file cannot be parsed as YAML.
    /// </summary>
    [Fact]
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

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            yamlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the sequence count matches exactly.
    /// </summary>
    [Fact]
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

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            yamlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the sequence count does not match exactly.
    /// </summary>
    [Fact]
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

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            yamlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the sequence count is within min/max bounds.
    /// </summary>
    [Fact]
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

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            yamlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that a scalar value counts as 1.
    /// </summary>
    [Fact]
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

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            yamlAssert.Run(context, container, fileName);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the count is below the minimum.
    /// </summary>
    [Fact]
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

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            yamlAssert.Run(context, container, fileName);

            // Assert - min violation produces an error
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the count exceeds the maximum.
    /// </summary>
    [Fact]
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

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            yamlAssert.Run(context, container, fileName);

            // Assert - max violation produces an error
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when query string is empty.
    /// </summary>
    [Fact]
    public void FileAssertYamlAssert_Create_EmptyQuery_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new List<FileAssertQueryData> { new() { Query = "   " } };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => FileAssertYamlAssert.Create(data));
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when query has a trailing dot.
    /// </summary>
    [Fact]
    public void FileAssertYamlAssert_Create_TrailingDotQuery_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new List<FileAssertQueryData> { new() { Query = "tools." } };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => FileAssertYamlAssert.Create(data));
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when query has a leading dot.
    /// </summary>
    [Fact]
    public void FileAssertYamlAssert_Create_LeadingDotQuery_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new List<FileAssertQueryData> { new() { Query = ".tools" } };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => FileAssertYamlAssert.Create(data));
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when query has consecutive dots.
    /// </summary>
    [Fact]
    public void FileAssertYamlAssert_Create_ConsecutiveDotsQuery_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new List<FileAssertQueryData> { new() { Query = "a..b" } };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => FileAssertYamlAssert.Create(data));
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when the query list is empty.
    /// </summary>
    [Fact]
    public void FileAssertYamlAssert_Create_EmptyQueryList_ThrowsInvalidOperationException()
    {
        // Arrange - no queries declared at all
        var data = new List<FileAssertQueryData>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => FileAssertYamlAssert.Create(data));
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when a query specifies
    ///     none of count, min, or max.
    /// </summary>
    [Fact]
    public void FileAssertYamlAssert_Create_QueryWithoutConstraint_ThrowsInvalidOperationException()
    {
        // Arrange - a valid path but no count/min/max constraint
        var data = new List<FileAssertQueryData> { new() { Query = "tools" } };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => FileAssertYamlAssert.Create(data));
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when a query's min
    ///     constraint exceeds its max constraint.
    /// </summary>
    [Fact]
    public void FileAssertYamlAssert_Create_QueryMinGreaterThanMax_ThrowsInvalidOperationException()
    {
        // Arrange - min is greater than max
        var data = new List<FileAssertQueryData> { new() { Query = "tools", Min = 5, Max = 2 } };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => FileAssertYamlAssert.Create(data));
    }

    /// <summary>
    ///     Verifies that Run reports zero matches for all queries when the YAML file has no documents.
    /// </summary>
    [Fact]
    public void FileAssertYamlAssert_Run_EmptyDocument_ReportsZeroCount()
    {
        // Arrange - write an empty file; YamlStream.Load produces no documents
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, string.Empty);
            var data = new List<FileAssertQueryData> { new() { Query = "tools", Min = 1 } };
            var yamlAssert = FileAssertYamlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            yamlAssert.Run(context, container, fileName);

            // Assert - no documents means 0 matches; min=1 constraint is violated
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that when the YAML file cannot be parsed, only the parse error is reported
    ///     and the remaining configured query assertions are skipped.
    /// </summary>
    [Fact]
    public void FileAssertYamlAssert_Run_InvalidFile_RemainingAssertionsSkipped()
    {
        // Arrange - malformed YAML with two configured queries
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "key: [unclosed");
            var data = new List<FileAssertQueryData>
            {
                new() { Query = "tools", Count = 3 },
                new() { Query = "version", Count = 1 }
            };
            var yamlAssert = FileAssertYamlAssert.Create(data);
            var context = new CapturingContext();

            var dir = Path.GetDirectoryName(tempFile)!;
            var fileName = Path.GetFileName(tempFile)!;
            using var container = new DirectoryFileContainer(dir);

            // Act
            yamlAssert.Run(context, container, fileName);

            // Assert - exactly one error (the parse failure); the queries are not evaluated
            Assert.Single(context.Errors);
            Assert.Contains("could not be parsed", context.Errors[0]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Captures error messages written via <see cref="WriteError"/> for assertion in tests.
    /// </summary>
    private sealed class CapturingContext : IContext
    {
        private readonly List<string> _errors = [];

        /// <summary>Gets all error messages captured since this context was created.</summary>
        public ReadOnlyCollection<string> Errors => _errors.AsReadOnly();

        /// <inheritdoc/>
        public void WriteLine(string message) { }

        /// <inheritdoc/>
        public void WriteError(string message) => _errors.Add(message);

        /// <inheritdoc/>
        public IContext WithPrefix(string prefix) => this;
    }
}
