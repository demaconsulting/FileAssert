package OtsDependencies {
    private import SoftwareItemMetadata::*;

    doc /* Off-the-shelf (OTS) dependencies used by FileAssert. */

    part def YamlDotNet {
        doc /* OTS: YamlDotNet. */

        metadata info : SoftwareInfo {
            description = "OTS: YamlDotNet.";
            designFiles = ("docs/design/ots/yamldotnet.md");
            verificationFiles = ("docs/verification/ots/yamldotnet.md");
            requirementFiles = ("docs/reqstream/ots/yamldotnet.yaml");
        }
    }

    part def PdfPig {
        doc /* OTS: PdfPig. */

        metadata info : SoftwareInfo {
            description = "OTS: PdfPig.";
            designFiles = ("docs/design/ots/pdfpig.md");
            verificationFiles = ("docs/verification/ots/pdfpig.md");
            requirementFiles = ("docs/reqstream/ots/pdfpig.yaml");
        }
    }

    part def HtmlAgilityPack {
        doc /* OTS: HtmlAgilityPack. */

        metadata info : SoftwareInfo {
            description = "OTS: HtmlAgilityPack.";
            designFiles = ("docs/design/ots/htmlagilitypack.md");
            verificationFiles = ("docs/verification/ots/htmlagilitypack.md");
            requirementFiles = ("docs/reqstream/ots/htmlagilitypack.yaml");
        }
    }

    part def FileSystemGlobbing {
        doc /* OTS: FileSystemGlobbing. */

        metadata info : SoftwareInfo {
            description = "OTS: FileSystemGlobbing.";
            designFiles = ("docs/design/ots/filesystemglobbing.md");
            verificationFiles = ("docs/verification/ots/filesystemglobbing.md");
            requirementFiles = ("docs/reqstream/ots/filesystemglobbing.yaml");
        }
    }
}
