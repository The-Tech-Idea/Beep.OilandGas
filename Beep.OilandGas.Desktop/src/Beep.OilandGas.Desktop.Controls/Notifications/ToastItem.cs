using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Controls.Notifications;

/// <summary>
/// One toast on screen: the already-localised text, its kind, the failure reference and the actions.
/// Built by the notification hub; the view never localises or decides severity.
/// </summary>
public sealed class ToastItem
{
    public ToastItem(OgToast toast, string text, string titleText)
    {
        Toast = toast;
        Text = text;
        TitleText = titleText;
    }

    public OgToast Toast { get; }

    public string Text { get; }

    public string TitleText { get; }

    public OgToastKind Kind => Toast.Kind;

    public string? Reference => Toast.Reference;

    public bool HasReference => !string.IsNullOrWhiteSpace(Reference);

    /// <summary>Critical failures must be dismissed deliberately.</summary>
    public bool IsSticky => Kind == OgToastKind.Failure
                            && Toast.Duration is null;

    public TimeSpan Duration => Toast.Duration ?? TimeSpan.FromSeconds(Kind switch
    {
        OgToastKind.Failure => 10,
        OgToastKind.Warning => 6,
        OgToastKind.Success => 3,
        OgToastKind.Audit => 3,
        OgToastKind.Info => 3,
    });

    public string Glyph => Kind switch
    {
        OgToastKind.Success => "\uE73E",
        OgToastKind.Warning => "\uE7BA",
        OgToastKind.Failure => "\uEA39",
        OgToastKind.Audit => "\uE9D9",
        OgToastKind.Info => "\uE946",
    };

    public string AccentKey => Kind switch
    {
        OgToastKind.Success => "BeepSuccessBrush",
        OgToastKind.Warning => "BeepWarningBrush",
        OgToastKind.Failure => "BeepDangerBrush",
        OgToastKind.Audit => "BeepTextSecondaryBrush",
        OgToastKind.Info => "BeepInfoBrush",
    };

    public string SurfaceKey => Kind switch
    {
        OgToastKind.Success => "BeepSuccessSurfaceBrush",
        OgToastKind.Warning => "BeepWarningSurfaceBrush",
        OgToastKind.Failure => "BeepDangerSurfaceBrush",
        OgToastKind.Audit => "BeepSurfaceAltBrush",
        OgToastKind.Info => "BeepInfoSurfaceBrush",
    };

    /// <summary>Assertive for failures so a screen reader interrupts; polite otherwise.</summary>
    public System.Windows.Automation.AutomationLiveSetting LiveSetting =>
        Kind == OgToastKind.Failure
            ? System.Windows.Automation.AutomationLiveSetting.Assertive
            : System.Windows.Automation.AutomationLiveSetting.Polite;

    internal bool IsDismissed { get; set; }
}
