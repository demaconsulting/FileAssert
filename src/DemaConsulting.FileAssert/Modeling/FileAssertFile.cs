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
using DemaConsulting.FileAssert.Utilities;
using Microsoft.Extensions.FileSystemGlobbing;

namespace DemaConsulting.FileAssert.Modeling;

/// <summary>
///     Represents a glob file pattern with optional count constraints and file-type assertions.
/// </summary>
internal sealed class FileAssertFile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertFile"/> class.
    /// </summary>
    /// <param name="pattern">The glob pattern used to match files.</param>
    /// <param name="min">The minimum number of files that must match, or null for no lower bound.</param>
    /// <param name="max">The maximum number of files that may match, or null for no upper bound.</param>
    /// <param name="count">The exact number of files that must match, or null for no constraint.</param>
    /// <param name="minSize">The minimum file size in bytes, or null for no constraint.</param>
    /// <param name="maxSize">The maximum file size in bytes, or null for no constraint.</param>
    /// <param name="textAssert">The text assert unit, or null when no text: block is declared.</param>
    /// <param name="pdfAssert">The PDF assert unit, or null when no pdf: block is declared.</param>
    /// <param name="xmlAssert">The XML assert unit, or null when no xml: block is declared.</param>
    /// <param name="htmlAssert">The HTML assert unit, or null when no html: block is declared.</param>
    /// <param name="yamlAssert">The YAML assert unit, or null when no yaml: block is declared.</param>
    /// <param name="jsonAssert">The JSON assert unit, or null when no json: block is declared.</param>
    /// <param name="zipAssert">The zip assert unit, or null when no zip: block is declared.</param>
    private FileAssertFile(
        string pattern,
        int? min,
        int? max,
        int? count,
        long? minSize,
        long? maxSize,
        FileAssertTextAssert? textAssert,
        FileAssertPdfAssert? pdfAssert,
        FileAssertXmlAssert? xmlAssert,
        FileAssertHtmlAssert? htmlAssert,
        FileAssertYamlAssert? yamlAssert,
        FileAssertJsonAssert? jsonAssert,
        FileAssertZipAssert? zipAssert)
    {
        // Store all validated properties for use during execution
        Pattern = pattern;
        Min = min;
        Max = max;
        Count = count;
        MinSize = minSize;
        MaxSize = maxSize;
        TextAssert = textAssert;
        PdfAssert = pdfAssert;
        XmlAssert = xmlAssert;
        HtmlAssert = htmlAssert;
        YamlAssert = yamlAssert;
        JsonAssert = jsonAssert;
        ZipAssert = zipAssert;
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
    ///     Gets the exact number of files that must match the pattern, or null for no constraint.
    /// </summary>
    internal int? Count { get; }

    /// <summary>
    ///     Gets the minimum file size in bytes, or null for no constraint.
    /// </summary>
    internal long? MinSize { get; }

    /// <summary>
    ///     Gets the maximum file size in bytes, or null for no constraint.
    /// </summary>
    internal long? MaxSize { get; }

    /// <summary>
    ///     Gets the text assert unit, or null when no text: block is declared.
    /// </summary>
    internal FileAssertTextAssert? TextAssert { get; }

    /// <summary>
    ///     Gets the PDF assert unit, or null when no pdf: block is declared.
    /// </summary>
    internal FileAssertPdfAssert? PdfAssert { get; }

    /// <summary>
    ///     Gets the XML assert unit, or null when no xml: block is declared.
    /// </summary>
    internal FileAssertXmlAssert? XmlAssert { get; }

    /// <summary>
    ///     Gets the HTML assert unit, or null when no html: block is declared.
    /// </summary>
    internal FileAssertHtmlAssert? HtmlAssert { get; }

    /// <summary>
    ///     Gets the YAML assert unit, or null when no yaml: block is declared.
    /// </summary>
    internal FileAssertYamlAssert? YamlAssert { get; }

    /// <summary>
    ///     Gets the JSON assert unit, or null when no json: block is declared.
    /// </summary>
    internal FileAssertJsonAssert? JsonAssert { get; }

    /// <summary>
    ///     Gets the zip assert unit, or null when no zip: block is declared.
    /// </summary>
    internal FileAssertZipAssert? ZipAssert { get; }

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

        // Build file-type assert units from the YAML data when declared
        var textAssert = data.Text != null ? FileAssertTextAssert.Create(data.Text) : null;
        var pdfAssert = data.Pdf != null ? FileAssertPdfAssert.Create(data.Pdf) : null;
        var xmlAssert = data.Xml != null ? FileAssertXmlAssert.Create(data.Xml) : null;
        var htmlAssert = data.Html != null ? FileAssertHtmlAssert.Create(data.Html) : null;
        var yamlAssert = data.Yaml != null ? FileAssertYamlAssert.Create(data.Yaml) : null;
        var jsonAssert = data.Json != null ? FileAssertJsonAssert.Create(data.Json) : null;
        var zipAssert = data.Zip != null ? FileAssertZipAssert.Create(data.Zip) : null;

        // Return the fully constructed file assertion
        return new FileAssertFile(
            data.Pattern, data.Min, data.Max, data.Count, data.MinSize, data.MaxSize,
            textAssert, pdfAssert, xmlAssert, htmlAssert, yamlAssert, jsonAssert, zipAssert);
    }

    /// <summary>
    ///     Executes the file assertion against the provided container, reporting any violations.
    /// </summary>
    /// <remarks>
    ///     The glob pattern is matched against all entries exposed by <paramref name="container"/>
    ///     using <c>Matcher.Match(".", entries)</c>, which works uniformly for both directory-backed
    ///     and zip-backed containers. Per-file type asserters receive the scoped container and the
    ///     relative entry path so they can open the entry as a stream without needing to know
    ///     whether the content lives on disk or inside an archive.
    /// </remarks>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="container">The container in which to evaluate the glob pattern.</param>
    internal void Run(IContext context, IFileContainer container)
    {
        // Validate required parameters before performing any container operations
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(container);

        // Perform the glob match against the container entries.
        // Normalize the pattern to forward slashes so that user-supplied patterns with
        // backslash separators (common on Windows) match the normalized entry paths from GetEntries().
        var matcher = new Matcher();
        matcher.AddInclude(Pattern.Replace('\\', '/'));
        var allEntries = container.GetEntries();
        var result = matcher.Match(".", allEntries);
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

        // Enforce the exact file count constraint if specified
        if (Count.HasValue && count != Count.Value)
        {
            context.WriteError(
                $"Pattern '{Pattern}' matched {count} file(s), but expected exactly {Count.Value}");
            return;
        }

        // Skip the per-file loop entirely when no size or file-type assertions are configured,
        // avoiding unnecessary container reads for purely count-constrained patterns.
        var hasPerFileChecks = MinSize.HasValue || MaxSize.HasValue ||
                               TextAssert != null || PdfAssert != null ||
                               XmlAssert != null || HtmlAssert != null ||
                               YamlAssert != null || JsonAssert != null ||
                               ZipAssert != null;

        if (hasPerFileChecks)
        {
            foreach (var entryPath in files)
            {
                RunEntryChecks(context, container, entryPath);
            }
        }
    }

    /// <summary>
    ///     Runs size and file-type assertions for a single matched entry, reporting violations.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="container">The container that holds the entry.</param>
    /// <param name="entryPath">The relative path of the entry within the container.</param>
    private void RunEntryChecks(IContext context, IFileContainer container, string entryPath)
    {
        // Enforce size constraints when specified
        if (MinSize.HasValue || MaxSize.HasValue)
        {
            var size = container.GetEntrySize(entryPath);
            var displayPath = container.GetDisplayPath(entryPath);

            if (MinSize.HasValue && size < MinSize.Value)
            {
                context.WriteError(
                    $"File '{displayPath}' is {size} byte(s), which is less than the minimum {MinSize.Value} bytes");
            }

            if (MaxSize.HasValue && size > MaxSize.Value)
            {
                context.WriteError(
                    $"File '{displayPath}' is {size} byte(s), which exceeds the maximum {MaxSize.Value} bytes");
            }
        }

        // Delegate to each file-type assert unit when declared
        TextAssert?.Run(context, container, entryPath);
        PdfAssert?.Run(context, container, entryPath);
        XmlAssert?.Run(context, container, entryPath);
        HtmlAssert?.Run(context, container, entryPath);
        YamlAssert?.Run(context, container, entryPath);
        JsonAssert?.Run(context, container, entryPath);
        ZipAssert?.Run(context, container, entryPath);
    }
}
