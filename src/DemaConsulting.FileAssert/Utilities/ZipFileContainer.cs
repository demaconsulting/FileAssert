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

namespace DemaConsulting.FileAssert.Utilities;

/// <summary>
///     Implements <see cref="IFileContainer"/> over a <see cref="ZipArchive"/>.
/// </summary>
/// <remarks>
///     ZipFileContainer is constructed by FileAssertZipAssert.Run to expose the contents
///     of a zip archive entry as a container that nested FileAssertFile instances can
///     query and open. It wraps a ZipArchive and must be disposed when file assertions
///     are complete so that the underlying stream is released.
/// </remarks>
internal sealed class ZipFileContainer : IFileContainer, IDisposable
{
    /// <summary>
    ///     The underlying zip archive opened from the provided stream.
    /// </summary>
    private readonly ZipArchive _archive;

    /// <summary>
    ///     The display name of this container used in error message breadcrumbs.
    /// </summary>
    private readonly string _displayName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ZipFileContainer"/> class from a stream.
    /// </summary>
    /// <remarks>
    ///     Opening from a stream (rather than a file path) supports the zip-in-zip scenario
    ///     where FileAssertZipAssert opens an entry stream from a parent ZipFileContainer.
    ///     The archive takes ownership of the stream and closes it on disposal.
    /// </remarks>
    /// <param name="stream">The stream containing zip archive data. Must not be null.</param>
    /// <param name="displayName">
    ///     The display name of this container, used as a breadcrumb prefix in error messages.
    ///     Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="stream"/> or <paramref name="displayName"/> is null.
    /// </exception>
    /// <exception cref="InvalidDataException">Thrown when the stream is not a valid zip archive.</exception>
    internal ZipFileContainer(Stream stream, string displayName)
    {
        // Validate required parameters before opening the archive
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(displayName);

        // Open the archive in read mode; leaveOpen: false so the stream is closed with the archive
        _archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        _displayName = displayName;
    }

    /// <summary>
    ///     Returns all file entry paths in the zip archive, normalized to forward slashes and
    ///     excluding directory markers.
    /// </summary>
    /// <returns>
    ///     A read-only list of forward-slash-separated entry paths. Directory entries (whose
    ///     names end with '/') are excluded.
    /// </returns>
    public IReadOnlyList<string> GetEntries()
    {
        // Collect file entry names, normalizing to forward slashes and excluding directory markers
        return _archive.Entries
            .Select(e => e.FullName.Replace('\\', '/'))
            .Where(name => !name.EndsWith('/'))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    ///     Opens the zip archive entry at the specified path and returns a readable stream.
    /// </summary>
    /// <param name="entryPath">
    ///     The relative path of the entry to open. Must match a value returned by
    ///     <see cref="GetEntries"/>. Must not be null.
    /// </param>
    /// <returns>A readable <see cref="Stream"/> over the entry content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryPath"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the entry cannot be found or opened.</exception>
    public Stream OpenEntry(string entryPath)
    {
        // Validate the entry path before searching the archive
        ArgumentNullException.ThrowIfNull(entryPath);

        // Normalize backslashes to forward slashes so that callers using
        // either separator can locate entries; mirrors GetEntries normalization.
        var normalized = entryPath.Replace('\\', '/');

        // Locate the entry by its normalized name within the archive
        var entry = _archive.GetEntry(normalized)
            ?? throw new IOException($"Zip entry '{entryPath}' not found in '{_displayName}'");

        return entry.Open();
    }

    /// <summary>
    ///     Returns the uncompressed size of the specified zip archive entry in bytes.
    /// </summary>
    /// <param name="entryPath">
    ///     The relative path of the entry. Must match a value returned by <see cref="GetEntries"/>.
    ///     Must not be null.
    /// </param>
    /// <returns>The uncompressed size of the entry in bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryPath"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the entry cannot be found.</exception>
    public long GetEntrySize(string entryPath)
    {
        // Validate the entry path before searching the archive
        ArgumentNullException.ThrowIfNull(entryPath);

        // Normalize backslashes to forward slashes so that callers using
        // either separator can locate entries; mirrors GetEntries normalization.
        var normalized = entryPath.Replace('\\', '/');

        // Locate the entry and return its uncompressed length
        var entry = _archive.GetEntry(normalized)
            ?? throw new IOException($"Zip entry '{entryPath}' not found in '{_displayName}'");

        return entry.Length;
    }

    /// <summary>
    ///     Returns a display path for the specified entry for use in error messages.
    /// </summary>
    /// <param name="entryPath">The relative path of the entry. Must not be null.</param>
    /// <returns>
    ///     A breadcrumb-style display path of the form <c>"{displayName} > {entryPath}"</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryPath"/> is null.</exception>
    public string GetDisplayPath(string entryPath)
    {
        // Validate the entry path before constructing the display string
        ArgumentNullException.ThrowIfNull(entryPath);

        // Prefix the entry path with the archive display name as a navigation breadcrumb
        return $"{_displayName} > {entryPath}";
    }

    /// <summary>
    ///     Disposes the underlying <see cref="ZipArchive"/> and its associated stream.
    /// </summary>
    public void Dispose()
    {
        // Dispose the archive to release the underlying stream and any unmanaged resources
        _archive.Dispose();
    }
}
