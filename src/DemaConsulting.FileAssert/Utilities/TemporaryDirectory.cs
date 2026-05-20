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

namespace DemaConsulting.FileAssert.Utilities;

/// <summary>
///     A disposable temporary directory that is automatically deleted when disposed.
/// </summary>
/// <remarks>
///     The temporary directory is created under <see cref="Environment.CurrentDirectory"/>
///     rather than <see cref="Path.GetTempPath()"/>. This avoids OS symlink issues such as
///     <c>/tmp</c> resolving to <c>/private/tmp</c> on macOS, which can cause
///     path-comparison failures when the OS returns the real (resolved) path instead
///     of the symlink path used to construct it.
/// </remarks>
internal sealed class TemporaryDirectory : IDisposable
{
    /// <summary>Gets the full path to the temporary directory.</summary>
    public string DirectoryPath { get; }

    /// <summary>
    ///     Initializes a new instance, creating a uniquely-named subdirectory under
    ///     <see cref="Environment.CurrentDirectory"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the temporary directory cannot be created due to an
    ///     <see cref="IOException"/>, <see cref="UnauthorizedAccessException"/>, or
    ///     <see cref="ArgumentException"/>.
    /// </exception>
    public TemporaryDirectory()
    {
        var effectiveBase = Environment.CurrentDirectory;
        DirectoryPath = PathHelpers.SafePathCombine(effectiveBase, $"tmp-{Guid.NewGuid():N}");

        try
        {
            Directory.CreateDirectory(DirectoryPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            throw new InvalidOperationException($"Failed to create temporary directory: {ex.Message}", ex);
        }
    }

    /// <summary>
    ///     Returns the full path to a file within the temporary directory,
    ///     creating any required intermediate subdirectories.
    /// </summary>
    /// <param name="relativePath">
    ///     A relative path within the temporary directory. Must not be null.
    /// </param>
    /// <returns>The combined full path within the temporary directory.</returns>
    /// <exception cref="ArgumentNullException">Thrown when relativePath is null.</exception>
    /// <exception cref="ArgumentException">Thrown when relativePath would escape the temporary directory.</exception>
    public string GetFilePath(string relativePath)
    {
        var path = PathHelpers.SafePathCombine(DirectoryPath, relativePath);

        var directory = Path.GetDirectoryName(path);
        if (directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        return path;
    }

    /// <summary>
    ///     Deletes the temporary directory and all its contents.
    /// </summary>
    /// <remarks>
    ///     IOException and UnauthorizedAccessException are intentionally suppressed during
    ///     disposal. Cleanup failures are non-fatal.
    /// </remarks>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Ignore cleanup errors during disposal
        }
    }
}
