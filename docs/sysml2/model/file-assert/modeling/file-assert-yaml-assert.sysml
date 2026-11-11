package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertYamlAssert {
        doc /* YAML document assertions (YamlDotNet). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.FileAssert/Modeling/FileAssertYamlAssert.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Modeling/FileAssertYamlAssertTests.cs");
            designFiles = ("docs/design/file-assert/modeling/file-assert-yaml-assert.md");
            verificationFiles = ("docs/verification/file-assert/modeling/file-assert-yaml-assert.md");
            requirementFiles = ("docs/reqstream/file-assert/modeling/file-assert-yaml-assert.yaml");
        }
    }
}
