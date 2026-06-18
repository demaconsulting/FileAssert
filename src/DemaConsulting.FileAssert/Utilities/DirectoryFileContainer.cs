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
///     Implements <see cref="IFileContainer"/> over a local file-system directory.
/// </summary>
/// <remarks>
///     DirectoryFileContainer is the top-level container used by FileAssertFile.Run when
///     evaluating assertions against a base directory. It lists all files recursively,
///     opens them via their absolute paths, and returns their on-disk sizes.
///     It implements IDisposable for symmetry with ZipFileContainer but holds no disposable
///     resources itself — Dispose is a no-op.
/// </remarks>
internal sealed class DirectoryFileContainer : IFileContainer, IDisposable
{
    /// <summary>
    ///     Gets the absolute path of the root directory.
    /// </summary>
    internal string BasePath { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DirectoryFileContainer"/> class.
    /// </summary>
    /// <param name="basePath">
    ///     The absolute path of the directory to expose as a container. Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="basePath"/> is null.</exception>
    internal DirectoryFileContainer(string basePath)
    {
        // Validate the base path before storing it for use in all subsequent operations
        ArgumentNullException.ThrowIfNull(basePath);
        BasePath = basePath;
    }

    /// <summary>
    ///     Returns all file paths under the directory, relative to <see cref="BasePath"/>
    ///     and using forward slashes as the separator.
    /// </summary>
    /// <returns>A read-only list of forward-slash-separated relative file paths.</returns>
    public IReadOnlyList<string> GetEntries()
    {
        // Return an empty list when the directory does not exist, consistent with
        // the zero-match behavior expected by glob-based count constraints
        if (!Directory.Exists(BasePath))
        {
            return Array.Empty<string>();
        }

        // Enumerate all files recursively, producing paths relative to BasePath
        // and normalizing to forward slashes for cross-platform consistency
        return Directory
            .EnumerateFiles(BasePath, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(BasePath, f).Replace('\\', '/'))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    ///     Opens the file at the specified relative path for reading.
    /// </summary>
    /// <param name="entryPath">Relative path of the file to open. Must not be null.</param>
    /// <returns>A readable <see cref="FileStream"/> positioned at the start of the file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryPath"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the file cannot be opened.</exception>
    public Stream OpenEntry(string entryPath)
    {
        // Validate the entry path before constructing the full file-system path
        ArgumentNullException.ThrowIfNull(entryPath);

        // Combine the base path with the relative entry path to get the full on-disk location
        var fullPath = Path.Combine(BasePath, entryPath);
        return File.OpenRead(fullPath);
    }

    /// <summary>
    ///     Returns the size of the file at the specified relative path in bytes.
    /// </summary>
    /// <param name="entryPath">Relative path of the file. Must not be null.</param>
    /// <returns>The file size in bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryPath"/> is null.</exception>
    public long GetEntrySize(string entryPath)
    {
        // Validate the entry path before constructing the full path
        ArgumentNullException.ThrowIfNull(entryPath);

        // Combine to get the full path and query the size via FileInfo
        var fullPath = Path.Combine(BasePath, entryPath);
        return new FileInfo(fullPath).Length;
    }

    /// <summary>
    ///     Returns the full file-system path of the specified entry for use in error messages.
    /// </summary>
    /// <param name="entryPath">Relative path of the entry. Must not be null.</param>
    /// <returns>The absolute file-system path of the entry.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryPath"/> is null.</exception>
    public string GetDisplayPath(string entryPath)
    {
        // Validate the entry path before combining
        ArgumentNullException.ThrowIfNull(entryPath);

        // Return the full path so error messages identify the exact file on disk
        return Path.Combine(BasePath, entryPath);
    }

    /// <summary>
    ///     Releases resources held by this container.
    /// </summary>
    /// <remarks>
    ///     DirectoryFileContainer holds no disposable resources; this method is a no-op
    ///     provided for symmetry with ZipFileContainer so that both can be used in
    ///     <c>using</c> statements.
    /// </remarks>
    public void Dispose()
    {
        // No disposable resources held — no-op for IDisposable symmetry with ZipFileContainer
    }
}
