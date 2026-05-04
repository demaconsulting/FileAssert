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

namespace DemaConsulting.FileAssert.Tests.Cli;

/// <summary>
///     Unit tests for the new <c>ConfigFile</c>, <c>Filters</c>, and <c>--config</c> features of <see cref="Context"/>.
/// </summary>
[Collection("Sequential")]
public class ContextNewPropertiesTests
{
    /// <summary>
    ///     Verifies that ConfigFile defaults to <c>.fileassert.yaml</c> when no arguments are provided.
    /// </summary>
    [Fact]
    public void Context_Create_NoArguments_ConfigFileHasDefaultValue()
    {
        // Act
        using var context = Context.Create([]);

        // Assert
        Assert.Equal(".fileassert.yaml", context.ConfigFile);
    }

    /// <summary>
    ///     Verifies that Filters is empty when no positional arguments are provided.
    /// </summary>
    [Fact]
    public void Context_Create_NoArguments_FiltersIsEmpty()
    {
        // Act
        using var context = Context.Create([]);

        // Assert
        Assert.Empty(context.Filters);
    }

    /// <summary>
    ///     Verifies that <c>--config</c> sets the ConfigFile property.
    /// </summary>
    [Fact]
    public void Context_Create_ConfigFlag_SetsConfigFile()
    {
        // Act
        using var context = Context.Create(["--config", "my-tests.yaml"]);

        // Assert
        Assert.Equal("my-tests.yaml", context.ConfigFile);
    }

    /// <summary>
    ///     Verifies that positional arguments are collected into the Filters list.
    /// </summary>
    [Fact]
    public void Context_Create_PositionalArguments_AddedToFilters()
    {
        // Act
        using var context = Context.Create(["smoke", "regression"]);

        // Assert
        Assert.Equal(2, context.Filters.Count);
        Assert.Equal("smoke", context.Filters[0]);
        Assert.Equal("regression", context.Filters[1]);
    }

    /// <summary>
    ///     Verifies that positional arguments may be mixed with flag arguments.
    /// </summary>
    [Fact]
    public void Context_Create_MixedArguments_ParsesCorrectly()
    {
        // Act
        using var context = Context.Create(["--silent", "my-filter", "--config", "cfg.yaml"]);

        // Assert
        Assert.True(context.Silent);
        Assert.Equal("cfg.yaml", context.ConfigFile);
        Assert.Single(context.Filters);
        Assert.Equal("my-filter", context.Filters[0]);
    }

    /// <summary>
    ///     Verifies that an unknown flag (starting with <c>-</c>) still throws <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void Context_Create_UnknownFlagWithDash_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Context.Create(["--bogus-flag"]));
        Assert.Contains("Unsupported argument", exception.Message);
    }

    /// <summary>
    ///     Verifies that <c>--config</c> without a value throws <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void Context_Create_ConfigFlag_WithoutValue_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Context.Create(["--config"]));
        Assert.Contains("--config", exception.Message);
    }
}
