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
using HtmlAgilityPack;

namespace DemaConsulting.FileAssert.Modeling;

/// <summary>
///     Applies XPath query assertions to a matched HTML file using HtmlAgilityPack.
/// </summary>
internal sealed class FileAssertHtmlAssert
{
    /// <summary>
    ///     Represents a single XPath query assertion with count constraints.
    /// </summary>
    private sealed record HtmlQuery(string Query, int? Count, int? Min, int? Max);

    private readonly IReadOnlyList<HtmlQuery> _queries;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertHtmlAssert"/> class.
    /// </summary>
    /// <param name="queries">The list of XPath query assertions to apply.</param>
    private FileAssertHtmlAssert(IReadOnlyList<HtmlQuery> queries)
    {
        _queries = queries;
    }

    /// <summary>
    ///     Creates a new <see cref="FileAssertHtmlAssert"/> from the provided query data.
    /// </summary>
    /// <param name="data">The list of query data objects from YAML configuration.</param>
    /// <returns>A new <see cref="FileAssertHtmlAssert"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a query does not specify a query string.</exception>
    internal static FileAssertHtmlAssert Create(IEnumerable<FileAssertQueryData> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var queries = data.Select(d =>
        {
            if (string.IsNullOrWhiteSpace(d.Query))
            {
                throw new InvalidOperationException("HTML query assertion must specify a 'query'");
            }

            return new HtmlQuery(d.Query, d.Count, d.Min, d.Max);
        }).ToList();

        return new FileAssertHtmlAssert(queries.AsReadOnly());
    }

    /// <summary>
    ///     Parses the HTML file and evaluates all configured XPath queries, reporting violations.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The full path to the HTML file to validate.</param>
    internal void Run(Context context, string fileName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fileName);

        // Load the HTML document using HtmlAgilityPack
        var doc = new HtmlDocument();
        try
        {
            doc.Load(fileName);
        }
        catch (Exception)
        {
            context.WriteError($"File '{fileName}' could not be parsed as an HTML document");
            return;
        }

        // Evaluate each configured XPath query and apply count constraints
        foreach (var q in _queries)
        {
            var nodes = doc.DocumentNode.SelectNodes(q.Query);
            var n = nodes?.Count ?? 0;
            ApplyConstraints(context, fileName, q.Query, q.Count, q.Min, q.Max, n);
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
        Context context, string fileName, string query,
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
