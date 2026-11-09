package FileAssert {
    private import SoftwareItemMetadata::*;

    part def ModelingSubsystem {
        doc /* Test-suite data model and content-assertion rule implementations. */

        metadata info : SoftwareInfo {
            description = "Test-suite data model and content-assertion rule implementations.";
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Modeling/ModelingTests.cs");
            designFiles = ("docs/design/file-assert/modeling.md");
            verificationFiles = ("docs/verification/file-assert/modeling.md");
            requirementFiles = ("docs/reqstream/file-assert/modeling.yaml");
        }

        part fileAssertTest : FileAssertTest;
        part fileAssertFile : FileAssertFile;
        part fileAssertRule : FileAssertRule;
        part fileAssertTextAssert : FileAssertTextAssert;
        part fileAssertPdfAssert : FileAssertPdfAssert;
        part fileAssertXmlAssert : FileAssertXmlAssert;
        part fileAssertHtmlAssert : FileAssertHtmlAssert;
        part fileAssertYamlAssert : FileAssertYamlAssert;
        part fileAssertJsonAssert : FileAssertJsonAssert;
        part fileAssertZipAssert : FileAssertZipAssert;
    }
}
