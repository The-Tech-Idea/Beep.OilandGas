using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// REL-03 AC-7: the command palette. Opened with <c>Ctrl+K</c>, filtered as you type, moved with the arrow
/// keys, accepted with Enter and dismissed with Esc — no mouse anywhere in the flow.
/// </summary>
public sealed class CommandPaletteWindow : Window
{
    private readonly IReadOnlyList<OgPaletteEntry> _entries;
    private readonly TextBox _filter = new();
    private readonly ListBox _list = new();

    public CommandPaletteWindow(OgApplication application, IReadOnlyList<OgPaletteEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(entries);

        _entries = entries;

        Title = application.Loc.Get("Palette.Title");
        Width = 520;
        Height = 380;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        FlowDirection = application.Loc.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        AutomationProperties.SetName(this, application.Loc.Get("Palette.Title"));

        _filter.Margin = new Thickness(12, 12, 12, 8);
        _filter.Padding = new Thickness(6, 4, 6, 4);
        AutomationProperties.SetName(_filter, application.Loc.Get("Palette.Search"));
        AutomationProperties.SetHelpText(_filter, application.Loc.Get("Palette.SearchHelp"));
        _filter.TextChanged += (_, _) => Apply(_filter.Text);
        _filter.PreviewKeyDown += OnFilterKey;

        _list.Margin = new Thickness(12, 0, 12, 12);
        _list.DisplayMemberPath = nameof(OgPaletteEntry.Title);
        AutomationProperties.SetName(_list, application.Loc.Get("Palette.Pages"));
        _list.MouseDoubleClick += (_, _) => Accept();

        var panel = new DockPanel();
        DockPanel.SetDock(_filter, Dock.Top);
        panel.Children.Add(_filter);
        panel.Children.Add(_list);

        Content = panel;

        Loaded += (_, _) =>
        {
            Apply(string.Empty);
            _filter.Focus();
        };
    }

    /// <summary>The route the person accepted, or null when the palette was dismissed.</summary>
    public string? SelectedRoute { get; private set; }

    /// <summary>The entries currently offered, for the smoke run and tests.</summary>
    public IReadOnlyList<OgPaletteEntry> Results { get; private set; } = [];

    /// <summary>Filters the list and keeps a sensible selection.</summary>
    public void Apply(string? query)
    {
        Results = OgCommandPalette.Filter(_entries, query);
        _list.ItemsSource = Results;
        _list.SelectedIndex = Results.Count > 0 ? 0 : -1;
    }

    /// <summary>Moves the selection by <paramref name="delta"/>, wrapping.</summary>
    public void Move(int delta)
    {
        if (Results.Count == 0)
        {
            return;
        }

        _list.SelectedIndex = OgCommandPalette.Move(_list.SelectedIndex, delta, Results.Count);
        _list.ScrollIntoView(_list.SelectedItem);
    }

    /// <summary>Accepts the current selection; returns false when there is nothing to accept.</summary>
    public bool Accept()
    {
        var entry = OgCommandPalette.Selected(Results, _list.SelectedIndex);

        if (entry is null)
        {
            return false;
        }

        SelectedRoute = entry.Route;
        DialogResult = true;
        return true;
    }

    private void OnFilterKey(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down:
                Move(1);
                e.Handled = true;
                break;

            case Key.Up:
                Move(-1);
                e.Handled = true;
                break;

            case Key.Enter:
                Accept();
                e.Handled = true;
                break;

            case Key.Escape:
                DialogResult = false;
                e.Handled = true;
                break;
        }
    }
}
