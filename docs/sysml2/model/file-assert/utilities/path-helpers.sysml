package FileAssert {
    private import SoftwareItemMetadata::*;

    part def PathHelpers {
        doc /* Safe path-combination utility. */

        metadata info : SoftwareInfo {
            description = "Safe path-combination utility.";
            sourceFiles = ("src/DemaConsulting.FileAssert/Utilities/PathHelpers.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Utilities/PathHelpersTests.cs");
            designFiles = ("docs/design/file-assert/utilities/path-helpers.md");
            verificationFiles = ("docs/verification/file-assert/utilities/path-helpers.md");
            requirementFiles = ("docs/reqstream/file-assert/utilities/path-helpers.yaml");
        }
    }
}
