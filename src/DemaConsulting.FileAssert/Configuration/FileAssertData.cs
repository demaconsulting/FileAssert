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
    ///     Gets or sets the list of content rules to apply to each matching file.
    /// </summary>
    [YamlMember(Alias = "rules")]
    public List<FileAssertRuleData>? Rules { get; set; }
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
