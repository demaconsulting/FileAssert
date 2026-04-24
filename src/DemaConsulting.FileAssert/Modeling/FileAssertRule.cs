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

using System.Text.RegularExpressions;
using DemaConsulting.FileAssert.Cli;
using DemaConsulting.FileAssert.Configuration;

namespace DemaConsulting.FileAssert.Modeling;

/// <summary>
///     Shared regex evaluation timeout to guard against catastrophic backtracking on adversarial inputs.
/// </summary>
internal static class RegexTimeout
{
    /// <summary>Regex evaluation will throw <see cref="RegexMatchTimeoutException"/> if it exceeds this duration.</summary>
    internal static readonly TimeSpan Default = TimeSpan.FromSeconds(10);
}

/// <summary>
///     Abstract base class representing a content validation rule applied to file content.
/// </summary>
internal abstract class FileAssertRule
{
    /// <summary>
    ///     Creates a concrete <see cref="FileAssertRule"/> from the provided data.
    /// </summary>
    /// <param name="data">The rule data deserialized from YAML configuration.</param>
    /// <returns>
    ///     A <see cref="FileAssertContainsRule"/>, <see cref="FileAssertDoesNotContainRule"/>,
    ///     <see cref="FileAssertMatchesRule"/>, or <see cref="FileAssertDoesNotMatchRule"/> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the rule data does not specify a rule type.</exception>
    internal static FileAssertRule Create(FileAssertRuleData data)
    {
        // Validate input to prevent null reference errors downstream
        ArgumentNullException.ThrowIfNull(data);

        // Create a contains rule when the 'contains' field is specified
        if (data.Contains != null)
        {
            return new FileAssertContainsRule(data.Contains);
        }

        // Create a does-not-contain rule when the 'does-not-contain' field is specified
        if (data.DoesNotContain != null)
        {
            return new FileAssertDoesNotContainRule(data.DoesNotContain);
        }

        // Create a regex match rule when the 'matches' field is specified
        if (data.Matches != null)
        {
            return new FileAssertMatchesRule(data.Matches);
        }

        // Create a does-not-match rule when the 'does-not-contain-regex' field is specified
        if (data.DoesNotContainRegex != null)
        {
            return new FileAssertDoesNotMatchRule(data.DoesNotContainRegex);
        }

        // No field was specified - this is a configuration error
        throw new InvalidOperationException(
            "Rule must specify 'contains', 'does-not-contain', 'matches', or 'does-not-contain-regex'");
    }

    /// <summary>
    ///     Applies this rule to the specified file content, writing errors to the context if the rule fails.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The name of the file being validated, used in error messages.</param>
    /// <param name="content">The full text content of the file to validate.</param>
    internal abstract void Apply(Context context, string fileName, string content);
}

/// <summary>
///     A file content rule that checks whether the content contains a specified substring.
/// </summary>
internal sealed class FileAssertContainsRule : FileAssertRule
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertContainsRule"/> class.
    /// </summary>
    /// <param name="value">The substring that must be present in the file content.</param>
    internal FileAssertContainsRule(string value)
    {
        // Store the required substring for use during content validation
        Value = value;
    }

    /// <summary>
    ///     Gets the substring that the file content must contain.
    /// </summary>
    internal string Value { get; }

    /// <summary>
    ///     Applies the contains rule, writing an error if the content does not include the required substring.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The name of the file being validated.</param>
    /// <param name="content">The full text content of the file to validate.</param>
    internal override void Apply(Context context, string fileName, string content)
    {
        // Validate that we have a context to report errors to
        ArgumentNullException.ThrowIfNull(context);

        // Report an error if the content does not contain the expected substring
        if (!content.Contains(Value, StringComparison.Ordinal))
        {
            context.WriteError($"File '{fileName}' does not contain expected text '{Value}'");
        }
    }
}

/// <summary>
///     A file content rule that checks whether the content matches a specified regular expression.
/// </summary>
internal sealed class FileAssertMatchesRule : FileAssertRule
{
    /// <summary>
    ///     Compiled regular expression used for efficient repeated matching.
    /// </summary>
    private readonly Regex _regex;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertMatchesRule"/> class.
    /// </summary>
    /// <param name="pattern">The regular expression pattern that the file content must match.</param>
    internal FileAssertMatchesRule(string pattern)
    {
        // Store the pattern string for use in error messages
        Pattern = pattern;

        // Compile the regex with a timeout to guard against catastrophic backtracking
        _regex = new Regex(pattern, RegexOptions.Compiled, RegexTimeout.Default);
    }

    /// <summary>
    ///     Gets the regular expression pattern that the file content must match.
    /// </summary>
    internal string Pattern { get; }

    /// <summary>
    ///     Applies the regex rule, writing an error if the content does not match the pattern.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The name of the file being validated.</param>
    /// <param name="content">The full text content of the file to validate.</param>
    internal override void Apply(Context context, string fileName, string content)
    {
        // Validate that we have a context to report errors to
        ArgumentNullException.ThrowIfNull(context);

        // Report an error if the content does not match the regular expression pattern
        if (!_regex.IsMatch(content))
        {
            context.WriteError($"File '{fileName}' does not match pattern '{Pattern}'");
        }
    }
}

/// <summary>
///     A file content rule that checks whether the content does NOT contain a specified substring.
/// </summary>
internal sealed class FileAssertDoesNotContainRule : FileAssertRule
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertDoesNotContainRule"/> class.
    /// </summary>
    /// <param name="value">The substring that must NOT be present in the file content.</param>
    internal FileAssertDoesNotContainRule(string value)
    {
        // Store the forbidden substring for use during content validation
        Value = value;
    }

    /// <summary>
    ///     Gets the substring that the file content must NOT contain.
    /// </summary>
    internal string Value { get; }

    /// <summary>
    ///     Applies the does-not-contain rule, writing an error if the content includes the forbidden substring.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The name of the file being validated.</param>
    /// <param name="content">The full text content of the file to validate.</param>
    internal override void Apply(Context context, string fileName, string content)
    {
        // Validate that we have a context to report errors to
        ArgumentNullException.ThrowIfNull(context);

        // Report an error if the content contains the forbidden substring
        if (content.Contains(Value, StringComparison.Ordinal))
        {
            context.WriteError($"File '{fileName}' contains forbidden text '{Value}'");
        }
    }
}

/// <summary>
///     A file content rule that checks whether the content does NOT match a specified regular expression.
/// </summary>
internal sealed class FileAssertDoesNotMatchRule : FileAssertRule
{
    /// <summary>
    ///     Compiled regular expression used for efficient repeated matching.
    /// </summary>
    private readonly Regex _regex;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileAssertDoesNotMatchRule"/> class.
    /// </summary>
    /// <param name="pattern">The regular expression pattern that the file content must NOT match.</param>
    internal FileAssertDoesNotMatchRule(string pattern)
    {
        // Store the pattern string for use in error messages
        Pattern = pattern;

        // Compile the regex with a timeout to guard against catastrophic backtracking
        _regex = new Regex(pattern, RegexOptions.Compiled, RegexTimeout.Default);
    }

    /// <summary>
    ///     Gets the regular expression pattern that the file content must NOT match.
    /// </summary>
    internal string Pattern { get; }

    /// <summary>
    ///     Applies the does-not-match rule, writing an error if the content matches the forbidden pattern.
    /// </summary>
    /// <param name="context">The context used for reporting errors.</param>
    /// <param name="fileName">The name of the file being validated.</param>
    /// <param name="content">The full text content of the file to validate.</param>
    internal override void Apply(Context context, string fileName, string content)
    {
        // Validate that we have a context to report errors to
        ArgumentNullException.ThrowIfNull(context);

        // Report an error if the content matches the forbidden regular expression pattern
        if (_regex.IsMatch(content))
        {
            context.WriteError($"File '{fileName}' matches forbidden pattern '{Pattern}'");
        }
    }
}
