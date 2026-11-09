package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertXmlAssert {
        doc /* XML document assertions (System.Xml.Linq/XPath). */

        metadata info : SoftwareInfo {
            description = "XML document assertions (System.Xml.Linq/XPath).";
            sourceFiles = ("src/DemaConsulting.FileAssert/Modeling/FileAssertXmlAssert.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Modeling/FileAssertXmlAssertTests.cs");
            designFiles = ("docs/design/file-assert/modeling/file-assert-xml-assert.md");
            verificationFiles = ("docs/verification/file-assert/modeling/file-assert-xml-assert.md");
            requirementFiles = ("docs/reqstream/file-assert/modeling/file-assert-xml-assert.yaml");
        }
    }
}
