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

namespace DemaConsulting.FileAssert.Tests;

/// <summary>
///     Unit tests for the new <c>ConfigFile</c>, <c>Filters</c>, and <c>--config</c> features of <see cref="Context"/>.
/// </summary>
[TestClass]
public class ContextNewPropertiesTests
{
    /// <summary>
    ///     Verifies that ConfigFile defaults to <c>.fileassert.yaml</c> when no arguments are provided.
    /// </summary>
    [TestMethod]
    public void Context_Create_NoArguments_ConfigFileHasDefaultValue()
    {
        // Act
        using var context = Context.Create([]);

        // Assert
        Assert.AreEqual(".fileassert.yaml", context.ConfigFile);
    }

    /// <summary>
    ///     Verifies that Filters is empty when no positional arguments are provided.
    /// </summary>
    [TestMethod]
    public void Context_Create_NoArguments_FiltersIsEmpty()
    {
        // Act
        using var context = Context.Create([]);

        // Assert
        Assert.AreEqual(0, context.Filters.Count);
    }

    /// <summary>
    ///     Verifies that <c>--config</c> sets the ConfigFile property.
    /// </summary>
    [TestMethod]
    public void Context_Create_ConfigFlag_SetsConfigFile()
    {
        // Act
        using var context = Context.Create(["--config", "my-tests.yaml"]);

        // Assert
        Assert.AreEqual("my-tests.yaml", context.ConfigFile);
    }

    /// <summary>
    ///     Verifies that positional arguments are collected into the Filters list.
    /// </summary>
    [TestMethod]
    public void Context_Create_PositionalArguments_AddedToFilters()
    {
        // Act
        using var context = Context.Create(["smoke", "regression"]);

        // Assert
        Assert.AreEqual(2, context.Filters.Count);
        Assert.AreEqual("smoke", context.Filters[0]);
        Assert.AreEqual("regression", context.Filters[1]);
    }

    /// <summary>
    ///     Verifies that positional arguments may be mixed with flag arguments.
    /// </summary>
    [TestMethod]
    public void Context_Create_MixedArguments_ParsesCorrectly()
    {
        // Act
        using var context = Context.Create(["--silent", "my-filter", "--config", "cfg.yaml"]);

        // Assert
        Assert.IsTrue(context.Silent);
        Assert.AreEqual("cfg.yaml", context.ConfigFile);
        Assert.AreEqual(1, context.Filters.Count);
        Assert.AreEqual("my-filter", context.Filters[0]);
    }

    /// <summary>
    ///     Verifies that an unknown flag (starting with <c>-</c>) still throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void Context_Create_UnknownFlagWithDash_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Context.Create(["--bogus-flag"]));
        Assert.Contains("Unsupported argument", exception.Message);
    }

    /// <summary>
    ///     Verifies that <c>--config</c> without a value throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void Context_Create_ConfigFlag_WithoutValue_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Context.Create(["--config"]));
        Assert.Contains("--config", exception.Message);
    }
}
