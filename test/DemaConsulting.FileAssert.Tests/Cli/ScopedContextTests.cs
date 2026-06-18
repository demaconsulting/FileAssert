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
///     Unit tests for <see cref="Context.WithPrefix"/> and the nested <c>ScopedContext</c> class.
/// </summary>
[Collection("Sequential")]
public sealed class ScopedContextTests
{
    /// <summary>
    ///     Verifies that WithPrefix returns a non-null scoped context.
    /// </summary>
    [Fact]
    public void Context_WithPrefix_ReturnsNonNullScopedContext()
    {
        // Arrange
        using var context = Context.Create(["--silent"]);

        // Act
        var scoped = context.WithPrefix("archive.zip");

        // Assert
        Assert.NotNull(scoped);
    }

    /// <summary>
    ///     Verifies that WithPrefix throws <see cref="ArgumentNullException"/> when prefix is null.
    /// </summary>
    [Fact]
    public void Context_WithPrefix_NullPrefix_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = Context.Create(["--silent"]);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context.WithPrefix(null!));
    }

    /// <summary>
    ///     Verifies that errors written via a scoped context propagate to the root context exit code.
    /// </summary>
    [Fact]
    public void ScopedContext_WriteError_PropagatesExitCodeToRoot()
    {
        // Arrange - create a root context; derive a scoped context from it
        using var context = Context.Create(["--silent"]);
        var scoped = context.WithPrefix("archive.zip");

        // Act - write an error via the scoped context
        scoped.WriteError("some error");

        // Assert - the root context must reflect the error
        Assert.Equal(1, context.ExitCode);
        Assert.Equal(1, context.ErrorCount);
    }

    /// <summary>
    ///     Verifies that WriteLine written via a scoped context does not set an error on the root.
    /// </summary>
    [Fact]
    public void ScopedContext_WriteLine_DoesNotSetError()
    {
        // Arrange
        using var context = Context.Create(["--silent"]);
        var scoped = context.WithPrefix("archive.zip");

        // Act
        scoped.WriteLine("informational message");

        // Assert - no error should be recorded
        Assert.Equal(0, context.ExitCode);
        Assert.Equal(0, context.ErrorCount);
    }

    /// <summary>
    ///     Verifies that nested scoped contexts chain prefixes correctly and still propagate errors.
    /// </summary>
    [Fact]
    public void ScopedContext_Nested_WriteError_PropagatesExitCodeToRoot()
    {
        // Arrange - two levels of scoping
        using var context = Context.Create(["--silent"]);
        var level1 = context.WithPrefix("outer.zip");
        var level2 = level1.WithPrefix("inner.zip");

        // Act
        level2.WriteError("nested error");

        // Assert - root reflects the error
        Assert.Equal(1, context.ExitCode);
        Assert.Equal(1, context.ErrorCount);
    }

    /// <summary>
    ///     Verifies that multiple errors from different scoped contexts all accumulate on the root.
    /// </summary>
    [Fact]
    public void ScopedContext_MultipleErrors_AllAccumulateOnRoot()
    {
        // Arrange
        using var context = Context.Create(["--silent"]);
        var scoped1 = context.WithPrefix("zip1.zip");
        var scoped2 = context.WithPrefix("zip2.zip");

        // Act
        scoped1.WriteError("error in zip1");
        scoped2.WriteError("error in zip2");
        context.WriteError("direct error");

        // Assert - all three errors must be counted
        Assert.Equal(3, context.ErrorCount);
        Assert.Equal(1, context.ExitCode);
    }
}
