package FileAssert {
    private import SoftwareItemMetadata::*;

    part def IContext {
        doc /* Output contract interface for reporting assertion results and errors. */

        metadata info : SoftwareInfo {
            description = "Output contract interface for reporting assertion results and errors.";
            sourceFiles = ("src/DemaConsulting.FileAssert/Cli/IContext.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Cli/ContextTests.cs");
            designFiles = ("docs/design/file-assert/cli/i-context.md");
            verificationFiles = ("docs/verification/file-assert/cli/i-context.md");
            requirementFiles = ("docs/reqstream/file-assert/cli/i-context.yaml");
        }
    }
}
