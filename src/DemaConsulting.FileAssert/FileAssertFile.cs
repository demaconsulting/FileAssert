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

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace DemaConsulting.FileAssert;

/// <summary>
///     Represents a glob file pattern with optional count constraints and content rules.
/// </summary>
internal sealed class FileAssertFile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertFile"/> class.
    /// </summary>
    /// <param name="pattern">The glob pattern used to match files.</param>
    /// <param name="min">The minimum number of files that must match, or null for no lower bound.</param>
    /// <param name="max">The maximum number of files that may match, or null for no upper bound.</param>
    /// <param name="rules">The content rules to apply to each matching file.</param>
    private FileAssertFile(string pattern, int? min, int? max, IReadOnlyList<FileAssertRule> rules)
    {
        // Store all validated properties for use during execution
        Pattern = pattern;
        Min = min;
        Max = max;
        Rules = rules;
    }

    /// <summary>
    ///     Gets the glob pattern used to match files.
    /// </summary>
    internal string Pattern { get; }

    /// <summary>
    ///     Gets the minimum number of files that must match the pattern, or null for no constraint.
    /// </summary>
    internal int? Min { get; }

    /// <summary>
    ///     Gets the maximum number of files that may match the pattern, or null for no constraint.
    /// </summary>
    internal int? Max { get; }

    /// <summary>
    ///     Gets the list of content validation rules to apply to each matching file.
    /// </summary>
    internal IReadOnlyList<FileAssertRule> Rules { get; }

    /// <summary>
    ///     Creates a new <see cref="FileAssertFile"/> from the provided YAML data.
    /// </summary>
    /// <param name="data">The file data deserialized from YAML configuration.</param>
    /// <returns>A new <see cref="FileAssertFile"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pattern is not specified.</exception>
    internal static FileAssertFile Create(FileAssertFileData data)
    {
        // Validate that data was provided
        ArgumentNullException.ThrowIfNull(data);

        // Validate that a glob pattern was specified
        if (string.IsNullOrWhiteSpace(data.Pattern))
        {
            throw new InvalidOperationException("File assertion must specify a pattern");
        }

        // Build the list of content rules from the YAML data
        var rules = (data.Rules ?? [])
            .Select(FileAssertRule.Create)
            .ToList();

        // Return the fully constructed file assertion
        return new FileAssertFile(data.Pattern, data.Min, data.Max, rules.AsReadOnly());
    }

    /// <summary>
    ///     Executes the file assertion against the specified base directory, reporting any violations.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="basePath">The directory path in which to evaluate the glob pattern.</param>
    internal void Run(Context context, string basePath)
    {
        // Validate required parameters before performing any file system operations
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(basePath);

        // Perform the glob match to discover files matching the pattern
        var matcher = new Matcher();
        matcher.AddInclude(Pattern);
        var result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(basePath)));
        var files = result.Files.Select(f => f.Path).ToList();
        var count = files.Count;

        // Enforce the minimum file count constraint if specified
        if (Min.HasValue && count < Min.Value)
        {
            context.WriteError(
                $"Pattern '{Pattern}' matched {count} file(s), but expected at least {Min.Value}");
            return;
        }

        // Enforce the maximum file count constraint if specified
        if (Max.HasValue && count > Max.Value)
        {
            context.WriteError(
                $"Pattern '{Pattern}' matched {count} file(s), but expected at most {Max.Value}");
            return;
        }

        // Apply content rules to each matching file when rules are defined
        if (Rules.Count > 0)
        {
            foreach (var file in files)
            {
                // Read the full text of the file for content validation
                var content = File.ReadAllText(Path.Combine(basePath, file));

                // Apply each rule to validate the file content
                foreach (var rule in Rules)
                {
                    rule.Apply(context, file, content);
                }
            }
        }
    }
}
