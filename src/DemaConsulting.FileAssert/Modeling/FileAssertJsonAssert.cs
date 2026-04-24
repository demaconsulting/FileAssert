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

using System.Text.Json;
using DemaConsulting.FileAssert.Cli;
using DemaConsulting.FileAssert.Configuration;

namespace DemaConsulting.FileAssert.Modeling;

/// <summary>
///     Applies dot-notation path assertions to a matched JSON file.
/// </summary>
internal sealed class FileAssertJsonAssert
{
    /// <summary>
    ///     Represents a single dot-notation path assertion with count constraints.
    /// </summary>
    /// <param name="Query">The dot-notation path to traverse within the JSON document.</param>
    /// <param name="Count">The exact element count expected, or null for no exact-count constraint.</param>
    /// <param name="Min">The minimum element count expected, or null for no lower-bound constraint.</param>
    /// <param name="Max">The maximum element count expected, or null for no upper-bound constraint.</param>
    private sealed record JsonQuery(string Query, int? Count, int? Min, int? Max);

    /// <summary>The list of configured dot-notation path assertions to evaluate against each matched JSON file.</summary>
    private readonly IReadOnlyList<JsonQuery> _queries;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertJsonAssert"/> class.
    /// </summary>
    /// <param name="queries">The list of dot-notation path assertions to apply.</param>
    private FileAssertJsonAssert(IReadOnlyList<JsonQuery> queries)
    {
        _queries = queries;
    }

    /// <summary>
    ///     Creates a new <see cref="FileAssertJsonAssert"/> from the provided query data.
    /// </summary>
    /// <param name="data">The list of query data objects from YAML configuration.</param>
    /// <returns>A new <see cref="FileAssertJsonAssert"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a query does not specify a query string.</exception>
    internal static FileAssertJsonAssert Create(IEnumerable<FileAssertQueryData> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var queries = data.Select(d =>
        {
            if (string.IsNullOrWhiteSpace(d.Query))
            {
                throw new InvalidOperationException("JSON query assertion must specify a 'query'");
            }

            return new JsonQuery(d.Query, d.Count, d.Min, d.Max);
        }).ToList();

        return new FileAssertJsonAssert(queries.AsReadOnly());
    }

    /// <summary>
    ///     Parses the JSON file and evaluates all configured dot-notation path queries, reporting violations.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The full path to the JSON file to validate.</param>
    internal void Run(Context context, string fileName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fileName);

        // Attempt to parse the file as a JSON document
        JsonDocument document;
        try
        {
            var json = File.ReadAllText(fileName);
            document = JsonDocument.Parse(json);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            context.WriteError($"File '{fileName}' could not be parsed as a JSON document");
            return;
        }

        // Evaluate each query against the document root and apply count constraints
        using (document)
        {
            foreach (var q in _queries)
            {
                var n = CountJsonNodes(document.RootElement, q.Query);
                ApplyConstraints(context, fileName, q.Query, q.Count, q.Min, q.Max, n);
            }
        }
    }

    /// <summary>
    ///     Traverses the JSON document using dot-notation and returns the count of matching elements.
    /// </summary>
    /// <param name="root">The root JSON element.</param>
    /// <param name="query">The dot-notation path to evaluate.</param>
    /// <returns>
    ///     The array length for array elements, 1 for non-array elements, or 0 when the
    ///     path does not exist or the current element is not an object.
    /// </returns>
    private static int CountJsonNodes(JsonElement root, string query)
    {
        var segments = query.Split('.');
        var current = root;

        for (var i = 0; i < segments.Length; i++)
        {
            if (current.ValueKind != JsonValueKind.Object)
            {
                return 0;
            }

            if (!current.TryGetProperty(segments[i], out var next))
            {
                return 0;
            }

            current = next;

            if (i == segments.Length - 1)
            {
                // Return array length for arrays, or 1 for any other element
                return current.ValueKind == JsonValueKind.Array
                    ? current.GetArrayLength()
                    : 1;
            }
        }

        // Path traversal completed without reaching the final segment (empty segments list)
        return 0;
    }

    /// <summary>
    ///     Applies count constraints to the given element count, reporting violations.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The file being validated.</param>
    /// <param name="query">The dot-notation path string.</param>
    /// <param name="count">The exact count constraint, or null.</param>
    /// <param name="min">The minimum count constraint, or null.</param>
    /// <param name="max">The maximum count constraint, or null.</param>
    /// <param name="n">The actual element count returned by the query.</param>
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
