package FileAssert {
    private import SoftwareItemMetadata::*;

    part def Context {
        doc /* Command-line argument parser and I/O owner implementing IContext. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.FileAssert/Cli/Context.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Cli/ContextTests.cs");
            designFiles = ("docs/design/file-assert/cli/context.md");
            verificationFiles = ("docs/verification/file-assert/cli/context.md");
            requirementFiles = ("docs/reqstream/file-assert/cli/context.yaml");
        }
    }
}
