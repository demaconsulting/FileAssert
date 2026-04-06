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
///     Unit tests for the <see cref="FileAssertXmlAssert"/> class.
/// </summary>
[TestClass]
public sealed class FileAssertXmlAssertTests
{
    private const string SampleXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <root>
          <item>one</item>
          <item>two</item>
          <item>three</item>
        </root>
        """;

    /// <summary>
    ///     Verifies that Create succeeds given valid query data.
    /// </summary>
    [TestMethod]
    public void FileAssertXmlAssert_Create_ValidData_CreatesXmlAssert()
    {
        // Arrange
        var data = new List<FileAssertQueryData>
        {
            new() { Query = "//item", Count = 3 }
        };

        // Act
        var xmlAssert = FileAssertXmlAssert.Create(data);

        // Assert
        Assert.IsNotNull(xmlAssert);
    }

    /// <summary>
    ///     Verifies that Create throws <see cref="ArgumentNullException"/> when data is null.
    /// </summary>
    [TestMethod]
    public void FileAssertXmlAssert_Create_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileAssertXmlAssert.Create(null!));
    }

    /// <summary>
    ///     Verifies that Run reports an error when the file cannot be parsed as XML.
    /// </summary>
    [TestMethod]
    public void FileAssertXmlAssert_Run_InvalidFile_WritesError()
    {
        // Arrange - create a non-XML file
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "this is not XML");
            var data = new List<FileAssertQueryData> { new() { Query = "//item", Count = 1 } };
            var xmlAssert = FileAssertXmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            xmlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the XPath count matches exactly.
    /// </summary>
    [TestMethod]
    public void FileAssertXmlAssert_Run_ExactCount_Matches_NoError()
    {
        // Arrange - write sample XML with 3 item elements
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleXml);
            var data = new List<FileAssertQueryData> { new() { Query = "//item", Count = 3 } };
            var xmlAssert = FileAssertXmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            xmlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the XPath count does not match exactly.
    /// </summary>
    [TestMethod]
    public void FileAssertXmlAssert_Run_ExactCount_Mismatch_WritesError()
    {
        // Arrange - sample XML has 3 items but we assert count = 5
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleXml);
            var data = new List<FileAssertQueryData> { new() { Query = "//item", Count = 5 } };
            var xmlAssert = FileAssertXmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            xmlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run produces no error when the XPath count is within min/max bounds.
    /// </summary>
    [TestMethod]
    public void FileAssertXmlAssert_Run_MinMaxCount_WithinBounds_NoError()
    {
        // Arrange - sample XML has 3 items; assert min=2, max=5
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleXml);
            var data = new List<FileAssertQueryData> { new() { Query = "//item", Min = 2, Max = 5 } };
            var xmlAssert = FileAssertXmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            xmlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    ///     Verifies that Run reports an error when the XPath query is invalid syntax.
    /// </summary>
    [TestMethod]
    public void FileAssertXmlAssert_Run_InvalidXPathQuery_WritesError()
    {
        // Arrange - valid XML but an XPath expression with invalid syntax
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleXml);
            var data = new List<FileAssertQueryData> { new() { Query = "//item[invalid", Count = 1 } };
            var xmlAssert = FileAssertXmlAssert.Create(data);
            using var context = Context.Create(["--silent"]);

            // Act
            xmlAssert.Run(context, tempFile);

            // Assert
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
