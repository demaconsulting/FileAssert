package FileAssert_CliSubsystem {
    public import FileAssert_IContext::*;
    public import FileAssert_Context::*;
    public import FileAssert_HelpCommand::*;
    private import SoftwareItemMetadata::*;

    part def CliSubsystem {
        doc /* Command-line argument parsing, output-reporting contract, and built-in help command. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Cli/CliTests.cs");
            designFiles = ("docs/design/file-assert/cli.md");
            verificationFiles = ("docs/verification/file-assert/cli.md");
            requirementFiles = ("docs/reqstream/file-assert/cli.yaml");
        }

        part iContext : IContext;
        part context : Context;
        part helpCommand : HelpCommand;
    }
}
