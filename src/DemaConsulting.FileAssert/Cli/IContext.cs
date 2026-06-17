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

namespace DemaConsulting.FileAssert.Cli;

/// <summary>
///     Defines the output contract for reporting assertion results and errors.
/// </summary>
/// <remarks>
///     IContext is implemented by Context (the root context) and Context.ScopedContext (a
///     scoped wrapper that prepends a path prefix to all error messages). Accepting IContext
///     in Run methods allows FileAssertZipAssert to pass a scoped context to nested asserters
///     without requiring those asserters to know about the scoping mechanism.
/// </remarks>
internal interface IContext
{
    /// <summary>
    ///     Writes a line of informational output.
    /// </summary>
    /// <param name="message">The message to write.</param>
    void WriteLine(string message);

    /// <summary>
    ///     Writes an error message, marking the context as having errors.
    /// </summary>
    /// <param name="message">The error message to write.</param>
    void WriteError(string message);

    /// <summary>
    ///     Returns a new scoped context that prepends <c>"{prefix} > "</c> to every
    ///     <see cref="WriteError"/> message.
    /// </summary>
    /// <param name="prefix">The prefix to prepend to all error messages. Must not be null.</param>
    /// <returns>A new scoped context delegating state to this context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="prefix"/> is null.</exception>
    IContext WithPrefix(string prefix);
}
