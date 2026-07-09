package FileAssert {
    import OtsDependencies::*;
    import SharedDependencies::*;

    doc
    /*
     * FileAssert is a .NET command-line application for asserting file properties
     * using YAML-defined test suites.
     */

    part def FileAssertSystem {
        doc /* The FileAssert system as a whole. */

        comment testRef /* Test: test/DemaConsulting.FileAssert.Tests/IntegrationTests.cs */
        comment designRef /* Design: docs/design/file-assert.md */
        comment verificationRef /* Verification: docs/verification/file-assert.md */
        comment reqRef /* Requirements: docs/reqstream/file-assert.yaml */

        part program : Program;
        part cli : CliSubsystem;
        part configuration : ConfigurationSubsystem;
        part modeling : ModelingSubsystem;
        part utilities : UtilitiesSubsystem;
        part selfTest : SelfTestSubsystem;

        part yamlDotNet : YamlDotNet;
        part pdfPig : PdfPig;
        part htmlAgilityPack : HtmlAgilityPack;
        part fileSystemGlobbing : FileSystemGlobbing;

        part fileAssertSharedPackage : FileAssertSharedPackage;
    }
}
