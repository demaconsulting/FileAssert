package FileAssert {
    private import SoftwareItemMetadata::*;

    part def FileAssertRule {
        doc /* Abstract content validation rule hierarchy. */

        metadata info : SoftwareInfo {
            description = "Abstract content validation rule hierarchy.";
            sourceFiles = ("src/DemaConsulting.FileAssert/Modeling/FileAssertRule.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Modeling/FileAssertRuleTests.cs");
            designFiles = ("docs/design/file-assert/modeling/file-assert-rule.md");
            verificationFiles = ("docs/verification/file-assert/modeling/file-assert-rule.md");
            requirementFiles = ("docs/reqstream/file-assert/modeling/file-assert-rule.yaml");
        }
    }
}
