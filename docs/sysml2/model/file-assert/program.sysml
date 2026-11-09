package FileAssert {
    private import SoftwareItemMetadata::*;

    part def Program {
        doc /* Entry point and execution orchestrator. */

        metadata info : SoftwareInfo {
            description = "Entry point and execution orchestrator.";
            sourceFiles = ("src/DemaConsulting.FileAssert/Program.cs");
            testFiles = ("test/DemaConsulting.FileAssert.Tests/ProgramTests.cs");
            designFiles = ("docs/design/file-assert/program.md");
            verificationFiles = ("docs/verification/file-assert/program.md");
            requirementFiles = ("docs/reqstream/file-assert/program.yaml");
        }
    }
}
