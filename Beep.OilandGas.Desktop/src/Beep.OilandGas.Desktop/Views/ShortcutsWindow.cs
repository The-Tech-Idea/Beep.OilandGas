using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// REL-03: the keyboard reference behind <c>F1</c>. It lists every global shortcut, so the map is discoverable
/// without documentation and without a mouse.
/// </summary>
public sealed class ShortcutsWindow : Window
{
    private static readonly (string Keys, string What)[] Shortcuts =
    [
        ("Ctrl+K", "Search and open a page (command palette)"),
        ("Ctrl+Tab", "Next page"),
        ("Ctrl+Shift+Tab", "Previous page"),
        ("Alt+Left", "Back to the previous page"),
        ("F6", "Move focus between the navigation and the page"),
        ("F1", "Show this list"),
        ("Esc", "Close a dialog or the palette"),
        ("Tab / Shift+Tab", "Move between controls"),
        ("Enter", "Accept the default action"),
    ];

    public ShortcutsWindow(OgApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        Title = application.Loc.Get("Shortcuts.Title");
        Width = 480;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        FlowDirection = application.Loc.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        AutomationProperties.SetName(this, application.Loc.Get("Shortcuts.Title"));

        var list = new ListBox
        {
            Margin = new Thickness(12),
            Focusable = true,
        };

        AutomationProperties.SetName(list, application.Loc.Get("Shortcuts.Title"));

        foreach (var (keys, what) in Shortcuts)
        {
            list.Items.Add($"{keys} — {what}");
        }

        var close = new Button
        {
            Content = application.Loc.Get("State.Close"),
            IsCancel = true,
            MinWidth = 90,
            Padding = new Thickness(12, 4, 12, 4),
            Margin = new Thickness(12, 0, 12, 12),
            HorizontalAlignment = HorizontalAlignment.Right,
        };

        AutomationProperties.SetName(close, application.Loc.Get("State.Close"));

        var panel = new DockPanel();
        DockPanel.SetDock(close, Dock.Bottom);
        panel.Children.Add(close);
        panel.Children.Add(list);

        Content = panel;
        Loaded += (_, _) => list.Focus();
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        };
    }

    /// <summary>The shortcuts shown, for the smoke run.</summary>
    public static IReadOnlyList<(string Keys, string What)> All => Shortcuts;
}
