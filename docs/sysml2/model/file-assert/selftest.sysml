package FileAssert {
    part def SelfTestSubsystem {
        doc /* Self-validation test runner exercised at startup for OTS/BuildMark self-check evidence. */

        comment testRef /* Test: test/DemaConsulting.FileAssert.Tests/SelfTest/ValidationTests.cs */
        comment designRef /* Design: docs/design/file-assert/selftest.md */
        comment verificationRef /* Verification: docs/verification/file-assert/selftest.md */
        comment reqRef /* Requirements: docs/reqstream/file-assert/selftest.yaml */

        part validation : Validation;
    }
}
