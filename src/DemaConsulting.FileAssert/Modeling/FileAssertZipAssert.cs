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
using Microsoft.Extensions.FileSystemGlobbing;

namespace DemaConsulting.FileAssert.Modeling;

/// <summary>
///     Validates zip archive contents by matching entry names against glob patterns and enforcing
///     count constraints. Invoked by <see cref="FileAssertFile"/> when a <c>zip:</c> assertion
///     block is declared in the YAML configuration.
/// </summary>
internal sealed class FileAssertZipAssert
{
    /// <summary>
    ///     Represents a single glob-pattern entry constraint for a zip archive, carrying the
    ///     pattern and optional minimum and maximum match counts.
    /// </summary>
    internal sealed class Entry
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Entry"/> class.
        /// </summary>
        /// <param name="pattern">The glob pattern used to match zip entry names.</param>
        /// <param name="min">The minimum number of entries that must match, or null for no lower bound.</param>
        /// <param name="max">The maximum number of entries that may match, or null for no upper bound.</param>
        internal Entry(string pattern, int? min, int? max)
        {
            // Store the validated pattern and count constraints for use during zip inspection
            Pattern = pattern;
            Min = min;
            Max = max;
        }

        /// <summary>
        ///     Gets the glob pattern used to match zip entry names.
        /// </summary>
        internal string Pattern { get; }

        /// <summary>
        ///     Gets the minimum number of entries that must match the pattern, or null for no constraint.
        /// </summary>
        internal int? Min { get; }

        /// <summary>
        ///     Gets the maximum number of entries that may match the pattern, or null for no constraint.
        /// </summary>
        internal int? Max { get; }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertZipAssert"/> class.
    /// </summary>
    /// <param name="entries">The list of entry constraints to apply to the zip archive.</param>
    private FileAssertZipAssert(IReadOnlyList<Entry> entries)
    {
        Entries = entries;
    }

    /// <summary>
    ///     Gets the list of entry constraints applied to the zip archive.
    /// </summary>
    internal IReadOnlyList<Entry> Entries { get; }

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

        // Convert each entry DTO into a validated Entry domain object
        var entries = (data.Entries ?? [])
            .Select(e =>
            {
                // Require every entry to specify a glob pattern before any I/O is attempted
                if (string.IsNullOrWhiteSpace(e.Pattern))
                {
                    throw new InvalidOperationException("Zip entry assertion must specify a pattern");
                }

                return new Entry(e.Pattern, e.Min, e.Max);
            })
            .ToList();

        return new FileAssertZipAssert(entries.AsReadOnly());
    }

    /// <summary>
    ///     Opens the zip archive at <paramref name="fileName"/>, enumerates its entries, and
    ///     applies all configured entry constraints, reporting violations via the context.
    /// </summary>
    /// <remarks>
    ///     Directory entries (whose names end with <c>/</c>) are excluded from matching because
    ///     they represent containers rather than file content. Entry names are normalized to
    ///     forward slashes so that glob patterns work consistently across platforms.
    ///
    ///     If the file cannot be opened as a zip archive, a single error is written and the
    ///     method returns immediately without evaluating any entry constraints.
    /// </remarks>
    /// <param name="context">The context used for reporting errors. Must not be null.</param>
    /// <param name="fileName">The full path to the zip file to validate. Must not be null.</param>
    internal void Run(Context context, string fileName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fileName);

        // Attempt to open the zip archive; report and abort on any I/O or format error
        ZipArchive archive;
        try
        {
            archive = ZipFile.OpenRead(fileName);
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            context.WriteError($"File '{fileName}' could not be read as a zip archive");
            return;
        }

        using (archive)
        {
            // Collect all file entry names, normalizing to forward slashes and excluding directory markers
            var allEntries = archive.Entries
                .Select(e => e.FullName.Replace('\\', '/'))
                .Where(name => !name.EndsWith('/'))
                .ToList();

            // Evaluate each entry constraint against the complete list of zip file entries
            foreach (var entry in Entries)
            {
                // Use the FileSystemGlobbing Matcher with a virtual root "." so patterns are applied
                // directly to the normalized entry names without any filesystem path manipulation
                var matcher = new Matcher();
                matcher.AddInclude(entry.Pattern);
                var result = matcher.Match(".", allEntries);
                var count = result.Files.Count();

                // Enforce the minimum entry count constraint if specified
                if (entry.Min.HasValue && count < entry.Min.Value)
                {
                    context.WriteError(
                        $"Zip '{fileName}' entry pattern '{entry.Pattern}' matched {count} " +
                        $"entry(s), but expected at least {entry.Min.Value}");
                }

                // Enforce the maximum entry count constraint if specified
                if (entry.Max.HasValue && count > entry.Max.Value)
                {
                    context.WriteError(
                        $"Zip '{fileName}' entry pattern '{entry.Pattern}' matched {count} " +
                        $"entry(s), but expected at most {entry.Max.Value}");
                }
            }
        }
    }
}
