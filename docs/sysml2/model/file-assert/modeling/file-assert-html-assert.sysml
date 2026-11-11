package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertHtmlAssert {
        doc /* HTML document assertions (HtmlAgilityPack). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.FileAssert/Modeling/FileAssertHtmlAssert.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Modeling/FileAssertHtmlAssertTests.cs");
            designFiles = ("docs/design/file-assert/modeling/file-assert-html-assert.md");
            verificationFiles = ("docs/verification/file-assert/modeling/file-assert-html-assert.md");
            requirementFiles = ("docs/reqstream/file-assert/modeling/file-assert-html-assert.yaml");
        }
    }
}
