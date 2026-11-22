using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// FND-06: the business audit list. One row per write and one per refusal, read from the local store, with
/// who did it and when.
/// </summary>
public sealed class AuditLogView : OgPageBase
{
    private const int PageSize = 200;

    private readonly OgApplication _application;
    private readonly TextBlock _summary;
    private readonly DataGrid _grid;

    public AuditLogView(OgApplication application)
        : base(application, "audit")
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
                Text("When", nameof(OgAuditRow.AtText), 135),
                Text("Verb", nameof(OgAuditRow.Verb), 170),
                Text("Subject", nameof(OgAuditRow.SubjectKind), 90),
                Text("Id", nameof(OgAuditRow.SubjectId), 120),
                Text("Outcome", nameof(OgAuditRow.Outcome), 85),
                Star("Detail", nameof(OgAuditRow.Detail)),
                Text("User", nameof(OgAuditRow.UserId), 100),
            },
        };

        var refresh = new Button
        {
            Content = "Refresh",
            Padding = new Thickness(12, 4, 12, 4),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        refresh.Click += (_, _) => Reload();

        var panel = new DockPanel();
        DockPanel.SetDock(refresh, Dock.Top);
        panel.Children.Add(refresh);
        DockPanel.SetDock(_summary, Dock.Top);
        panel.Children.Add(_summary);
        panel.Children.Add(_grid);

        Named(_grid, "Audit rows");
        ShowContent(panel);
        Reload();
    }

    /// <summary>The rows currently shown; the smoke run and tests read this.</summary>
    public IReadOnlyList<OgAuditRow> Rows => _grid.ItemsSource as IReadOnlyList<OgAuditRow> ?? [];

    /// <summary>REL-04 AC-3: what this grid is not showing, or null when it shows everything.</summary>
    public string? CeilingMessage { get; private set; }

    private void Reload()
    {
        try
        {
            var rows = OgAuditRow.From(_application.Audit.Recent(PageSize));
            var refusals = rows.Count(row => string.Equals(row.Outcome, "refused", StringComparison.OrdinalIgnoreCase));

            _grid.ItemsSource = rows;
            CeilingMessage = Core.OgPager.RecentCeiling(rows.Count, _application.Audit.Count());

        _summary.Text = $"{rows.Count} row(s) · {refusals} refusal(s)" +
                        (CeilingMessage is null ? string.Empty : $"  ·  {CeilingMessage}");
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading the audit trail",
                "the audit list is not shown",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The audit trail could not be read.", reference);
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
