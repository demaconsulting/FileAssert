package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertZipAssert {
        doc /* Zip archive entry assertions (System.IO.Compression). */

        metadata info : SoftwareInfo {
            description = "Zip archive entry assertions (System.IO.Compression).";
            sourceFiles = ("src/DemaConsulting.FileAssert/Modeling/FileAssertZipAssert.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Modeling/FileAssertZipAssertTests.cs");
            designFiles = ("docs/design/file-assert/modeling/file-assert-zip-assert.md");
            verificationFiles = ("docs/verification/file-assert/modeling/file-assert-zip-assert.md");
            requirementFiles = ("docs/reqstream/file-assert/modeling/file-assert-zip-assert.yaml");
        }
    }
}
