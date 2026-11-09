package FileAssert {
    private import SoftwareItemMetadata::*;

    part def IFileContainer {
        doc /* Uniform file-access abstraction over directories and zip archives. */

        metadata info : SoftwareInfo {
            description = "Uniform file-access abstraction over directories and zip archives.";
            sourceFiles = ("src/DemaConsulting.FileAssert/Utilities/IFileContainer.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Utilities/IFileContainerTests.cs");
            designFiles = ("docs/design/file-assert/utilities/i-file-container.md");
            verificationFiles = ("docs/verification/file-assert/utilities/i-file-container.md");
            requirementFiles = ("docs/reqstream/file-assert/utilities/i-file-container.yaml");
        }
    }
}
