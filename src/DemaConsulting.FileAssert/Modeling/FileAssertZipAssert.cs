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
using DemaConsulting.FileAssert.Utilities;

namespace DemaConsulting.FileAssert.Modeling;

/// <summary>
///     Validates zip archive contents by running the full file assertion suite against each
///     matching entry. Invoked by <see cref="FileAssertFile"/> when a <c>zip:</c> assertion
///     block is declared in the YAML configuration.
/// </summary>
/// <remarks>
///     Unlike the previous implementation that only checked entry counts, this implementation
///     opens the zip entry as a <see cref="ZipFileContainer"/> and runs each
///     <see cref="FileAssertFile"/> assertion against the virtual file system exposed by the
///     archive. This enables the full assertion suite — text, XML, HTML, YAML, JSON, PDF,
///     and recursively nested zip — to be applied to zip entries without any asserter needing
///     to know whether the file lives on disk or inside an archive.
/// </remarks>
internal sealed class FileAssertZipAssert
{
    /// <summary>
    ///     The list of file assertions to apply to the zip archive contents.
    /// </summary>
    private readonly IReadOnlyList<FileAssertFile> _files;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertZipAssert"/> class.
    /// </summary>
    /// <param name="files">The list of file assertions to apply to the zip archive.</param>
    private FileAssertZipAssert(IReadOnlyList<FileAssertFile> files)
    {
        _files = files;
    }

    /// <summary>
    ///     Gets the list of file assertions applied to the zip archive.
    /// </summary>
    internal IReadOnlyList<FileAssertFile> Files => _files;

    /// <summary>
    ///     Creates a new <see cref="FileAssertZipAssert"/> from the provided YAML data.
    /// </summary>
    /// <param name="data">The zip assertion block data deserialized from YAML configuration.</param>
    /// <returns>A new <see cref="FileAssertZipAssert"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when any entry data does not specify a pattern.</exception>
    internal static FileAssertZipAssert Create(FileAssertZipData data)
    {
        // Validate that data was provided
        ArgumentNullException.ThrowIfNull(data);

        // Convert each entry DTO into a FileAssertFile domain object using the shared factory
        var files = (data.Files ?? [])
            .Select(FileAssertFile.Create)
            .ToList();

        return new FileAssertZipAssert(files.AsReadOnly());
    }

    /// <summary>
    ///     Opens the zip entry identified by <paramref name="entryPath"/> inside
    ///     <paramref name="container"/>, wraps its contents in a <see cref="ZipFileContainer"/>,
    ///     and runs all configured file assertions against it.
    /// </summary>
    /// <remarks>
    ///     Each file assertion is run with a scoped context that prepends the zip display path
    ///     to every error message, providing breadcrumb-style context for nested archives.
    ///     If the entry cannot be opened or parsed as a zip archive, a single error is written
    ///     and no further assertions are evaluated.
    /// </remarks>
    /// <param name="context">The context used for reporting errors. Must not be null.</param>
    /// <param name="container">The container from which the zip entry is opened. Must not be null.</param>
    /// <param name="entryPath">The relative path of the zip entry inside the container. Must not be null.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="context"/>, <paramref name="container"/>, or
    ///     <paramref name="entryPath"/> is null.
    /// </exception>
    internal void Run(IContext context, IFileContainer container, string entryPath)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(entryPath);

        // Compute the display path for error messages and context scoping
        var displayPath = container.GetDisplayPath(entryPath);

        // Attempt to open the entry and wrap it as a ZipFileContainer
        // The stream must be disposed on ZipArchive constructor failure to avoid file locks
        ZipFileContainer zipContainer;
        try
        {
            var stream = container.OpenEntry(entryPath);
            try
            {
                zipContainer = new ZipFileContainer(stream, displayPath);
            }
            catch
            {
                // Ensure the stream is released even if ZipFileContainer construction fails
                stream.Dispose();
                throw;
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            context.WriteError($"File '{displayPath}' could not be read as a zip archive");
            return;
        }

        // Use a scoped context so all inner errors carry the zip path as a breadcrumb prefix
        var scopedContext = context.WithPrefix(displayPath);

        using (zipContainer)
        {
            // Run each file assertion against the zip container
            foreach (var file in _files)
            {
                file.Run(scopedContext, zipContainer);
            }
        }
    }
}
