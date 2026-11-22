using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// DAT-07: runs the quality rules over a table and lists what they found, worst first, with a resolution that
/// records who closed each finding.
/// </summary>
public sealed class DataQualityPageView : OgPageBase
{
    public const string Route = "dataquality";

    private readonly OgApplication _application;
    private readonly IOgPpdmTableGateway _gateway;
    private readonly IOgQualityJournal _journal;
    private readonly TextBox _table;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _findings;

    private List<OgQualityFinding> _open = [];

    public DataQualityPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _gateway = application.TableGateway;
        _journal = application.QualityJournal;

        _table = new TextBox { MinWidth = 200, Margin = new Thickness(0, 0, 8, 0) };
        Named(_table, "Table to check");

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _findings = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 320,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Severity", nameof(OgQualityFinding.Severity), 90),
                Text("Rule", nameof(OgQualityFinding.Rule), 130),
                Text("Column", nameof(OgQualityFinding.Column), 130),
                Text("Key", nameof(OgQualityFinding.Key), 120),
                Star("Message", nameof(OgQualityFinding.Message)),
            },
        };

        Named(_findings, "Quality findings");

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Caption("Table"));
        toolbar.Children.Add(_table);
        toolbar.Children.Add(Button("Run checks", (_, _) => RunChecks()));
        toolbar.Children.Add(Button("Resolve selected", (_, _) => ResolveSelected()));
        toolbar.Children.Add(Button("Export findings", (_, _) => Export()));

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_summary);
        panel.Children.Add(_notice);
        panel.Children.Add(_findings);

        ShowContent(panel);

        if (_application.Workbench.Pending is { } pending)
        {
            _table.Text = pending.TableName;
        }
    }

    public OgQualityRun? LastRun { get; private set; }

    public IReadOnlyList<OgQualityFinding> Findings => _open;

    public string SummaryText => _summary.Text;

    public string NoticeText => _notice.Text;

    public string? ExportedPath { get; private set; }

    /// <summary>Runs the rules over one bounded page of the table.</summary>
    public bool RunChecks()
    {
        var table = _table.Text.Trim();

        if (table.Length == 0)
        {
            _notice.Text = "Name the table to check.";
            return false;
        }

        var form = _gateway.FormAsync(table).GetAwaiter().GetResult();

        if (!form.Success || form.Value is null)
        {
            LastRun = new OgQualityRun(
                table, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 0, 0,
                Succeeded: false, Findings: [], Error: form.Message ?? "The table could not be read.");

            _summary.Text = LastRun.Describe();
            _notice.Text = "The last successful run, if any, is unchanged.";
            _findings.ItemsSource = null;

            ShowState(OgViewState.Failed, "The table could not be checked.", form.Reference);
            return false;
        }

        var page = _gateway
            .QueryAsync(table, new OgQuery(Page: 0, PageSize: OgPpdmTableGateway.MaximumPageSize))
            .GetAwaiter()
            .GetResult();

        if (!page.Success || page.Value is null)
        {
            LastRun = new OgQualityRun(
                table, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 0, 0,
                Succeeded: false, Findings: [], Error: page.Message ?? "The table could not be read.");

            _summary.Text = LastRun.Describe();
            _findings.ItemsSource = null;

            ShowState(OgViewState.Failed, "The table could not be checked.", page.Reference);
            return false;
        }

        var rules = OgQualityReviewer.RulesFor(form.Value);
        var run = OgQualityReviewer.Run(form.Value, page.Value.Items, rules);

        LastRun = run;
        _journal.Record(run);
        _open = run.Findings.ToList();
        _findings.ItemsSource = _open;

        _summary.Text = run.Describe() +
                        $" (checked the first {page.Value.Items.Count} of {page.Value.Total} row(s); " +
                        $"last successful run {_journal.LastSuccess(table):yyyy-MM-dd HH:mm} UTC)";

        _notice.Text = run.HasRules
            ? "Findings are ordered worst first; a resolution records who closed it."
            : "This table has no rules to run, so the empty list is not a clean bill of health.";

        _application.AuditLog.Add(
            "data.quality",
            "table",
            table,
            run.HasFindings ? "findings" : "ok",
            $"rules={run.RulesApplied} rows={run.RowsScanned} findings={run.Findings.Count}");

        ShowState(OgViewState.Ready, string.Empty);
        return true;
    }

    /// <summary>Records who resolved the selected finding (AC-4).</summary>
    public bool ResolveSelected()
    {
        if (_findings.SelectedItem is not OgQualityFinding finding)
        {
            _notice.Text = "Select a finding to resolve.";
            return false;
        }

        var resolved = OgQualityReviewer.Resolve(
            finding,
            _application.Session.UserId,
            "Resolved from the data quality page.");

        if (!resolved.Success)
        {
            _notice.Text = resolved.Message ?? "The finding was not resolved.";
            return false;
        }

        _open.Remove(finding);
        _findings.ItemsSource = null;
        _findings.ItemsSource = _open;

        _notice.Text = $"Resolved by {resolved.Value!.Actor} at {resolved.Value.ResolvedAtUtc:HH:mm}.";

        _application.AuditLog.Add(
            "data.quality.resolve",
            "finding",
            finding.Id,
            "ok",
            $"rule={finding.Rule} key={finding.Key} actor={resolved.Value.Actor}");

        return true;
    }

    /// <summary>Writes the findings as CSV, so a person can work through them elsewhere.</summary>
    public bool Export()
    {
        if (_open.Count == 0)
        {
            _notice.Text = "There is nothing to export.";
            return false;
        }

        try
        {
            Directory.CreateDirectory(OgPaths.ExportsDirectory);

            var path = Path.Combine(
                OgPaths.ExportsDirectory,
                $"quality-{_table.Text.Trim()}-{DateTime.Now:yyyyMMdd-HHmmss}.csv");

            var builder = new StringBuilder("severity,rule,column,key,message\n");

            foreach (var finding in _open)
            {
                builder.Append(finding.Severity).Append(',')
                    .Append(finding.Rule).Append(',')
                    .Append(finding.Column ?? string.Empty).Append(',')
                    .Append('"').Append(finding.Key.Replace("\"", "\"\"")).Append('"').Append(',')
                    .Append('"').Append(finding.Message.Replace("\"", "\"\"")).Append('"')
                    .Append('\n');
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
            ExportedPath = path;
            _notice.Text = $"{_open.Count} finding(s) written to {path}.";
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "writing the quality findings",
                "no file was written",
                FailureSeverity.Error);

            _notice.Text = $"The findings could not be written. Reference: {reference}";
            return false;
        }
    }

    private static Button Button(string text, RoutedEventHandler onClick)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(10, 3, 10, 3),
            Margin = new Thickness(0, 0, 8, 0),
        };

        button.Click += onClick;
        return button;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
    };

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new System.Windows.Data.Binding(path),
        Width = new DataGridLength(width),
    };

    private static DataGridTextColumn Star(string header, string path) => new()
    {
        Header = header,
        Binding = new System.Windows.Data.Binding(path),
        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
    };
}
