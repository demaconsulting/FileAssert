package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertJsonAssert {
        doc /* JSON document assertions (System.Text.Json). */

        metadata info : SoftwareInfo {
            description = "JSON document assertions (System.Text.Json).";
            sourceFiles = ("src/DemaConsulting.FileAssert/Modeling/FileAssertJsonAssert.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Modeling/FileAssertJsonAssertTests.cs");
            designFiles = ("docs/design/file-assert/modeling/file-assert-json-assert.md");
            verificationFiles = ("docs/verification/file-assert/modeling/file-assert-json-assert.md");
            requirementFiles = ("docs/reqstream/file-assert/modeling/file-assert-json-assert.yaml");
        }
    }
}
