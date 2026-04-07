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

namespace DemaConsulting.FileAssert.Tests.Configuration;

/// <summary>
///     Subsystem tests for the Configuration subsystem.
/// </summary>
[TestClass]
public class ConfigurationTests
{
    /// <summary>
    ///     Verifies that the Configuration subsystem loads a YAML file and builds the
    ///     complete test hierarchy (tests → files → rules) correctly.
    /// </summary>
    [TestMethod]
    public void Configuration_LoadYaml_BuildsCompleteTestHierarchy()
    {
        // Arrange - write a YAML configuration with nested test, file, and rule entries
        var tempDir = Directory.CreateTempSubdirectory("fileassert_config_");
        try
        {
            var configPath = Path.Combine(tempDir.FullName, "config.yaml");
            File.WriteAllText(configPath, """
                tests:
                  - name: "License Check"
                    tags:
                      - license
                    files:
                      - pattern: "**/*.txt"
                        min: 1
                        text:
                          - contains: "Copyright"
                """);

            // Act
            var config = FileAssertConfig.ReadFromFile(configPath);

            // Assert - the full hierarchy is correctly constructed
            Assert.AreEqual(1, config.Tests.Count);
            var test = config.Tests[0];
            Assert.AreEqual("License Check", test.Name);
            Assert.AreEqual(1, test.Tags.Count);
            Assert.AreEqual("license", test.Tags[0]);
            Assert.AreEqual(1, test.Files.Count);
            var file = test.Files[0];
            Assert.AreEqual("**/*.txt", file.Pattern);
            Assert.AreEqual(1, file.Min);
            Assert.AreEqual(1, file.TextAssert!.Rules.Count);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that the Configuration subsystem executes only tests that match
    ///     the provided filters when running a configuration with multiple tests.
    /// </summary>
    [TestMethod]
    public void Configuration_RunWithFilter_ExecutesOnlyMatchingTests()
    {
        // Arrange - two tests in config; only one file exists so only that test should pass
        var tempDir = Directory.CreateTempSubdirectory("fileassert_config_");
        try
        {
            var configPath = Path.Combine(tempDir.FullName, "config.yaml");
            File.WriteAllText(configPath, """
                tests:
                  - name: "Alpha"
                    files:
                      - pattern: "alpha.txt"
                        min: 1
                  - name: "Beta"
                    files:
                      - pattern: "beta.txt"
                        min: 1
                """);

            // Create only alpha.txt so the Alpha test passes and Beta would fail
            File.WriteAllText(Path.Combine(tempDir.FullName, "alpha.txt"), "content");

            var config = FileAssertConfig.ReadFromFile(configPath);
            using var context = Context.Create(["--silent"]);

            // Act - run with the "Alpha" filter only
            config.Run(context, ["Alpha"]);

            // Assert - no errors because only Alpha ran (and alpha.txt exists)
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    /// <summary>
    ///     Verifies that the Configuration subsystem executes only tests whose tag matches
    ///     the provided filter when running a configuration with multiple tests.
    /// </summary>
    [TestMethod]
    public void Configuration_RunWithTagFilter_ExecutesOnlyMatchingTests()
    {
        // Arrange - two tests with different tags; only one file exists so only that test passes
        var tempDir = Directory.CreateTempSubdirectory("fileassert_config_");
        try
        {
            var configPath = Path.Combine(tempDir.FullName, "config.yaml");
            File.WriteAllText(configPath, """
                tests:
                  - name: "Alpha"
                    tags:
                      - smoke
                    files:
                      - pattern: "alpha.txt"
                        min: 1
                  - name: "Beta"
                    tags:
                      - regression
                    files:
                      - pattern: "beta.txt"
                        min: 1
                """);

            // Create only alpha.txt so the Alpha test passes and Beta would fail
            File.WriteAllText(Path.Combine(tempDir.FullName, "alpha.txt"), "content");

            var config = FileAssertConfig.ReadFromFile(configPath);
            using var context = Context.Create(["--silent"]);

            // Act - run with the "smoke" tag filter only
            config.Run(context, ["smoke"]);

            // Assert - no errors because only Alpha ran (matching the smoke tag) and alpha.txt exists
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
