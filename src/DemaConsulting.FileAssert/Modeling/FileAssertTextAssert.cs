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
///     Applies text content rules to a file by reading it as UTF-8 text.
/// </summary>
internal sealed class FileAssertTextAssert
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertTextAssert"/> class.
    /// </summary>
    /// <param name="rules">The list of content rules to apply.</param>
    private FileAssertTextAssert(IReadOnlyList<FileAssertRule> rules)
    {
        Rules = rules;
    }

    /// <summary>
    ///     Gets the list of content rules to apply to each file.
    /// </summary>
    internal IReadOnlyList<FileAssertRule> Rules { get; }

    /// <summary>
    ///     Creates a new <see cref="FileAssertTextAssert"/> from the provided rule data.
    /// </summary>
    /// <param name="data">The list of rule data objects from YAML configuration.</param>
    /// <returns>A new <see cref="FileAssertTextAssert"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    internal static FileAssertTextAssert Create(IEnumerable<FileAssertRuleData> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var rules = data.Select(FileAssertRule.Create).ToList();
        return new FileAssertTextAssert(rules.AsReadOnly());
    }

    /// <summary>
    ///     Reads the file as UTF-8 text and applies all configured rules, reporting violations.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The full path to the file to validate.</param>
    internal void Run(Context context, string fileName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fileName);

        // Read the file content as UTF-8 text for rule evaluation
        var content = File.ReadAllText(fileName, System.Text.Encoding.UTF8);

        // Apply each rule to validate the file content
        foreach (var rule in Rules)
        {
            rule.Apply(context, fileName, content);
        }
    }
}
