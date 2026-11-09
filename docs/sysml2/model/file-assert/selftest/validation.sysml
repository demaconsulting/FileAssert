package FileAssert {
    private import SoftwareItemMetadata::*;

    part def Validation {
        doc /* Self-validation test runner. */

        metadata info : SoftwareInfo {
            description = "Self-validation test runner.";
            sourceFiles = ("src/DemaConsulting.FileAssert/SelfTest/Validation.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/SelfTest/ValidationTests.cs");
            designFiles = ("docs/design/file-assert/selftest/validation.md");
            verificationFiles = ("docs/verification/file-assert/selftest/validation.md");
            requirementFiles = ("docs/reqstream/file-assert/selftest/validation.yaml");
        }
    }
}
