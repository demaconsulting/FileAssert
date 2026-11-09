package FileAssert {
    private import SoftwareItemMetadata::*;

    part def ZipFileContainer {
        doc /* Zip archive implementation of IFileContainer. */

        metadata info : SoftwareInfo {
            description = "Zip archive implementation of IFileContainer.";
            sourceFiles = ("src/DemaConsulting.FileAssert/Utilities/ZipFileContainer.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Utilities/IFileContainerTests.cs");
            designFiles = ("docs/design/file-assert/utilities/zip-file-container.md");
            verificationFiles = ("docs/verification/file-assert/utilities/zip-file-container.md");
            requirementFiles = ("docs/reqstream/file-assert/utilities/zip-file-container.yaml");
        }
    }
}
