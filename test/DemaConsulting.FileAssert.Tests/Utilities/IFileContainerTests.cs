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
using DemaConsulting.FileAssert.Utilities;

namespace DemaConsulting.FileAssert.Tests.Utilities;

/// <summary>
///     Unit tests for <see cref="DirectoryFileContainer"/> and <see cref="ZipFileContainer"/>.
/// </summary>
[Collection("Sequential")]
public sealed class IFileContainerTests
{
    // ---------------------------------------------------------------------------
    // DirectoryFileContainer tests
    // ---------------------------------------------------------------------------

    /// <summary>
    ///     Verifies that GetEntries returns all files recursively with forward-slash paths.
    /// </summary>
    [Fact]
    public void DirectoryFileContainer_GetEntries_ReturnsAllFilesWithForwardSlashes()
    {
        // Arrange - create a small directory tree
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("a.txt"), "a");
        var subDir = Directory.CreateDirectory(Path.Combine(tempDir.DirectoryPath, "sub"));
        File.WriteAllText(Path.Combine(subDir.FullName, "b.txt"), "b");
        using var container = new DirectoryFileContainer(tempDir.DirectoryPath);

        // Act
        var entries = container.GetEntries().ToList();

        // Assert - both files are returned with forward slashes
        Assert.Equal(2, entries.Count);
        Assert.Contains("a.txt", entries);
        Assert.Contains("sub/b.txt", entries);
    }

    /// <summary>
    ///     Verifies that GetEntries returns an empty list when the directory is empty.
    /// </summary>
    [Fact]
    public void DirectoryFileContainer_GetEntries_EmptyDirectory_ReturnsEmpty()
    {
        // Arrange - create an empty directory
        using var tempDir = new TemporaryDirectory();
        using var container = new DirectoryFileContainer(tempDir.DirectoryPath);

        // Act
        var entries = container.GetEntries().ToList();

        // Assert
        Assert.Empty(entries);
    }

    /// <summary>
    ///     Verifies that GetEntries returns an empty list when the directory does not exist.
    /// </summary>
    [Fact]
    public void DirectoryFileContainer_GetEntries_NonExistentDirectory_ReturnsEmpty()
    {
        // Arrange - a directory that does not exist
        var missingDir = Path.Combine(Path.GetTempPath(), $"no_such_dir_{Guid.NewGuid():N}");
        using var container = new DirectoryFileContainer(missingDir);

        // Act
        var entries = container.GetEntries().ToList();

        // Assert
        Assert.Empty(entries);
    }

    /// <summary>
    ///     Verifies that OpenEntry returns a readable stream for an existing file.
    /// </summary>
    [Fact]
    public void DirectoryFileContainer_OpenEntry_ExistingFile_ReturnsStream()
    {
        // Arrange - write a file with known content
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("data.txt"), "hello");
        using var container = new DirectoryFileContainer(tempDir.DirectoryPath);

        // Act
        using var stream = container.OpenEntry("data.txt");
        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();

        // Assert
        Assert.Equal("hello", text);
    }

    /// <summary>
    ///     Verifies that OpenEntry throws <see cref="IOException"/> for a non-existent entry.
    /// </summary>
    [Fact]
    public void DirectoryFileContainer_OpenEntry_NonExistentFile_ThrowsIOException()
    {
        // Arrange - empty directory; entry does not exist
        using var tempDir = new TemporaryDirectory();
        using var container = new DirectoryFileContainer(tempDir.DirectoryPath);

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => container.OpenEntry("missing.txt"));
    }

    /// <summary>
    ///     Verifies that GetEntrySize returns the byte length of an existing file.
    /// </summary>
    [Fact]
    public void DirectoryFileContainer_GetEntrySize_ReturnsCorrectSize()
    {
        // Arrange - write a file with 5 ASCII bytes
        using var tempDir = new TemporaryDirectory();
        File.WriteAllText(tempDir.GetFilePath("size.txt"), "hello");
        using var container = new DirectoryFileContainer(tempDir.DirectoryPath);

        // Act
        var size = container.GetEntrySize("size.txt");

        // Assert
        Assert.Equal(5L, size);
    }

    /// <summary>
    ///     Verifies that GetDisplayPath returns the full file-system path for a root-level entry.
    /// </summary>
    [Fact]
    public void DirectoryFileContainer_GetDisplayPath_RootEntry_ReturnsFullPath()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        using var container = new DirectoryFileContainer(tempDir.DirectoryPath);

        // Act
        var displayPath = container.GetDisplayPath("report.pdf");

        // Assert - the display path is the full file-system path, useful in error messages
        var expectedPath = Path.Combine(tempDir.DirectoryPath, "report.pdf");
        Assert.Equal(expectedPath, displayPath);
    }

    // ---------------------------------------------------------------------------
    // ZipFileContainer tests
    // ---------------------------------------------------------------------------

    /// <summary>
    ///     Creates a zip archive byte array in memory containing the specified entries.
    /// </summary>
    /// <param name="entries">Entry name/content pairs.</param>
    /// <returns>A byte array containing the zip archive.</returns>
    private static byte[] CreateZipBytes(IEnumerable<(string name, string content)> entries)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                var entry = zip.CreateEntry(name);
                using var w = new StreamWriter(entry.Open());
                w.Write(content);
            }
        }

        return ms.ToArray();
    }

    /// <summary>
    ///     Verifies that GetEntries returns entry names with forward slashes, excluding directory entries.
    /// </summary>
    [Fact]
    public void ZipFileContainer_GetEntries_ReturnsFileEntriesWithForwardSlashes()
    {
        // Arrange - build a zip in memory with two file entries
        var bytes = CreateZipBytes([("lib/a.dll", "a"), ("lib/b.dll", "b")]);
        using var stream = new MemoryStream(bytes);
        using var container = new ZipFileContainer(stream, "archive.zip");

        // Act
        var entries = container.GetEntries().ToList();

        // Assert
        Assert.Equal(2, entries.Count);
        Assert.Contains("lib/a.dll", entries);
        Assert.Contains("lib/b.dll", entries);
    }

    /// <summary>
    ///     Verifies that OpenEntry returns a readable stream for an existing zip entry.
    /// </summary>
    [Fact]
    public void ZipFileContainer_OpenEntry_ExistingEntry_ReturnsStream()
    {
        // Arrange - zip containing one entry with known content
        var bytes = CreateZipBytes([("readme.txt", "zip content")]);
        using var stream = new MemoryStream(bytes);
        using var container = new ZipFileContainer(stream, "archive.zip");

        // Act
        using var entryStream = container.OpenEntry("readme.txt");
        using var reader = new StreamReader(entryStream);
        var text = reader.ReadToEnd();

        // Assert
        Assert.Equal("zip content", text);
    }

    /// <summary>
    ///     Verifies that OpenEntry throws <see cref="IOException"/> for a missing entry.
    /// </summary>
    [Fact]
    public void ZipFileContainer_OpenEntry_NonExistentEntry_ThrowsIOException()
    {
        // Arrange - empty zip
        var bytes = CreateZipBytes([]);
        using var stream = new MemoryStream(bytes);
        using var container = new ZipFileContainer(stream, "archive.zip");

        // Act & Assert
        Assert.Throws<IOException>(() => container.OpenEntry("missing.txt"));
    }

    /// <summary>
    ///     Verifies that GetEntrySize returns the uncompressed length of a zip entry.
    /// </summary>
    [Fact]
    public void ZipFileContainer_GetEntrySize_ReturnsUncompressedLength()
    {
        // Arrange - zip containing an entry with 5 ASCII chars
        var bytes = CreateZipBytes([("data.txt", "hello")]);
        using var stream = new MemoryStream(bytes);
        using var container = new ZipFileContainer(stream, "archive.zip");

        // Act
        var size = container.GetEntrySize("data.txt");

        // Assert
        Assert.Equal(5L, size);
    }

    /// <summary>
    ///     Verifies that GetDisplayPath returns the display name prefixed path for a zip entry.
    /// </summary>
    [Fact]
    public void ZipFileContainer_GetDisplayPath_ReturnsDisplayNamePrefixedPath()
    {
        // Arrange - zip container with display name "outer.zip"
        var bytes = CreateZipBytes([("inner.txt", "x")]);
        using var stream = new MemoryStream(bytes);
        using var container = new ZipFileContainer(stream, "outer.zip");

        // Act
        var displayPath = container.GetDisplayPath("inner.txt");

        // Assert
        Assert.Equal("outer.zip > inner.txt", displayPath);
    }

    /// <summary>
    ///     Verifies that GetEntries excludes zip directory marker entries (names ending in '/').
    /// </summary>
    [Fact]
    public void ZipFileContainer_GetEntries_ExcludesDirectoryMarkers()
    {
        // Arrange - build a zip containing a directory marker entry and a file entry
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            // Directory marker entry (name ends in '/', no content)
            zip.CreateEntry("lib/");

            // Regular file entry within that directory
            var fileEntry = zip.CreateEntry("lib/a.dll");
            using var w = new StreamWriter(fileEntry.Open());
            w.Write("a");
        }

        var bytes = ms.ToArray();
        using var stream = new MemoryStream(bytes);
        using var container = new ZipFileContainer(stream, "archive.zip");

        // Act
        var entries = container.GetEntries().ToList();

        // Assert - only the file entry is returned; the directory marker is excluded
        Assert.Single(entries);
        Assert.Contains("lib/a.dll", entries);
        Assert.DoesNotContain("lib/", entries);
    }

    /// <summary>
    ///     Verifies that OpenEntry and GetEntrySize accept entry paths that use backslash
    ///     separators by normalizing them to forward slashes before lookup.
    /// </summary>
    [Fact]
    public void ZipFileContainer_BackslashEntryPath_OpensAndSizesAfterNormalization()
    {
        // Arrange - zip stores the entry with a forward-slash separator
        var bytes = CreateZipBytes([("lib/a.dll", "abc")]);
        using var stream = new MemoryStream(bytes);
        using var container = new ZipFileContainer(stream, "archive.zip");

        // Act - look the entry up using a backslash path
        using var entryStream = container.OpenEntry("lib\\a.dll");
        using var reader = new StreamReader(entryStream);
        var content = reader.ReadToEnd();
        var size = container.GetEntrySize("lib\\a.dll");

        // Assert - both APIs resolve the entry through backslash normalization
        Assert.Equal("abc", content);
        Assert.Equal(3L, size);
    }

    // ---------------------------------------------------------------------------
    // DirectoryFileContainer null-input tests
    // ---------------------------------------------------------------------------

    /// <summary>
    ///     Verifies that the constructor rejects a null base path.
    /// </summary>
    [Fact]
    public void DirectoryFileContainer_Constructor_NullBasePath_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DirectoryFileContainer(null!));
    }

    /// <summary>
    ///     Verifies that OpenEntry rejects a null entry path.
    /// </summary>
    [Fact]
    public void DirectoryFileContainer_OpenEntry_NullEntryPath_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        using var container = new DirectoryFileContainer(tempDir.DirectoryPath);
        Assert.Throws<ArgumentNullException>(() => container.OpenEntry(null!));
    }

    /// <summary>
    ///     Verifies that GetEntrySize rejects a null entry path.
    /// </summary>
    [Fact]
    public void DirectoryFileContainer_GetEntrySize_NullEntryPath_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        using var container = new DirectoryFileContainer(tempDir.DirectoryPath);
        Assert.Throws<ArgumentNullException>(() => container.GetEntrySize(null!));
    }

    /// <summary>
    ///     Verifies that GetDisplayPath rejects a null entry path.
    /// </summary>
    [Fact]
    public void DirectoryFileContainer_GetDisplayPath_NullEntryPath_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        using var container = new DirectoryFileContainer(tempDir.DirectoryPath);
        Assert.Throws<ArgumentNullException>(() => container.GetDisplayPath(null!));
    }
}
