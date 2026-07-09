package FileAssert {
    part def ConfigurationSubsystem {
        doc /* Top-level configuration loading, deserialization, and test execution. */

        comment testRef /* Test: test/DemaConsulting.FileAssert.Tests/Configuration/ConfigurationTests.cs */
        comment designRef /* Design: docs/design/file-assert/configuration.md */
        comment verificationRef /* Verification: docs/verification/file-assert/configuration.md */
        comment reqRef /* Requirements: docs/reqstream/file-assert/configuration.yaml */

        part fileAssertConfig : FileAssertConfig;
        part fileAssertData : FileAssertData;
    }
}
