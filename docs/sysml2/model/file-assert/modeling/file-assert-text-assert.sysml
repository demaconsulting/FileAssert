package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertTextAssert {
        doc /* Text content assertions. */

        metadata info : SoftwareInfo {
            description = "Text content assertions.";
            sourceFiles = ("src/DemaConsulting.FileAssert/Modeling/FileAssertTextAssert.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Modeling/FileAssertTextAssertTests.cs");
            designFiles = ("docs/design/file-assert/modeling/file-assert-text-assert.md");
            verificationFiles = ("docs/verification/file-assert/modeling/file-assert-text-assert.md");
            requirementFiles = ("docs/reqstream/file-assert/modeling/file-assert-text-assert.yaml");
        }
    }
}
