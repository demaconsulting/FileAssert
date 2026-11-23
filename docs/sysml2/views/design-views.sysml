// Views rendered into docs/design/generated/ for the Design document.
// Imports the FileAssert facade package so each 'expose' statement can reference
// its target part def directly. Per SysML2Tools syntax, 'expose' is only valid
// inside a named 'view' usage (not a 'view def' definition), and it - not
// 'render' - is what scopes the rendered diagram content.
package FileAssertDesignViews {
    private import StandardViewDefinitions::*;
    private import FileAssert::*;

    view SoftwareStructureView : InterconnectionView {
        expose FileAssert::FileAssertSystem;
    }

    view FileAssertView {
        expose FileAssertSystem;
    }

    view CliView {
        expose CliSubsystem;
    }

    view ConfigurationView {
        expose ConfigurationSubsystem;
    }

    view ModelingView {
        expose ModelingSubsystem;
    }

    view UtilitiesView {
        expose UtilitiesSubsystem;
    }

    view SelfTestView {
        expose SelfTestSubsystem;
    }
}
