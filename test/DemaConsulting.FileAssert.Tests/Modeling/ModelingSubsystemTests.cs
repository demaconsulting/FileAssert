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
///     Subsystem tests for the Modeling subsystem.
/// </summary>
[TestClass]
public class ModelingSubsystemTests
{
    /// <summary>
    ///     Verifies that the Modeling subsystem executes the full test → file → rule
    ///     chain without errors when all constraints are satisfied.
    /// </summary>
    [TestMethod]
    public void ModelingSubsystem_ExecuteChain_PassesWhenAllConstraintsMet()
    {
        // Arrange - create a real file with content that satisfies all rules
        var tempDir = Directory.CreateTempSubdirectory("fileassert_modeling_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "sample.txt"), "Copyright (c) DEMA Consulting");

            var testData = new FileAssertTestData
            {
                Name = "License Check",
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "*.txt",
                        Min = 1,
                        Text =
                        [
                            new FileAssertRuleData { Contains = "Copyright" }
                        ]
                    }
                ]
            };

            var test = FileAssertTest.Create(testData);
            using var context = Context.Create(["--silent"]);

            // Act
            test.Run(context, tempDir.FullName);

            // Assert - no errors reported
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that the Modeling subsystem reports failures through the context
    ///     when a content rule is not satisfied.
    /// </summary>
    [TestMethod]
    public void ModelingSubsystem_ExecuteChain_ReportsFailuresThroughContext()
    {
        // Arrange - create a file that does NOT contain the required text
        var tempDir = Directory.CreateTempSubdirectory("fileassert_modeling_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "sample.txt"), "no license header here");

            var testData = new FileAssertTestData
            {
                Name = "License Check",
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "*.txt",
                        Text =
                        [
                            new FileAssertRuleData { Contains = "Copyright" }
                        ]
                    }
                ]
            };

            var test = FileAssertTest.Create(testData);
            using var context = Context.Create(["--silent"]);

            // Act
            test.Run(context, tempDir.FullName);

            // Assert - an error was reported and the exit code is non-zero
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that the Modeling subsystem reports a parse error when a file-type
    ///     assertion block is declared but the file cannot be parsed as the declared format.
    /// </summary>
    [TestMethod]
    public void ModelingSubsystem_FileTypeParsing_InvalidXml_ReportsParseError()
    {
        // Arrange - create a file with invalid XML content
        var tempDir = Directory.CreateTempSubdirectory("fileassert_modeling_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "config.xml"), "this is not valid xml <<>>");

            var testData = new FileAssertTestData
            {
                Name = "XmlCheck",
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "*.xml",
                        Xml =
                        [
                            new FileAssertQueryData { Query = "//root", Count = 1 }
                        ]
                    }
                ]
            };

            var test = FileAssertTest.Create(testData);
            using var context = Context.Create(["--silent"]);

            // Act
            test.Run(context, tempDir.FullName);

            // Assert - an error was reported because the file could not be parsed as XML
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that the Modeling subsystem evaluates XPath query assertions against
    ///     a valid XML document and reports no error when the count constraint is satisfied.
    /// </summary>
    [TestMethod]
    public void ModelingSubsystem_QueryAssertions_XmlQueryMeetsCount_NoError()
    {
        // Arrange - create a valid XML file with elements the query will match
        var tempDir = Directory.CreateTempSubdirectory("fileassert_modeling_");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "config.xml"), """
                <configuration>
                  <setting key="env">production</setting>
                  <setting key="debug">false</setting>
                </configuration>
                """);

            var testData = new FileAssertTestData
            {
                Name = "XmlQueryCheck",
                Files =
                [
                    new FileAssertFileData
                    {
                        Pattern = "*.xml",
                        Xml =
                        [
                            new FileAssertQueryData { Query = "//configuration/setting", Min = 1 }
                        ]
                    }
                ]
            };

            var test = FileAssertTest.Create(testData);
            using var context = Context.Create(["--silent"]);

            // Act
            test.Run(context, tempDir.FullName);

            // Assert - no errors reported because the query matched the expected count
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
