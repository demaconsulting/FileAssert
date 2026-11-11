package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertData {
        doc /* YAML data transfer objects for configuration deserialization. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.FileAssert/Configuration/FileAssertData.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Configuration/FileAssertConfigTests.cs");
            designFiles = ("docs/design/file-assert/configuration/file-assert-data.md");
            verificationFiles = ("docs/verification/file-assert/configuration/file-assert-data.md");
            requirementFiles = ("docs/reqstream/file-assert/configuration/file-assert-data.yaml");
        }
    }
}
