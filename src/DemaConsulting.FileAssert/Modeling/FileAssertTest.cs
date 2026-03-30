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

using DemaConsulting.FileAssert.Cli;
using DemaConsulting.FileAssert.Configuration;

namespace DemaConsulting.FileAssert.Modeling;

/// <summary>
///     Represents a named test containing file assertions that can be filtered by name or tag.
/// </summary>
internal sealed class FileAssertTest
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertTest"/> class.
    /// </summary>
    /// <param name="name">The name of this test.</param>
    /// <param name="tags">The tags associated with this test for filtering.</param>
    /// <param name="files">The file assertions to perform in this test.</param>
    private FileAssertTest(string name, IReadOnlyList<string> tags, IReadOnlyList<FileAssertFile> files)
    {
        // Store all validated properties for use during execution and filtering
        Name = name;
        Tags = tags;
        Files = files;
    }

    /// <summary>
    ///     Gets the name of this test.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    ///     Gets the tags associated with this test.
    /// </summary>
    internal IReadOnlyList<string> Tags { get; }

    /// <summary>
    ///     Gets the file assertions defined in this test.
    /// </summary>
    internal IReadOnlyList<FileAssertFile> Files { get; }

    /// <summary>
    ///     Creates a new <see cref="FileAssertTest"/> from the provided YAML data.
    /// </summary>
    /// <param name="data">The test data deserialized from YAML configuration.</param>
    /// <returns>A new <see cref="FileAssertTest"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the test name is not specified.</exception>
    internal static FileAssertTest Create(FileAssertTestData data)
    {
        // Validate that data was provided
        ArgumentNullException.ThrowIfNull(data);

        // Validate that a test name was specified
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            throw new InvalidOperationException("Test must specify a name");
        }

        // Build the list of file assertions from the YAML data
        var files = (data.Files ?? [])
            .Select(FileAssertFile.Create)
            .ToList();

        // Capture tags, defaulting to empty if not specified
        var tags = (data.Tags ?? []).AsReadOnly();

        // Return the fully constructed test
        return new FileAssertTest(data.Name, tags, files.AsReadOnly());
    }

    /// <summary>
    ///     Determines whether this test matches any of the provided filters.
    /// </summary>
    /// <param name="filters">
    ///     A collection of filter strings to match against the test name and tags.
    ///     An empty collection matches all tests.
    /// </param>
    /// <returns>
    ///     <c>true</c> if <paramref name="filters"/> is empty or contains the test name or any tag
    ///     (case-insensitive); otherwise <c>false</c>.
    /// </returns>
    internal bool MatchesFilter(IEnumerable<string> filters)
    {
        // Materialize to avoid multiple enumeration
        var filterList = filters.ToList();

        // An empty filter collection matches all tests
        if (filterList.Count == 0)
        {
            return true;
        }

        // Return true if any filter matches the test name or any tag (case-insensitive)
        return filterList.Any(f =>
            Name.Equals(f, StringComparison.OrdinalIgnoreCase) ||
            Tags.Any(t => t.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    ///     Executes all file assertions in this test against the specified base directory.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="basePath">The base directory path against which file patterns are evaluated.</param>
    internal void Run(Context context, string basePath)
    {
        // Validate required parameters
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(basePath);

        // Execute each file assertion in sequence
        foreach (var file in Files)
        {
            file.Run(context, basePath);
        }
    }
}
