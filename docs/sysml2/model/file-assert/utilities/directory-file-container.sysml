package FileAssert {
    private import SoftwareItemMetadata::*;

    part def DirectoryFileContainer {
        doc /* Filesystem implementation of IFileContainer. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.FileAssert/Utilities/DirectoryFileContainer.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Utilities/IFileContainerTests.cs");
            designFiles = ("docs/design/file-assert/utilities/directory-file-container.md");
            verificationFiles = ("docs/verification/file-assert/utilities/directory-file-container.md");
            requirementFiles = ("docs/reqstream/file-assert/utilities/directory-file-container.yaml");
        }
    }
}
