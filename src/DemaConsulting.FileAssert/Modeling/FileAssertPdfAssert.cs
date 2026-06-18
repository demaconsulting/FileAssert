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

using System.Text;
using System.Text.RegularExpressions;
using DemaConsulting.FileAssert.Cli;
using DemaConsulting.FileAssert.Configuration;
using DemaConsulting.FileAssert.Utilities;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace DemaConsulting.FileAssert.Modeling;

/// <summary>
///     Applies PDF-specific assertions (metadata, page count, and body text) to a matched file.
/// </summary>
internal sealed class FileAssertPdfAssert
{
    /// <summary>
    ///     Represents a single PDF metadata field assertion.
    /// </summary>
    private sealed class PdfMetadataRule
    {
        /// <summary>The compiled regex for the matches constraint, or null if no matches check.</summary>
        private readonly Regex? _matchesRegex;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PdfMetadataRule"/> class.
        /// </summary>
        /// <param name="field">The metadata field name.</param>
        /// <param name="contains">The required substring, or null if no contains check.</param>
        /// <param name="matches">The required regex pattern, or null if no matches check.</param>
        private PdfMetadataRule(string field, string? contains, string? matches)
        {
            Field = field;
            Contains = contains;
            Matches = matches;
            _matchesRegex = matches != null
                ? new Regex(matches, RegexOptions.Compiled, TimeSpan.FromSeconds(10))
                : null;
        }

        /// <summary>Gets the metadata field name.</summary>
        internal string Field { get; }

        /// <summary>Gets the required substring, or null if no contains check.</summary>
        internal string? Contains { get; }

        /// <summary>Gets the required regex pattern, or null if no matches check.</summary>
        internal string? Matches { get; }

        /// <summary>
        ///     Creates a <see cref="PdfMetadataRule"/> from the provided data.
        /// </summary>
        /// <param name="data">The rule data from YAML configuration.</param>
        /// <returns>A new <see cref="PdfMetadataRule"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the field name is missing, or when neither contains nor matches is specified.
        /// </exception>
        internal static PdfMetadataRule FromData(FileAssertPdfMetadataRuleData data)
        {
            ArgumentNullException.ThrowIfNull(data);
            if (string.IsNullOrWhiteSpace(data.Field))
            {
                throw new InvalidOperationException("PDF metadata rule must specify a 'field'");
            }

            if (data.Contains == null && data.Matches == null)
            {
                throw new InvalidOperationException(
                    $"PDF metadata rule for field '{data.Field}' must specify at least one of 'contains' or 'matches'");
            }

            return new PdfMetadataRule(data.Field, data.Contains, data.Matches);
        }

        /// <summary>
        ///     Applies the rule to the given metadata field value, reporting violations.
        /// </summary>
        /// <param name="context">The context used for reporting errors.</param>
        /// <param name="fileName">The file being validated.</param>
        /// <param name="value">The metadata field value, or null if not present.</param>
        internal void Apply(IContext context, string fileName, string? value)
        {
            ArgumentNullException.ThrowIfNull(context);

            // Check contains constraint when specified
            if (Contains != null && (value == null || !value.Contains(Contains, StringComparison.Ordinal)))
            {
                context.WriteError(
                    $"File '{fileName}' PDF metadata '{Field}' does not contain '{Contains}'");
            }

            // Check matches constraint using pre-compiled regex when specified
            if (_matchesRegex != null && (value == null || !_matchesRegex.IsMatch(value)))
            {
                context.WriteError(
                    $"File '{fileName}' PDF metadata '{Field}' does not match '{Matches}'");
            }
        }
    }

    /// <summary>
    ///     Represents PDF page count constraints.
    /// </summary>
    private sealed class PdfPages
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PdfPages"/> class.
        /// </summary>
        /// <param name="min">The minimum page count, or null for no lower bound.</param>
        /// <param name="max">The maximum page count, or null for no upper bound.</param>
        private PdfPages(int? min, int? max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>Gets the minimum page count, or null for no lower bound.</summary>
        internal int? Min { get; }

        /// <summary>Gets the maximum page count, or null for no upper bound.</summary>
        internal int? Max { get; }

        /// <summary>
        ///     Creates a <see cref="PdfPages"/> from the provided data.
        /// </summary>
        /// <param name="data">The pages data from YAML configuration.</param>
        /// <returns>A new <see cref="PdfPages"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        internal static PdfPages FromData(FileAssertPdfPagesData data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return new PdfPages(data.Min, data.Max);
        }

        /// <summary>
        ///     Applies page count constraints, reporting violations.
        /// </summary>
        /// <param name="context">The context used for reporting errors.</param>
        /// <param name="fileName">The file being validated.</param>
        /// <param name="n">The actual page count.</param>
        internal void Apply(IContext context, string fileName, int n)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (Min.HasValue && n < Min.Value)
            {
                context.WriteError(
                    $"File '{fileName}' PDF has {n} page(s) which is below the minimum of {Min.Value}");
            }

            if (Max.HasValue && n > Max.Value)
            {
                context.WriteError(
                    $"File '{fileName}' PDF has {n} page(s) which exceeds the maximum of {Max.Value}");
            }
        }
    }

    /// <summary>Metadata field assertion rules applied to the PDF document information dictionary.</summary>
    private readonly IReadOnlyList<PdfMetadataRule> _metadata;

    /// <summary>Page count constraints applied to the parsed PDF, or null when no page constraints are configured.</summary>
    private readonly PdfPages? _pages;

    /// <summary>Body text content rules applied to the concatenated text extracted from all PDF pages.</summary>
    private readonly IReadOnlyList<FileAssertRule> _text;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertPdfAssert"/> class.
    /// </summary>
    /// <param name="metadata">The list of metadata rules to apply.</param>
    /// <param name="pages">The page count constraints, or null for none.</param>
    /// <param name="text">The list of body text rules to apply.</param>
    private FileAssertPdfAssert(
        IReadOnlyList<PdfMetadataRule> metadata,
        PdfPages? pages,
        IReadOnlyList<FileAssertRule> text)
    {
        _metadata = metadata;
        _pages = pages;
        _text = text;
    }

    /// <summary>
    ///     Creates a new <see cref="FileAssertPdfAssert"/> from the provided PDF data.
    /// </summary>
    /// <param name="data">The PDF data deserialized from YAML configuration.</param>
    /// <returns>A new <see cref="FileAssertPdfAssert"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when a metadata rule does not specify a field name or constraint.
    /// </exception>
    internal static FileAssertPdfAssert Create(FileAssertPdfData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        // Build metadata rules from configuration data
        var metadata = (data.Metadata ?? [])
            .Select(PdfMetadataRule.FromData)
            .ToList();

        // Build page count constraints when declared
        var pages = data.Pages != null ? PdfPages.FromData(data.Pages) : null;

        // Build body text rules from configuration data
        var text = (data.Text ?? [])
            .Select(FileAssertRule.Create)
            .ToList();

        return new FileAssertPdfAssert(metadata.AsReadOnly(), pages, text.AsReadOnly());
    }

    /// <summary>
    ///     Opens the PDF entry and applies all configured assertions, reporting violations.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="container">The container from which the entry is opened.</param>
    /// <param name="entryPath">The relative path of the entry to validate.</param>
    internal void Run(IContext context, IFileContainer container, string entryPath)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(entryPath);

        // Compute the display path once for use in error messages
        var displayPath = container.GetDisplayPath(entryPath);

        // Attempt to open the entry as a PDF document
        // PdfPig does not support opening from a Stream directly, so bytes are read first
        PdfDocument document;
        try
        {
            using var stream = container.OpenEntry(entryPath);
            var bytes = ReadAllBytes(stream);
            document = PdfDocument.Open(bytes);
        }
        catch (IOException ex)
        {
            context.WriteError($"File '{displayPath}' could not be read: {ex.Message}");
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            context.WriteError($"File '{displayPath}' could not be read: {ex.Message}");
            return;
        }
        catch (Exception)
        {
            // Fallback: PdfPig surfaces a wide variety of exception types for malformed
            // PDF input (PdfDocumentFormatException, InvalidOperationException,
            // ArgumentException, etc.). Treat any unrecognized parse exception as an
            // invalid PDF so behavior degrades gracefully.
            context.WriteError($"File '{displayPath}' could not be parsed as a PDF document");
            return;
        }

        using (document)
        {
            RunDocumentAssertions(context, displayPath, document);
        }
    }

    /// <summary>
    ///     Applies all configured metadata, page-count, and text assertions to an already-opened
    ///     PDF document, reporting violations via <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="displayPath">The display path of the file, used in error messages.</param>
    /// <param name="document">The opened PDF document to assert against.</param>
    private void RunDocumentAssertions(IContext context, string displayPath, PdfDocument document)
    {
        // Apply metadata assertions to the document information
        foreach (var rule in _metadata)
        {
            var value = GetMetadataField(document, rule.Field);
            rule.Apply(context, displayPath, value);
        }

        // Skip page and text checks when no such constraints are configured
        if (_pages == null && _text.Count == 0)
        {
            return;
        }

        // When text rules are present, materialize pages once for both count and content.
        // When only a page-count constraint is configured, enumerate without allocating Page objects.
        if (_text.Count > 0)
        {
            var pageList = document.GetPages().ToList();
            _pages?.Apply(context, displayPath, pageList.Count);
            var content = BuildPageText(pageList);
            foreach (var rule in _text)
            {
                rule.Apply(context, displayPath, content);
            }
        }
        else
        {
            _pages?.Apply(context, displayPath, document.GetPages().Count());
        }
    }

    /// <summary>
    ///     Concatenates the text from all pages into a single string, separating pages with newlines
    ///     so that text rules do not see words from adjacent pages merged together.
    /// </summary>
    /// <param name="pages">The ordered list of pages from the PDF document.</param>
    /// <returns>A single string containing all page text joined with newline separators.</returns>
    private static string BuildPageText(IReadOnlyList<Page> pages)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < pages.Count; i++)
        {
            if (i > 0)
            {
                sb.Append('\n');
            }

            sb.Append(pages[i].Text);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Reads all bytes from a stream into a byte array.
    /// </summary>
    /// <remarks>
    ///     PdfPig does not expose a Stream-based Open overload, so bytes are buffered
    ///     first. This helper is used to read the entry contents from any IFileContainer.
    /// </remarks>
    /// <param name="stream">The stream to read.</param>
    /// <returns>A byte array containing the full stream contents.</returns>
    private static byte[] ReadAllBytes(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    /// <summary>
    ///     Retrieves a named metadata field value from the PDF document.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="field">The field name (Title, Author, Subject, Keywords, Creator, Producer).</param>
    /// <returns>The field value, or null if the field is unknown or not set.</returns>
    private static string? GetMetadataField(PdfDocument document, string field)
    {
        return field switch
        {
            "Title" => document.Information.Title,
            "Author" => document.Information.Author,
            "Subject" => document.Information.Subject,
            "Keywords" => document.Information.Keywords,
            "Creator" => document.Information.Creator,
            "Producer" => document.Information.Producer,
            _ => null
        };
    }
}
