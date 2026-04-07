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
[TestClass]
public class UtilitiesTests
{
    /// <summary>
    ///     Verifies that the Utilities subsystem's safe path combination prevents
    ///     path traversal when used against the real file system.
    /// </summary>
    [TestMethod]
    public void Utilities_SafePathCombine_PreventsPathTraversalToFileSystem()
    {
        // Arrange
        var tempDir = Directory.CreateTempSubdirectory("fileassert_util_");
        try
        {
            // Act & Assert - a traversal attempt is rejected with ArgumentException
            Assert.Throws<ArgumentException>(
                () => PathHelpers.SafePathCombine(tempDir.FullName, "../escape.txt"));

            // Act & Assert - a valid relative path within the base is accepted
            var combined = PathHelpers.SafePathCombine(tempDir.FullName, "nested/file.txt");
            Assert.IsTrue(combined.StartsWith(tempDir.FullName, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
