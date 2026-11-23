package FileAssert_UtilitiesSubsystem {
    public import FileAssert_IFileContainer::*;
    public import FileAssert_DirectoryFileContainer::*;
    public import FileAssert_ZipFileContainer::*;
    public import FileAssert_PathHelpers::*;
    public import FileAssert_TemporaryDirectory::*;
    private import SoftwareItemMetadata::*;

    part def UtilitiesSubsystem {
        doc /* File-access abstraction and small supporting helpers. */

        metadata info : SoftwareInfo {
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
