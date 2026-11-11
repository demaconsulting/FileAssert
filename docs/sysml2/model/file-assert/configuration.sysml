package FileAssert {
    private import SoftwareItemMetadata::*;

    part def ConfigurationSubsystem {
        doc /* Top-level configuration loading, deserialization, and test execution. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Configuration/ConfigurationTests.cs");
            designFiles = ("docs/design/file-assert/configuration.md");
            verificationFiles = ("docs/verification/file-assert/configuration.md");
            requirementFiles = ("docs/reqstream/file-assert/configuration.yaml");
        }

        part fileAssertConfig : FileAssertConfig;
        part fileAssertData : FileAssertData;
    }
}
