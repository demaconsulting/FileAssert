package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertFile {
        doc /* Glob pattern matcher with count constraints and content rules. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.FileAssert/Modeling/FileAssertFile.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Modeling/FileAssertFileTests.cs");
            designFiles = ("docs/design/file-assert/modeling/file-assert-file.md");
            verificationFiles = ("docs/verification/file-assert/modeling/file-assert-file.md");
            requirementFiles = ("docs/reqstream/file-assert/modeling/file-assert-file.yaml");
        }
    }
}
