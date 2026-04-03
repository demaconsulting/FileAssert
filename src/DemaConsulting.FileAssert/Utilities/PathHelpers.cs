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

namespace DemaConsulting.FileAssert.Utilities;

/// <summary>
///     Helper utilities for safe path operations.
/// </summary>
internal static class PathHelpers
{
    /// <summary>
    ///     Safely combines two paths, ensuring the second path doesn't contain path traversal sequences.
    /// </summary>
    /// <param name="basePath">The base path.</param>
    /// <param name="relativePath">The relative path to combine.</param>
    /// <returns>The combined path.</returns>
    /// <exception cref="ArgumentException">Thrown when relativePath contains invalid characters or path traversal sequences.</exception>
    internal static string SafePathCombine(string basePath, string relativePath)
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(basePath);
        ArgumentNullException.ThrowIfNull(relativePath);

        // Combine the paths
        var combinedPath = Path.Combine(basePath, relativePath);

        // Security check: verify the combined path stays under the base directory.
        // Trim any trailing separator from the resolved base so that appending one
        // separator produces a clean prefix (e.g. base="/a/b/" would otherwise yield
        // fullBasePathWithSeparator="/a/b//" which never matches the combined path).
        // Append a trailing directory separator to the base so that a partial match
        // (e.g. base="/a/b" vs combined="/a/bc/...") is not treated as "inside" the base.
        var fullBasePath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(basePath));
        var fullCombinedPath = Path.GetFullPath(combinedPath);
        var fullBasePathWithSeparator = fullBasePath + Path.DirectorySeparatorChar;

        // Use platform-appropriate string comparison (Windows/macOS paths are case-insensitive).
        var comparison = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        // The combined path must either equal the base directory or be inside it.
        if (!fullCombinedPath.Equals(fullBasePath, comparison) &&
            !fullCombinedPath.StartsWith(fullBasePathWithSeparator, comparison))
        {
            throw new ArgumentException($"Invalid path component: {relativePath}", nameof(relativePath));
        }

        return combinedPath;
    }
}
