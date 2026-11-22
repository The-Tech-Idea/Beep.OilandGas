using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Shell;

/// <summary>
/// The notification centre: every message this session raised, newest first, with the failure reference
/// so it can be copied. It reads the pipeline's history and never creates a message of its own.
/// </summary>
public sealed class NotificationCenterWindow : Window
{
    private readonly OgNotificationHub _hub;
    private readonly ILocProvider _loc;

    public NotificationCenterWindow(OgNotificationHub hub, ILocProvider loc)
    {
        _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        _loc = loc ?? throw new ArgumentNullException(nameof(loc));

        Title = _loc.Get("Notifications.Title");
        Width = 720;
        Height = 480;
        MinWidth = 480;
        MinHeight = 320;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush("BeepBackgroundBrush", Brushes.White);
        FlowDirection = _loc.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        Content = BuildContent();
    }

    private FrameworkElement BuildContent()
    {
        var rows = _hub.HistoryItems
            .OrderByDescending(item => item.RaisedAt)
            .Select(item => new NotificationRow(
                item.RaisedAt.ToString("HH:mm:ss"),
                TitleFor(item.Toast.Kind),
                _loc.Get(item.Toast.MessageKey, item.Toast.Args ?? []),
                item.Toast.Reference ?? string.Empty))
            .ToList();

        var grid = new DataGrid
        {
            AutoGenerateColumns = true,
            IsReadOnly = true,
            ItemsSource = rows,
            Margin = new Thickness(12),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
        };

        var empty = new TextBlock
        {
            Text = _loc.Get("Notifications.Empty"),
            Margin = new Thickness(16),
            Foreground = Brush("BeepTextSecondaryBrush", Brushes.Gray),
            Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed,
        };

        var close = new Button
        {
            Content = _loc.Get("Notifications.Close"),
            Padding = new Thickness(14, 6, 14, 6),
            Margin = new Thickness(0, 0, 12, 12),
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 96,
        };
        close.Click += (_, _) => Close();

        var header = new TextBlock
        {
            Text = _loc.Get("Notifications.Count", rows.Count),
            Margin = new Thickness(16, 12, 16, 0),
            FontWeight = FontWeights.SemiBold,
            Foreground = Brush("BeepTextPrimaryBrush", Brushes.Black),
        };

        var panel = new DockPanel();
        DockPanel.SetDock(header, Dock.Top);
        DockPanel.SetDock(close, Dock.Bottom);
        panel.Children.Add(header);
        panel.Children.Add(empty);
        panel.Children.Add(close);
        panel.Children.Add(grid);

        return panel;
    }

    private string TitleFor(OgToastKind kind) => kind switch
    {
        OgToastKind.Success => _loc.Get("Toast.Done"),
        OgToastKind.Warning => _loc.Get("Toast.Attention"),
        OgToastKind.Failure => _loc.Get("Toast.Wrong"),
        OgToastKind.Audit => _loc.Get("Toast.Recorded"),
        OgToastKind.Info => _loc.Get("Toast.Information"),
    };

    private Brush Brush(string key, Brush fallback) =>
        TryFindResource(key) as Brush
        ?? Application.Current?.TryFindResource(key) as Brush
        ?? fallback;

    private sealed record NotificationRow(string Time, string Kind, string Message, string Reference);
}
