package FileAssert {
    part def CliSubsystem {
        doc /* Command-line argument parsing and output-reporting contract. */

        comment testRef /* Test: test/DemaConsulting.FileAssert.Tests/Cli/CliTests.cs */
        comment designRef /* Design: docs/design/file-assert/cli.md */
        comment verificationRef /* Verification: docs/verification/file-assert/cli.md */
        comment reqRef /* Requirements: docs/reqstream/file-assert/cli.yaml */

        part iContext : IContext;
        part context : Context;
    }
}
