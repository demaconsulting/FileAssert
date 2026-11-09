package FileAssert {
    private import SoftwareItemMetadata::*;

    part def SelfTestSubsystem {
        doc /* Self-validation test runner exercised at startup for OTS/BuildMark self-check evidence. */

        metadata info : SoftwareInfo {
            description = "Self-validation test runner exercised at startup for OTS/BuildMark self-check evidence.";
            testFiles = ("test/DemaConsulting.FileAssert.Tests/SelfTest/ValidationTests.cs");
            designFiles = ("docs/design/file-assert/selftest.md");
            verificationFiles = ("docs/verification/file-assert/selftest.md");
            requirementFiles = ("docs/reqstream/file-assert/selftest.yaml");
        }

        part validation : Validation;
    }
}
