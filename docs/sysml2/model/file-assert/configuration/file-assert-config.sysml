package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertConfig {
        doc /* Top-level configuration loader and test runner. */

        metadata info : SoftwareInfo {
            description = "Top-level configuration loader and test runner.";
            sourceFiles = ("src/DemaConsulting.FileAssert/Configuration/FileAssertConfig.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Configuration/FileAssertConfigTests.cs");
            designFiles = ("docs/design/file-assert/configuration/file-assert-config.md");
            verificationFiles = ("docs/verification/file-assert/configuration/file-assert-config.md");
            requirementFiles = ("docs/reqstream/file-assert/configuration/file-assert-config.yaml");
        }
    }
}
