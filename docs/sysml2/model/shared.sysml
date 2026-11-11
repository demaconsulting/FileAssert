package SharedDependencies {
    private import SoftwareItemMetadata::*;

    doc /* Shared Packages consumed by FileAssert. */

    part def FileAssertSharedPackage {
        doc /* Shared Package: an earlier FileAssert release consumed as a CI dependency of this repository's own build pipeline. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/shared/fileassert.md");
            verificationFiles = ("docs/verification/shared/fileassert.md");
            requirementFiles = ("docs/reqstream/shared/fileassert.yaml");
        }
    }
}
