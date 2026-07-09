package FileAssert {
    part def UtilitiesSubsystem {
        doc /* File-access abstraction and small supporting helpers. */

        comment testRef /* Test: test/DemaConsulting.FileAssert.Tests/Utilities/UtilitiesTests.cs */
        comment designRef /* Design: docs/design/file-assert/utilities.md */
        comment verificationRef /* Verification: docs/verification/file-assert/utilities.md */
        comment reqRef /* Requirements: docs/reqstream/file-assert/utilities.yaml */

        part iFileContainer : IFileContainer;
        part directoryFileContainer : DirectoryFileContainer;
        part zipFileContainer : ZipFileContainer;
        part pathHelpers : PathHelpers;
        part temporaryDirectory : TemporaryDirectory;
    }
}
