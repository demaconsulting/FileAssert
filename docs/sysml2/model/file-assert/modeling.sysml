package FileAssert {
    part def ModelingSubsystem {
        doc /* Test-suite data model and content-assertion rule implementations. */

        comment testRef /* Test: test/DemaConsulting.FileAssert.Tests/Modeling/ModelingTests.cs */
        comment designRef /* Design: docs/design/file-assert/modeling.md */
        comment verificationRef /* Verification: docs/verification/file-assert/modeling.md */
        comment reqRef /* Requirements: docs/reqstream/file-assert/modeling.yaml */

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
