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

using System.IO.Compression;
using DemaConsulting.FileAssert.Cli;
using DemaConsulting.FileAssert.Configuration;
using DemaConsulting.FileAssert.Modeling;

namespace DemaConsulting.FileAssert.Tests.Modeling;

/// <summary>
///     Unit tests for the <see cref="FileAssertZipAssert"/> class.
/// </summary>
[Collection("Sequential")]
public sealed class FileAssertZipAssertTests
{
    /// <summary>
    ///     Creates a zip file at <paramref name="path"/> containing the specified entry names,
    ///     each with a single placeholder byte of content.
    /// </summary>
    /// <param name="path">Destination path for the zip file. Any existing file is removed first.</param>
    /// <param name="entries">Entry names to add to the zip archive.</param>
    private static void CreateZipFile(string path, IEnumerable<string> entries)
    {
        // Remove the file first because ZipFile.Open in Create mode requires a non-existent path,
        // but Path.GetTempFileName() creates a zero-byte placeholder that must be deleted first.
        File.Delete(path);

        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (var entry in entries)
        {
            var archiveEntry = archive.CreateEntry(entry);
            using var stream = archiveEntry.Open();

            // Write a single placeholder byte so the entry is not an empty-stream edge case
            stream.WriteByte(0x00);
        }
    }

    /// <summary>
    ///     Verifies that Create succeeds given valid data.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Create_ValidData_CreatesZipAssert()
    {
        // Arrange
        var data = new FileAssertZipData
        {
            Entries =
            [
                new FileAssertZipEntryData { Pattern = "lib/**/*.dll", Min = 1 }
            ]
        };

        // Act
        var zipAssert = FileAssertZipAssert.Create(data);

        // Assert
        Assert.NotNull(zipAssert);
        Assert.Single(zipAssert.Entries);
        Assert.Equal("lib/**/*.dll", zipAssert.Entries[0].Pattern);
        Assert.Equal(1, zipAssert.Entries[0].Min);
        Assert.Null(zipAssert.Entries[0].Max);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Create_NullData_ThrowsArgumentNullException()
    {
        // Act / Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertZipAssert.Create(null!));
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="InvalidOperationException"/> when an entry has
    ///     no pattern.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Create_EntryMissingPattern_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new FileAssertZipData
        {
            Entries = [new FileAssertZipEntryData { Min = 1 }]
        };

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => FileAssertZipAssert.Create(data));
    }

    /// <summary>
    ///     Verifies that Run produces no error when the zip archive contains entries that match
    ///     the pattern and satisfy the count constraints.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_MatchingEntriesMeetConstraints_NoError()
    {
        // Arrange - create a zip archive containing a matching entry
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFile(tempFile, ["lib/net8.0/MyLib.dll"]);
            var data = new FileAssertZipData
            {
                Entries =
                [
                    new FileAssertZipEntryData { Pattern = "lib/net8.0/MyLib.dll", Min = 1, Max = 1 }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            zipAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when a glob pattern matches multiple entries within
    ///     the zip archive and the count is within the declared bounds.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_GlobPatternMatchesMultipleEntries_NoError()
    {
        // Arrange - create a zip archive containing multiple dll entries under lib/
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFile(tempFile, ["lib/net8.0/MyLib.dll", "lib/net8.0/MyOther.dll"]);
            var data = new FileAssertZipData
            {
                Entries =
                [
                    new FileAssertZipEntryData { Pattern = "lib/**/*.dll", Min = 1 }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            zipAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when the number of matching entries is below
    ///     the declared minimum count.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_TooFewMatchingEntries_WritesError()
    {
        // Arrange - create an empty zip archive; the min constraint will be violated
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFile(tempFile, []);
            var data = new FileAssertZipData
            {
                Entries =
                [
                    new FileAssertZipEntryData { Pattern = "lib/**/*.dll", Min = 1 }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            zipAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when the number of matching entries exceeds
    ///     the declared maximum count.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_TooManyMatchingEntries_WritesError()
    {
        // Arrange - create a zip archive with two dll entries; max is set to 1
        var tempFile = Path.GetTempFileName();
        try
        {
            CreateZipFile(tempFile, ["lib/net8.0/MyLib.dll", "lib/net8.0/MyOther.dll"]);
            var data = new FileAssertZipData
            {
                Entries =
                [
                    new FileAssertZipEntryData { Pattern = "lib/**/*.dll", Max = 1 }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            zipAssert.Run(context, tempFile);

            // Assert
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when the file is not a valid zip archive.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_InvalidZipFile_WritesError()
    {
        // Arrange - write arbitrary bytes that are not a valid zip archive
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, [0x00, 0x01, 0x02, 0x03]);
            var data = new FileAssertZipData
            {
                Entries =
                [
                    new FileAssertZipEntryData { Pattern = "**/*.dll", Min = 1 }
                ]
            };
            var zipAssert = FileAssertZipAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            zipAssert.Run(context, tempFile);

            // Assert - a single parse error should be reported
            Assert.Equal(1, context.ExitCode);
            Assert.Equal(1, context.ErrorCount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run writes an error when the zip file path does not exist.
    /// </summary>
    [Fact]
    public void FileAssertZipAssert_Run_NonExistentFile_WritesError()
    {
        // Arrange - use a path guaranteed not to exist
        var missingFile = Path.Combine(Path.GetTempPath(), $"does_not_exist_{Guid.NewGuid():N}.zip");
        var data = new FileAssertZipData
        {
            Entries =
            [
                new FileAssertZipEntryData { Pattern = "**/*.dll", Min = 1 }
            ]
        };
        var zipAssert = FileAssertZipAssert.Create(data);
        using var context = Context.Create(["--silent"]);

        // Act
        zipAssert.Run(context, missingFile);

        // Assert - a single I/O error should be reported
        Assert.Equal(1, context.ExitCode);
        Assert.Equal(1, context.ErrorCount);
    }
}
