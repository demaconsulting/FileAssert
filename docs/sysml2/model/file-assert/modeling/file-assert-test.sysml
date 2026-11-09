package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertTest {
        doc /* Named test with file assertions and tag filtering. */

        metadata info : SoftwareInfo {
            description = "Named test with file assertions and tag filtering.";
            sourceFiles = ("src/DemaConsulting.FileAssert/Modeling/FileAssertTest.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Modeling/FileAssertTestTests.cs");
            designFiles = ("docs/design/file-assert/modeling/file-assert-test.md");
            verificationFiles = ("docs/verification/file-assert/modeling/file-assert-test.md");
            requirementFiles = ("docs/reqstream/file-assert/modeling/file-assert-test.yaml");
        }
    }
}
