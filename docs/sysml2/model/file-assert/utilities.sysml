package FileAssert {
    private import SoftwareItemMetadata::*;

    part def UtilitiesSubsystem {
        doc /* File-access abstraction and small supporting helpers. */

        metadata info : SoftwareInfo {
            description = "File-access abstraction and small supporting helpers.";
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Utilities/UtilitiesTests.cs");
            designFiles = ("docs/design/file-assert/utilities.md");
            verificationFiles = ("docs/verification/file-assert/utilities.md");
            requirementFiles = ("docs/reqstream/file-assert/utilities.yaml");
        }

        part iFileContainer : IFileContainer;
        part directoryFileContainer : DirectoryFileContainer;
        part zipFileContainer : ZipFileContainer;
        part pathHelpers : PathHelpers;
        part temporaryDirectory : TemporaryDirectory;
    }
}
