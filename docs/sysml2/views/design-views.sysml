// Views rendered into docs/design/generated/ for the Design document.
// Reopens the FileAssert package so each 'expose' statement can reference
// its target part def directly. Per SysML2Tools syntax, 'expose' is only valid
// inside a named 'view' usage (not a 'view def' definition), and it - not
// 'render' - is what scopes the rendered diagram content.
package FileAssert {
    view SoftwareStructureView {
        expose FileAssert;
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
