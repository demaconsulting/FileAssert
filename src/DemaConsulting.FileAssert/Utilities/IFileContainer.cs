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
///     Defines a uniform file-access abstraction over a directory or a zip archive entry set.
/// </summary>
/// <remarks>
///     IFileContainer decouples the asserters (FileAssertTextAssert, FileAssertXmlAssert, etc.)
///     from the underlying storage mechanism. FileAssertFile.Run passes either a
///     DirectoryFileContainer (for top-level assertions) or a ZipFileContainer (when a
///     zip: block is declared) through the same interface so that every asserter can open,
///     size, and display entries without knowing whether the content lives on disk or inside
///     a zip archive.
/// </remarks>
internal interface IFileContainer
{
    /// <summary>
    ///     Returns the relative entry paths for all file entries in this container.
    /// </summary>
    /// <remarks>
    ///     Directory entries (e.g., zip directory markers ending with '/') are excluded.
    ///     Paths use forward slashes as the separator for cross-platform consistency.
    /// </remarks>
    /// <returns>A read-only list of relative forward-slash-separated entry paths.</returns>
    IReadOnlyList<string> GetEntries();

    /// <summary>
    ///     Opens the entry at the given path and returns a readable stream.
    /// </summary>
    /// <param name="entryPath">
    ///     The relative path of the entry to open. Must match a value returned by
    ///     <see cref="GetEntries"/>. Must not be null.
    /// </param>
    /// <returns>A <see cref="Stream"/> positioned at the start of the entry content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryPath"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the entry cannot be opened.</exception>
    Stream OpenEntry(string entryPath);

    /// <summary>
    ///     Returns the uncompressed size of the specified entry in bytes.
    /// </summary>
    /// <param name="entryPath">
    ///     The relative path of the entry to measure. Must match a value returned by
    ///     <see cref="GetEntries"/>. Must not be null.
    /// </param>
    /// <returns>The uncompressed size of the entry in bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryPath"/> is null.</exception>
    long GetEntrySize(string entryPath);

    /// <summary>
    ///     Returns a human-readable display path for use in error messages.
    /// </summary>
    /// <param name="entryPath">
    ///     The relative path of the entry. Must not be null.
    /// </param>
    /// <returns>
    ///     A string suitable for inclusion in error messages. For a directory container this is
    ///     the full file-system path; for a zip container this includes the archive name as a
    ///     breadcrumb prefix.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryPath"/> is null.</exception>
    string GetDisplayPath(string entryPath);
}
