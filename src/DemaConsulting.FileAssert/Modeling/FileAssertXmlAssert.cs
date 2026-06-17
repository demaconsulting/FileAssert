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

using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DemaConsulting.FileAssert.Cli;
using DemaConsulting.FileAssert.Configuration;
using DemaConsulting.FileAssert.Utilities;

namespace DemaConsulting.FileAssert.Modeling;

/// <summary>
///     Applies XPath query assertions to a matched XML file.
/// </summary>
internal sealed class FileAssertXmlAssert
{
    /// <summary>
    ///     Represents a single XPath query assertion with count constraints.
    /// </summary>
    /// <param name="Query">XPath expression to evaluate against the document.</param>
    /// <param name="Count">Expected exact node count; <see langword="null"/> means no exact count constraint.</param>
    /// <param name="Min">Minimum node count; <see langword="null"/> means no lower bound.</param>
    /// <param name="Max">Maximum node count; <see langword="null"/> means no upper bound.</param>
    private sealed record XmlQuery(string Query, int? Count, int? Min, int? Max);

    /// <summary>
    ///     The ordered list of XPath query assertions to apply when this rule runs.
    /// </summary>
    private readonly IReadOnlyList<XmlQuery> _queries;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertXmlAssert"/> class.
    /// </summary>
    /// <param name="queries">The list of XPath query assertions to apply.</param>
    private FileAssertXmlAssert(IReadOnlyList<XmlQuery> queries)
    {
        _queries = queries;
    }

    /// <summary>
    ///     Creates a new <see cref="FileAssertXmlAssert"/> from the provided query data.
    /// </summary>
    /// <param name="data">The list of query data objects from YAML configuration.</param>
    /// <returns>A new <see cref="FileAssertXmlAssert"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a query does not specify a query string.</exception>
    internal static FileAssertXmlAssert Create(IEnumerable<FileAssertQueryData> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var queries = data.Select(d =>
        {
            if (string.IsNullOrWhiteSpace(d.Query))
            {
                throw new InvalidOperationException("XML query assertion must specify a 'query'");
            }

            return new XmlQuery(d.Query, d.Count, d.Min, d.Max);
        }).ToList();

        return new FileAssertXmlAssert(queries.AsReadOnly());
    }

    /// <summary>
    ///     Parses the XML entry and evaluates all configured XPath queries, reporting violations.
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

        // Attempt to parse the entry as an XML document
        XDocument document;
        try
        {
            using var stream = container.OpenEntry(entryPath);
            document = XDocument.Load(stream);
        }
        catch (Exception ex) when (ex is XmlException or IOException or UnauthorizedAccessException)
        {
            context.WriteError($"File '{displayPath}' could not be parsed as an XML document");
            return;
        }

        // Evaluate each configured XPath query and apply count constraints
        foreach (var q in _queries)
        {
            int n;
            try
            {
                n = document.XPathSelectElements(q.Query).Count();
            }
            catch (XPathException)
            {
                context.WriteError($"File '{displayPath}' query '{q.Query}' is not a valid XPath expression");
                continue;
            }

            ApplyConstraints(context, displayPath, q.Query, q.Count, q.Min, q.Max, n);
        }
    }

    /// <summary>
    ///     Applies count constraints to the given node count, reporting violations.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The file being validated.</param>
    /// <param name="query">The XPath query string.</param>
    /// <param name="count">The exact count constraint, or null.</param>
    /// <param name="min">The minimum count constraint, or null.</param>
    /// <param name="max">The maximum count constraint, or null.</param>
    /// <param name="n">The actual node count returned by the query.</param>
    private static void ApplyConstraints(
        IContext context, string fileName, string query,
        int? count, int? min, int? max, int n)
    {
        if (count.HasValue && n != count.Value)
        {
            context.WriteError(
                $"File '{fileName}' query '{query}' returned {n} result(s) but expected exactly {count.Value}");
            return;
        }

        if (min.HasValue && n < min.Value)
        {
            context.WriteError(
                $"File '{fileName}' query '{query}' returned {n} result(s) which is below the minimum of {min.Value}");
        }

        if (max.HasValue && n > max.Value)
        {
            context.WriteError(
                $"File '{fileName}' query '{query}' returned {n} result(s) which exceeds the maximum of {max.Value}");
        }
    }
}
