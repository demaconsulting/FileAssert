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
using DemaConsulting.FileAssert.Modeling;

namespace DemaConsulting.FileAssert.Tests.Modeling;

/// <summary>
///     Unit tests for <see cref="FileAssertRule"/> and its derived rule classes.
/// </summary>
[Collection("Sequential")]
public class FileAssertRuleTests
{
    /// <summary>
    ///     Verifies that the factory creates a <see cref="FileAssertContainsRule"/> when 'contains' is specified.
    /// </summary>
    [Fact]
    public void FileAssertRule_Create_WithContains_ReturnsContainsRule()
    {
        // Arrange
        var data = new FileAssertRuleData { Contains = "expected text" };

        // Act
        var rule = FileAssertRule.Create(data);

        // Assert
        Assert.IsType<FileAssertContainsRule>(rule);
        Assert.Equal("expected text", ((FileAssertContainsRule)rule).Value);
    }

    /// <summary>
    ///     Verifies that the factory creates a <see cref="FileAssertMatchesRule"/> when 'matches' is specified.
    /// </summary>
    [Fact]
    public void FileAssertRule_Create_WithMatches_ReturnsMatchesRule()
    {
        // Arrange
        var data = new FileAssertRuleData { Matches = @"\d+" };

        // Act
        var rule = FileAssertRule.Create(data);

        // Assert
        Assert.IsType<FileAssertMatchesRule>(rule);
        Assert.Equal(@"\d+", ((FileAssertMatchesRule)rule).Pattern);
    }

    /// <summary>
    ///     Verifies that the factory throws <see cref="InvalidOperationException"/> when no rule type is specified.
    /// </summary>
    [Fact]
    public void FileAssertRule_Create_WithNoType_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new FileAssertRuleData();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => FileAssertRule.Create(data));
        Assert.Contains("contains", exception.Message);
        Assert.Contains("does-not-contain", exception.Message);
        Assert.Contains("matches", exception.Message);
        Assert.Contains("does-not-contain-regex", exception.Message);
    }

    /// <summary>
    ///     Verifies that the factory throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [Fact]
    public void FileAssertRule_Create_WithNullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertRule.Create(null!));
    }

    /// <summary>
    ///     Verifies that <see cref="FileAssertContainsRule.Apply"/> produces no error when content contains the value.
    /// </summary>
    [Fact]
    public void FileAssertContainsRule_Apply_ContentContainsValue_NoError()
    {
        // Arrange
        var rule = new FileAssertContainsRule("hello");
        using var context = Context.Create(["--silent"]);

        // Act
        rule.Apply(context, "test.txt", "say hello world");

        // Assert
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    ///     Verifies that <see cref="FileAssertContainsRule.Apply"/> reports an error when content is missing the value.
    /// </summary>
    [Fact]
    public void FileAssertContainsRule_Apply_ContentMissingValue_WritesError()
    {
        // Arrange
        var rule = new FileAssertContainsRule("hello");
        using var context = Context.Create(["--silent"]);

        // Act
        rule.Apply(context, "test.txt", "nothing relevant here");

        // Assert
        Assert.Equal(1, context.ExitCode);
    }

    /// <summary>
    ///     Verifies that <see cref="FileAssertMatchesRule.Apply"/> produces no error when content matches the pattern.
    /// </summary>
    [Fact]
    public void FileAssertMatchesRule_Apply_ContentMatchesPattern_NoError()
    {
        // Arrange
        var rule = new FileAssertMatchesRule(@"\d+");
        using var context = Context.Create(["--silent"]);

        // Act
        rule.Apply(context, "test.txt", "version 42 is here");

        // Assert
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    ///     Verifies that <see cref="FileAssertMatchesRule.Apply"/> reports an error when content does not match.
    /// </summary>
    [Fact]
    public void FileAssertMatchesRule_Apply_ContentDoesNotMatchPattern_WritesError()
    {
        // Arrange
        var rule = new FileAssertMatchesRule(@"\d+");
        using var context = Context.Create(["--silent"]);

        // Act
        rule.Apply(context, "test.txt", "no numbers here at all");

        // Assert
        Assert.Equal(1, context.ExitCode);
    }

    /// <summary>
    ///     Verifies that the factory creates a <see cref="FileAssertDoesNotContainRule"/> when 'does-not-contain' is specified.
    /// </summary>
    [Fact]
    public void FileAssertRule_Create_WithDoesNotContain_ReturnsDoesNotContainRule()
    {
        // Arrange
        var data = new FileAssertRuleData { DoesNotContain = "forbidden text" };

        // Act
        var rule = FileAssertRule.Create(data);

        // Assert
        Assert.IsType<FileAssertDoesNotContainRule>(rule);
        Assert.Equal("forbidden text", ((FileAssertDoesNotContainRule)rule).Value);
    }

    /// <summary>
    ///     Verifies that the factory creates a <see cref="FileAssertDoesNotMatchRule"/> when 'does-not-contain-regex' is specified.
    /// </summary>
    [Fact]
    public void FileAssertRule_Create_WithDoesNotContainRegex_ReturnsDoesNotMatchRule()
    {
        // Arrange
        var data = new FileAssertRuleData { DoesNotContainRegex = @"FATAL|ERROR" };

        // Act
        var rule = FileAssertRule.Create(data);

        // Assert
        Assert.IsType<FileAssertDoesNotMatchRule>(rule);
        Assert.Equal(@"FATAL|ERROR", ((FileAssertDoesNotMatchRule)rule).Pattern);
    }

    /// <summary>
    ///     Verifies that <see cref="FileAssertDoesNotContainRule.Apply"/> reports an error when content contains the forbidden value.
    /// </summary>
    [Fact]
    public void FileAssertDoesNotContainRule_Apply_ContentContainsValue_WritesError()
    {
        // Arrange
        var rule = new FileAssertDoesNotContainRule("password123");
        using var context = Context.Create(["--silent"]);

        // Act
        rule.Apply(context, "test.txt", "the password123 is here");

        // Assert
        Assert.Equal(1, context.ExitCode);
    }

    /// <summary>
    ///     Verifies that <see cref="FileAssertDoesNotContainRule.Apply"/> produces no error when content does not contain the forbidden value.
    /// </summary>
    [Fact]
    public void FileAssertDoesNotContainRule_Apply_ContentMissingValue_NoError()
    {
        // Arrange
        var rule = new FileAssertDoesNotContainRule("password123");
        using var context = Context.Create(["--silent"]);

        // Act
        rule.Apply(context, "test.txt", "no secrets here");

        // Assert
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    ///     Verifies that <see cref="FileAssertDoesNotMatchRule.Apply"/> reports an error when content matches the forbidden pattern.
    /// </summary>
    [Fact]
    public void FileAssertDoesNotMatchRule_Apply_ContentMatchesPattern_WritesError()
    {
        // Arrange
        var rule = new FileAssertDoesNotMatchRule(@"FATAL|ERROR");
        using var context = Context.Create(["--silent"]);

        // Act
        rule.Apply(context, "test.txt", "FATAL: something went wrong");

        // Assert
        Assert.Equal(1, context.ExitCode);
    }

    /// <summary>
    ///     Verifies that <see cref="FileAssertDoesNotMatchRule.Apply"/> produces no error when content does not match the forbidden pattern.
    /// </summary>
    [Fact]
    public void FileAssertDoesNotMatchRule_Apply_ContentDoesNotMatchPattern_NoError()
    {
        // Arrange
        var rule = new FileAssertDoesNotMatchRule(@"FATAL|ERROR");
        using var context = Context.Create(["--silent"]);

        // Act
        rule.Apply(context, "test.txt", "everything is fine");

        // Assert
        Assert.Equal(0, context.ExitCode);
    }
}
