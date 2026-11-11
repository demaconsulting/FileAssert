package FileAssert {
    private import SoftwareItemMetadata::*;

    part def CliSubsystem {
        doc /* Command-line argument parsing and output-reporting contract. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.FileAssert.Tests/Cli/CliTests.cs");
            designFiles = ("docs/design/file-assert/cli.md");
            verificationFiles = ("docs/verification/file-assert/cli.md");
            requirementFiles = ("docs/reqstream/file-assert/cli.yaml");
        }

        part iContext : IContext;
        part context : Context;
    }
}
