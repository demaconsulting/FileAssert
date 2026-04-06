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
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.FileAssert.Modeling;

/// <summary>
///     Applies dot-notation path assertions to a matched YAML file.
/// </summary>
internal sealed class FileAssertYamlAssert
{
    /// <summary>
    ///     Represents a single dot-notation path assertion with count constraints.
    /// </summary>
    private sealed record YamlQuery(string Query, int? Count, int? Min, int? Max);

    private readonly IReadOnlyList<YamlQuery> _queries;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertYamlAssert"/> class.
    /// </summary>
    /// <param name="queries">The list of dot-notation path assertions to apply.</param>
    private FileAssertYamlAssert(IReadOnlyList<YamlQuery> queries)
    {
        _queries = queries;
    }

    /// <summary>
    ///     Creates a new <see cref="FileAssertYamlAssert"/> from the provided query data.
    /// </summary>
    /// <param name="data">The list of query data objects from YAML configuration.</param>
    /// <returns>A new <see cref="FileAssertYamlAssert"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a query does not specify a query string.</exception>
    internal static FileAssertYamlAssert Create(IEnumerable<FileAssertQueryData> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var queries = data.Select(d =>
        {
            if (string.IsNullOrWhiteSpace(d.Query))
            {
                throw new InvalidOperationException("YAML query assertion must specify a 'query'");
            }

            return new YamlQuery(d.Query, d.Count, d.Min, d.Max);
        }).ToList();

        return new FileAssertYamlAssert(queries.AsReadOnly());
    }

    /// <summary>
    ///     Parses the YAML file and evaluates all configured dot-notation path queries, reporting violations.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The full path to the YAML file to validate.</param>
    internal void Run(Context context, string fileName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fileName);

        // Attempt to parse the file as a YAML document
        var yaml = new YamlStream();
        try
        {
            using var reader = new StreamReader(fileName);
            yaml.Load(reader);
        }
        catch (Exception ex) when (ex is YamlException or IOException or UnauthorizedAccessException)
        {
            context.WriteError($"File '{fileName}' could not be parsed as a YAML document");
            return;
        }

        // Report zero matches for all queries when the document is empty
        if (yaml.Documents.Count == 0)
        {
            foreach (var q in _queries)
            {
                ApplyConstraints(context, fileName, q.Query, q.Count, q.Min, q.Max, 0);
            }

            return;
        }

        // Evaluate each query against the root of the first YAML document
        var root = yaml.Documents[0].RootNode;

        foreach (var q in _queries)
        {
            var n = CountYamlNodes(root, q.Query);
            ApplyConstraints(context, fileName, q.Query, q.Count, q.Min, q.Max, n);
        }
    }

    /// <summary>
    ///     Traverses the YAML document using dot-notation and returns the count of matching nodes.
    /// </summary>
    /// <param name="root">The root YAML node.</param>
    /// <param name="query">The dot-notation path to evaluate.</param>
    /// <returns>
    ///     The count of matching nodes: the sequence length for sequence nodes, or 1 for
    ///     scalar and mapping nodes, or 0 when the path does not exist.
    /// </returns>
    private static int CountYamlNodes(YamlNode root, string query)
    {
        var segments = query.Split('.');
        YamlNode? current = root;

        for (var i = 0; i < segments.Length; i++)
        {
            if (current is not YamlMappingNode mapping)
            {
                return 0;
            }

            if (!mapping.Children.TryGetValue(new YamlScalarNode(segments[i]), out current))
            {
                return 0;
            }

            if (i == segments.Length - 1)
            {
                // Return sequence length, or 1 for scalar/mapping nodes
                return current switch
                {
                    YamlSequenceNode seq => seq.Children.Count,
                    YamlScalarNode => 1,
                    YamlMappingNode => 1,
                    _ => 0
                };
            }
        }

        return 0;
    }

    /// <summary>
    ///     Applies count constraints to the given node count, reporting violations.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The file being validated.</param>
    /// <param name="query">The dot-notation path string.</param>
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
