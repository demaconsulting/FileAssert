package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertPdfAssert {
        doc /* PDF document assertions (PdfPig). */

        metadata info : SoftwareInfo {
            description = "PDF document assertions (PdfPig).";
            sourceFiles = ("src/DemaConsulting.FileAssert/Modeling/FileAssertPdfAssert.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Modeling/FileAssertPdfAssertTests.cs");
            designFiles = ("docs/design/file-assert/modeling/file-assert-pdf-assert.md");
            verificationFiles = ("docs/verification/file-assert/modeling/file-assert-pdf-assert.md");
            requirementFiles = ("docs/reqstream/file-assert/modeling/file-assert-pdf-assert.yaml");
        }
    }
}
