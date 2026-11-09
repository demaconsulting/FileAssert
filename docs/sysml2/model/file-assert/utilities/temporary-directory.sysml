package FileAssert {
    private import SoftwareItemMetadata::*;

    part def TemporaryDirectory {
        doc /* Disposable temporary directory utility. */

        metadata info : SoftwareInfo {
            description = "Disposable temporary directory utility.";
            sourceFiles = ("src/DemaConsulting.FileAssert/Utilities/TemporaryDirectory.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Utilities/TemporaryDirectoryTests.cs");
            designFiles = ("docs/design/file-assert/utilities/temporary-directory.md");
            verificationFiles = ("docs/verification/file-assert/utilities/temporary-directory.md");
            requirementFiles = ("docs/reqstream/file-assert/utilities/temporary-directory.yaml");
        }
    }
}
