using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// FND-06: the durable failure list. It reads the local SQLite rows first and shows, for each one, whether
/// the API has it — because a reference the person quotes is only findable on the server once it does.
/// </summary>
public sealed class AdminFailuresView : OgPageBase
{
    private const int PageSize = 200;

    private readonly OgApplication _application;
    private readonly TextBlock _summary;
    private readonly DataGrid _grid;

    public AdminFailuresView(OgApplication application)
        : base(application, "failures")
    {
        _application = application;

        _summary = Body(string.Empty);
        _grid = new DataGrid
        {
            IsTabStop = true, // REL-03 AC-1: a read-only grid must be reachable by Tab.
            AutoGenerateColumns = false,
            IsReadOnly = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Margin = new Thickness(0, 8, 0, 0),
            Columns =
            {
                Text("Reference", nameof(OgFailureRow.Reference), 110),
                Text("When", nameof(OgFailureRow.OccurredAtText), 135),
                Text("Severity", nameof(OgFailureRow.Severity), 75),
                Text("Operation", nameof(OgFailureRow.Operation), 200),
                Star("Consequence", nameof(OgFailureRow.Consequence)),
                Text("Forwarding", nameof(OgFailureRow.ForwardingLabel), 120),
                Text("User", nameof(OgFailureRow.UserId), 100),
            },
        };

        var refresh = new Button
        {
            Content = "Refresh",
            Padding = new Thickness(12, 4, 12, 4),
            Margin = new Thickness(0, 0, 8, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        refresh.Click += (_, _) => Reload();

        var bar = new StackPanel { Orientation = Orientation.Horizontal };
        bar.Children.Add(refresh);

        var panel = new DockPanel();
        DockPanel.SetDock(bar, Dock.Top);
        panel.Children.Add(bar);
        DockPanel.SetDock(_summary, Dock.Top);
        panel.Children.Add(_summary);
        panel.Children.Add(_grid);

        Named(_grid, "Stored failures");
        ShowContent(panel);
        Reload();
    }

    /// <summary>The rows currently shown; the smoke run and tests read this.</summary>
    public IReadOnlyList<OgFailureRow> Rows => _grid.ItemsSource as IReadOnlyList<OgFailureRow> ?? [];

    /// <summary>REL-04 AC-3: what this grid is not showing, or null when it shows everything.</summary>
    public string? CeilingMessage { get; private set; }

    private void Reload()
    {
        try
        {
            var forwardingEnabled = _application.FailureRelay.IsEnabled;
            var rows = OgFailureRow.From(_application.Failures.Recent(PageSize), forwardingEnabled);

            _grid.ItemsSource = rows;
            CeilingMessage = Core.OgPager.RecentCeiling(rows.Count, _application.Failures.Count());

            _summary.Text = OgFailureSummary.From(rows, forwardingEnabled).Describe()
                            + "   ·   "
                            + $"outbox: {_application.FailureRelay.Pending} waiting"
                            + (CeilingMessage is null ? string.Empty : $"   ·   {CeilingMessage}");
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading the stored failures",
                "the failure list is not shown",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The stored failures could not be read.", reference);
        }
    }

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };

    private static DataGridTextColumn Star(string header, string path) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
    };
}
