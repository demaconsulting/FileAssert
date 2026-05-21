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

using DemaConsulting.FileAssert.Utilities;

namespace DemaConsulting.FileAssert.Tests.Utilities;

/// <summary>
///     Subsystem tests for the Utilities subsystem.
/// </summary>
[Collection("Sequential")]
public class UtilitiesTests
{
    /// <summary>
    ///     Verifies that the Utilities subsystem's safe path combination prevents
    ///     path traversal when used against the real file system.
    /// </summary>
    [Fact]
    public void Utilities_SafePathCombine_PreventsPathTraversalToFileSystem()
    {
        // Arrange
        using var tempDir = new TemporaryDirectory();
        // Act & Assert - a traversal attempt is rejected with ArgumentException
        Assert.Throws<ArgumentException>(
            () => PathHelpers.SafePathCombine(tempDir.DirectoryPath, "../escape.txt"));

        // Act & Assert - a valid relative path within the base is accepted
        var combined = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "nested/file.txt");
        var relativePath = Path.GetRelativePath(tempDir.DirectoryPath, combined);
        Assert.Equal(Path.Combine("nested", "file.txt"), relativePath);
    }

    /// <summary>
    ///     Verifies that the Utilities subsystem's temporary directory provides an isolated
    ///     scratch space that is created on construction, accessible during the lifetime,
    ///     and fully removed on disposal.
    /// </summary>
    [Fact]
    public void Utilities_TemporaryDirectory_IsolatesAndCleansUpScratchSpace()
    {
        // Arrange & Act: create a temp directory, write a file inside it, then dispose
        string filePath;
        using (var tempDir = new TemporaryDirectory())
        {
            filePath = tempDir.GetFilePath("scratch.txt");
            File.WriteAllText(filePath, "scratch content");

            // Assert: file is accessible within the temporary directory lifetime
            Assert.True(File.Exists(filePath),
                "Scratch file should be accessible within the temporary directory lifetime.");
        }

        // Assert: directory and its contents are removed after disposal
        Assert.False(File.Exists(filePath),
            "Scratch file should be removed after the temporary directory is disposed.");
    }
}
