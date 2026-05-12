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

using YamlDotNet.Serialization;

namespace DemaConsulting.FileAssert.Configuration;

/// <summary>
///     YAML data transfer object representing a single file assertion rule.
/// </summary>
internal sealed class FileAssertRuleData
{
    /// <summary>
    ///     Gets or sets the substring that the file content must contain.
    /// </summary>
    [YamlMember(Alias = "contains")]
    public string? Contains { get; set; }

    /// <summary>
    ///     Gets or sets the substring that the file content must NOT contain.
    /// </summary>
    [YamlMember(Alias = "does-not-contain")]
    public string? DoesNotContain { get; set; }

    /// <summary>
    ///     Gets or sets the regular expression pattern that the file content must match.
    /// </summary>
    [YamlMember(Alias = "matches")]
    public string? Matches { get; set; }

    /// <summary>
    ///     Gets or sets the regular expression pattern that the file content must NOT match.
    /// </summary>
    [YamlMember(Alias = "does-not-contain-regex")]
    public string? DoesNotContainRegex { get; set; }
}

/// <summary>
///     YAML data transfer object representing a file pattern with optional constraints and rules.
/// </summary>
internal sealed class FileAssertFileData
{
    /// <summary>
    ///     Gets or sets the glob pattern used to match files.
    /// </summary>
    [YamlMember(Alias = "pattern")]
    public string? Pattern { get; set; }

    /// <summary>
    ///     Gets or sets the minimum number of files that must match the pattern.
    /// </summary>
    [YamlMember(Alias = "min")]
    public int? Min { get; set; }

    /// <summary>
    ///     Gets or sets the maximum number of files that may match the pattern.
    /// </summary>
    [YamlMember(Alias = "max")]
    public int? Max { get; set; }

    /// <summary>
    ///     Gets or sets the exact number of files that must match the pattern.
    /// </summary>
    [YamlMember(Alias = "count")]
    public int? Count { get; set; }

    /// <summary>
    ///     Gets or sets the minimum file size in bytes.
    /// </summary>
    [YamlMember(Alias = "min-size")]
    public long? MinSize { get; set; }

    /// <summary>
    ///     Gets or sets the maximum file size in bytes.
    /// </summary>
    [YamlMember(Alias = "max-size")]
    public long? MaxSize { get; set; }

    /// <summary>
    ///     Gets or sets the list of text-content assertion rules to apply to each matching file.
    /// </summary>
    [YamlMember(Alias = "text")]
    public List<FileAssertRuleData>? Text { get; set; }

    /// <summary>
    ///     Gets or sets the PDF assertion block for this file pattern.
    /// </summary>
    [YamlMember(Alias = "pdf")]
    public FileAssertPdfData? Pdf { get; set; }

    /// <summary>
    ///     Gets or sets the list of XML XPath query assertions for this file pattern.
    /// </summary>
    [YamlMember(Alias = "xml")]
    public List<FileAssertQueryData>? Xml { get; set; }

    /// <summary>
    ///     Gets or sets the list of HTML XPath query assertions for this file pattern.
    /// </summary>
    [YamlMember(Alias = "html")]
    public List<FileAssertQueryData>? Html { get; set; }

    /// <summary>
    ///     Gets or sets the list of YAML dot-notation path assertions for this file pattern.
    /// </summary>
    [YamlMember(Alias = "yaml")]
    public List<FileAssertQueryData>? Yaml { get; set; }

    /// <summary>
    ///     Gets or sets the list of JSON dot-notation path assertions for this file pattern.
    /// </summary>
    [YamlMember(Alias = "json")]
    public List<FileAssertQueryData>? Json { get; set; }

    /// <summary>
    ///     Gets or sets the zip archive entry assertion block for this file pattern.
    /// </summary>
    [YamlMember(Alias = "zip")]
    public FileAssertZipData? Zip { get; set; }
}

/// <summary>
///     YAML data transfer object for a PDF metadata assertion rule.
/// </summary>
internal sealed class FileAssertPdfMetadataRuleData
{
    /// <summary>
    ///     Gets or sets the metadata field name to check (e.g. "Title", "Author").
    /// </summary>
    [YamlMember(Alias = "field")]
    public string? Field { get; set; }

    /// <summary>
    ///     Gets or sets the substring that the field value must contain.
    /// </summary>
    [YamlMember(Alias = "contains")]
    public string? Contains { get; set; }

    /// <summary>
    ///     Gets or sets the regular expression pattern the field value must match.
    /// </summary>
    [YamlMember(Alias = "matches")]
    public string? Matches { get; set; }
}

/// <summary>
///     YAML data transfer object for PDF page count constraints.
/// </summary>
internal sealed class FileAssertPdfPagesData
{
    /// <summary>
    ///     Gets or sets the minimum number of pages required.
    /// </summary>
    [YamlMember(Alias = "min")]
    public int? Min { get; set; }

    /// <summary>
    ///     Gets or sets the maximum number of pages allowed.
    /// </summary>
    [YamlMember(Alias = "max")]
    public int? Max { get; set; }
}

/// <summary>
///     YAML data transfer object for the PDF assertion block.
/// </summary>
internal sealed class FileAssertPdfData
{
    /// <summary>
    ///     Gets or sets the list of metadata field assertions.
    /// </summary>
    [YamlMember(Alias = "metadata")]
    public List<FileAssertPdfMetadataRuleData>? Metadata { get; set; }

    /// <summary>
    ///     Gets or sets the page count constraints.
    /// </summary>
    [YamlMember(Alias = "pages")]
    public FileAssertPdfPagesData? Pages { get; set; }

    /// <summary>
    ///     Gets or sets the list of text content rules applied to extracted PDF text.
    /// </summary>
    [YamlMember(Alias = "text")]
    public List<FileAssertRuleData>? Text { get; set; }
}

/// <summary>
///     YAML data transfer object representing a single zip archive entry pattern with count constraints.
/// </summary>
internal sealed class FileAssertZipEntryData
{
    /// <summary>
    ///     Gets or sets the glob pattern used to match zip archive entry names.
    /// </summary>
    [YamlMember(Alias = "pattern")]
    public string? Pattern { get; set; }

    /// <summary>
    ///     Gets or sets the minimum number of entries that must match the pattern.
    /// </summary>
    [YamlMember(Alias = "min")]
    public int? Min { get; set; }

    /// <summary>
    ///     Gets or sets the maximum number of entries that may match the pattern.
    /// </summary>
    [YamlMember(Alias = "max")]
    public int? Max { get; set; }
}

/// <summary>
///     YAML data transfer object for the zip archive assertion block.
/// </summary>
internal sealed class FileAssertZipData
{
    /// <summary>
    ///     Gets or sets the list of entry pattern constraints to validate against the zip archive.
    /// </summary>
    [YamlMember(Alias = "entries")]
    public List<FileAssertZipEntryData>? Entries { get; set; }
}

/// <summary>
///     YAML data transfer object for a structured-document query assertion (XML, HTML, YAML, JSON).
/// </summary>
internal sealed class FileAssertQueryData
{
    /// <summary>
    ///     Gets or sets the query string (XPath or dot-notation path).
    /// </summary>
    [YamlMember(Alias = "query")]
    public string? Query { get; set; }

    /// <summary>
    ///     Gets or sets the exact count of matching nodes expected.
    /// </summary>
    [YamlMember(Alias = "count")]
    public int? Count { get; set; }

    /// <summary>
    ///     Gets or sets the minimum number of matching nodes required.
    /// </summary>
    [YamlMember(Alias = "min")]
    public int? Min { get; set; }

    /// <summary>
    ///     Gets or sets the maximum number of matching nodes allowed.
    /// </summary>
    [YamlMember(Alias = "max")]
    public int? Max { get; set; }
}

/// <summary>
///     YAML data transfer object representing a named test containing file assertions.
/// </summary>
internal sealed class FileAssertTestData
{
    /// <summary>
    ///     Gets or sets the name of this test.
    /// </summary>
    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the optional list of tags used for test filtering.
    /// </summary>
    [YamlMember(Alias = "tags")]
    public List<string>? Tags { get; set; }

    /// <summary>
    ///     Gets or sets the list of file assertions to perform in this test.
    /// </summary>
    [YamlMember(Alias = "files")]
    public List<FileAssertFileData>? Files { get; set; }
}

/// <summary>
///     YAML data transfer object representing the top-level configuration file.
/// </summary>
internal sealed class FileAssertConfigData
{
    /// <summary>
    ///     Gets or sets the list of tests defined in this configuration.
    /// </summary>
    [YamlMember(Alias = "tests")]
    public List<FileAssertTestData>? Tests { get; set; }
}
