package FileAssert {
    public import FileAssert_Program::*;
    public import FileAssert_CliSubsystem::*;
    public import FileAssert_ConfigurationSubsystem::*;
    public import FileAssert_ModelingSubsystem::*;
    public import FileAssert_UtilitiesSubsystem::*;
    public import FileAssert_SelfTestSubsystem::*;
    private import OtsDependencies::*;
    private import SharedPackages::*;
    private import SoftwareItemMetadata::*;

    doc
    /*
     * FileAssert is a .NET command-line application for asserting file properties
     * using YAML-defined test suites.
     */

    part def FileAssertSystem {
        doc /* The FileAssert system as a whole. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.FileAssert.Tests/IntegrationTests.cs");
            designFiles = ("docs/design/file-assert.md");
            verificationFiles = ("docs/verification/file-assert.md");
            requirementFiles = ("docs/reqstream/file-assert.yaml");
        }

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
