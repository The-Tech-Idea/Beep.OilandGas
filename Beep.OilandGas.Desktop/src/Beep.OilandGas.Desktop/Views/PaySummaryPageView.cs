using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One audit row of the edit tab.</summary>
public sealed record OgEditRow(DateTime Utc, string Author, string Action, string Entity, string Field, string After, string Reason);

/// <summary>
/// ENG-10: compute pay from logs, edit the curves with an audit trail, and export.
/// </summary>
/// <remarks>
/// Editing before computing changes the answer, so an edit marks the result stale and the page says
/// "recompute" instead of showing a summary that no longer matches the log.
/// </remarks>
public sealed class PaySummaryPageView : OgPageBase
{
    private static readonly string[] Methods = ["Linear", "LarionovTertiary", "LarionovOlder", "Clavier", "Steiber"];

    private readonly OgApplication _application;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _zones;
    private readonly DataGrid _results;
    private readonly DataGrid _history;

    private readonly TextBox _vsh = new() { Text = "0.4", MinWidth = 60, Margin = new Thickness(0, 0, 8, 0) };
    private readonly TextBox _phi = new() { Text = "0.1", MinWidth = 60, Margin = new Thickness(0, 0, 8, 0) };
    private readonly TextBox _sw = new() { Text = "0.6", MinWidth = 60, Margin = new Thickness(0, 0, 8, 0) };
    private readonly ComboBox _method = new() { ItemsSource = Methods, SelectedIndex = 0, MinWidth = 150, Margin = new Thickness(0, 0, 8, 0) };

    private readonly WellModel _well;
    private readonly LogEditSession _session;
    private List<OgPayZoneInput> _zonesModel;
    private OgPaySummaryResult? _result;
    private bool _stale;

    public PaySummaryPageView(OgApplication application)
        : base(application, "paysummary")
    {
        _application = application;
        _ = application.Wellbore;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _well = OgPaySummaryWorkspace.ReferenceWell();
        _session = new LogEditSession(_well, application.Session.UserId);
        _zonesModel = OgPaySummaryWorkspace.ReferenceZones().ToList();

        _zones = Grid(
            ("Name", nameof(OgPayZoneInput.Name), 120),
            ("Top", nameof(OgPayZoneInput.Top), 90),
            ("Base", nameof(OgPayZoneInput.Base), 90));
        _zones.IsReadOnly = false;
        _zones.CanUserAddRows = false;
        Named(_zones, "Pay zones");

        _results = Grid(
            ("Zone", nameof(OgPayZoneRow.Name), 90),
            ("Gross", nameof(OgPayZoneRow.Gross), 80),
            ("Net res.", nameof(OgPayZoneRow.NetReservoir), 80),
            ("Net pay", nameof(OgPayZoneRow.NetPay), 80),
            ("N/G", nameof(OgPayZoneRow.NetToGross), 70),
            ("Porosity", nameof(OgPayZoneRow.Porosity), 80),
            ("Sw", nameof(OgPayZoneRow.Sw), 70),
            ("Vsh", nameof(OgPayZoneRow.Vsh), 70),
            ("Note", nameof(OgPayZoneRow.Note), 260));
        Named(_results, "Pay results");

        _history = Grid(
            ("When", nameof(OgEditRow.Utc), 130),
            ("Who", nameof(OgEditRow.Author), 100),
            ("Action", nameof(OgEditRow.Action), 90),
            ("Field", nameof(OgEditRow.Field), 110),
            ("After", nameof(OgEditRow.After), 300),
            ("Reason", nameof(OgEditRow.Reason), 180));
        Named(_history, "Edit history");

        Named(_vsh, "Vsh cutoff");
        Named(_phi, "Porosity cutoff");
        Named(_sw, "Sw cutoff");
        Named(_method, "Shale volume method");

        var parameters = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        parameters.Children.Add(Caption("Vsh <="));
        parameters.Children.Add(_vsh);
        parameters.Children.Add(Caption("PHI >="));
        parameters.Children.Add(_phi);
        parameters.Children.Add(Caption("Sw <="));
        parameters.Children.Add(_sw);
        parameters.Children.Add(Caption("Vsh method"));
        parameters.Children.Add(_method);
        parameters.Children.Add(Button("Compute", (_, _) => Compute()));
        parameters.Children.Add(Button("Reset zones", (_, _) => ResetZones()));
        parameters.Children.Add(Button("Export CSV", (_, _) => Export("csv")));
        parameters.Children.Add(Button("Export XLSX", (_, _) => Export("xlsx")));

        var edits = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        edits.Children.Add(Button("Shift +2 m", (_, _) => Shift()));
        edits.Children.Add(Button("Splice runs", (_, _) => Splice()));
        edits.Children.Add(Button("Undo", (_, _) => Undo()));

        var panes = new TabControl { Height = 300, Margin = new Thickness(0, 8, 0, 0), IsTabStop = true };
        panes.Items.Add(new TabItem { Header = "Zones", Content = _zones });
        panes.Items.Add(new TabItem { Header = "Results", Content = _results });
        panes.Items.Add(new TabItem { Header = "Edits", Content = new DockPanel { Children = { edits, _history } } });
        Named(panes, "Pay summary panes");

        var panel = new StackPanel();
        panel.Children.Add(parameters);
        panel.Children.Add(_summary);
        panel.Children.Add(_notice);
        panel.Children.Add(panes);

        ShowContent(panel);

        _zones.ItemsSource = _zonesModel;
        RefreshHistory();
        Compute();
    }

    /// <summary>What the page is showing; the smoke run and tests read these.</summary>
    public IReadOnlyList<OgPayZoneInput> Zones => _zonesModel;

    public IReadOnlyList<OgPayZoneRow> Rows => _result?.Zones ?? [];

    public IReadOnlyList<OgEditRow> History { get; private set; } = [];

    public OgPaySummaryResult? Result => _result;

    public string SummaryText => _summary.Text;

    public bool IsStale => _stale;

    public string? ExportedPath { get; private set; }

    /// <summary>Computes the summary from the zones and cutoffs on screen.</summary>
    public bool Compute()
    {
        var parameters = new OgPayParameters(
            new OgPayCutoffs(Number(_vsh.Text), Number(_phi.Text), Number(_sw.Text)),
            VshMethod: _method.SelectedItem as string ?? "Linear");

        var run = OgPaySummaryWorkspace.Compute(_well, _zonesModel, parameters);

        if (!run.Success || run.Value is null)
        {
            _application.AuditLog.Add("pay.compute", "well", _well.Header.Name, "refused", $"{run.Reference}: {run.Message}");
            _notice.Text = $"{run.Message}  ({run.Reference})";
            _summary.Text = "No result: the run was refused.";
            _result = null;
            _results.ItemsSource = null;
            return false;
        }

        _result = run.Value;
        _stale = false;
        _results.ItemsSource = _result.Zones;

        _summary.Text = _result.Describe() +
                        (_well.Audit.Count > 0 ? $"  ·  {_well.Audit.Count} log edit(s) before this run" : string.Empty);

        _notice.Text = _result.Warnings.Count == 0
            ? string.Empty
            : string.Join("  ·  ", _result.Warnings);

        _application.AuditLog.Add(
            "pay.compute",
            "well",
            _well.Header.Name,
            "ok",
            $"zones={_result.Zones.Count} net={_result.NetReservoir:0.###} netpay={_result.NetPay:0.###} cutoffs='{_result.Cutoffs}'");

        return true;
    }

    public void ResetZones()
    {
        _zonesModel = OgPaySummaryWorkspace.ReferenceZones().ToList();
        _zones.ItemsSource = _zonesModel;
        Compute();
    }

    /// <summary>Writes the current result as CSV or XLSX; an existing file is replaced deliberately.</summary>
    public bool Export(string format)
    {
        try
        {
            if (_result is null && !Compute())
            {
                return false;
            }

            Directory.CreateDirectory(OgPaths.ExportsDirectory);

            var path = Path.Combine(
                OgPaths.ExportsDirectory,
                $"pay-summary-{DateTime.Now:yyyyMMdd-HHmmss}.{format}");

            var written = format.Equals("xlsx", StringComparison.OrdinalIgnoreCase)
                ? OgPaySummaryWorkspace.ExportXlsx(_result!, _zonesModel, path, overwrite: true)
                : OgPaySummaryWorkspace.ExportCsv(_result!, _zonesModel, path, overwrite: true);

            if (!written.Success)
            {
                _notice.Text = $"{written.Message}  ({written.Reference})";
                return false;
            }

            ExportedPath = written.Value;
            _notice.Text = $"Exported {format.ToUpperInvariant()} · {new FileInfo(written.Value!).Length / 1024.0:0.#} KB · {written.Value}";
            _application.AuditLog.Add("pay.export", "well", _well.Header.Name, "ok", $"{format} {Path.GetFileName(written.Value!)}");
            return true;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting the pay summary",
                "no file was written",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The pay summary could not be exported.", reference);
            return false;
        }
    }

    /// <summary>Depth-shifts the first run by +2 m and records why.</summary>
    public bool Shift()
    {
        try
        {
            var set = _well.LogSets[0];
            var top = set.Index[0];

            _session.DepthShift(set.Id, [(top, top + 2)], reason: "pay summary page: shift to the reference run");

            AfterEdit("depth shift");
            return true;
        }
        catch (ArgumentException ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "shifting the log",
                "the log is unchanged",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The shift was refused.", reference);
            return false;
        }
    }

    /// <summary>Splices a second synthetic run into the well and records both runs.</summary>
    public bool Splice()
    {
        try
        {
            if (_well.LogSets.Count < 2)
            {
                _well.LogSets.Add(SecondRun(_well.LogSets[0]));
            }

            var ids = _well.LogSets.Take(2).Select(set => set.Id).ToList();
            _session.Splice(ids, reason: "pay summary page: composite of the two runs");

            AfterEdit("splice");
            return true;
        }
        catch (ArgumentException ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "splicing the runs",
                "the log is unchanged",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The splice was refused.", reference);
            return false;
        }
    }

    /// <summary>Undoes the last edit and records that it was undone.</summary>
    public bool Undo()
    {
        if (!_session.Undo())
        {
            _notice.Text = "There is no edit to undo.";
            return false;
        }

        AfterEdit("undo");
        return true;
    }

    private void AfterEdit(string what)
    {
        _stale = true;
        RefreshHistory();

        _application.AuditLog.Add(
            "logs.edit",
            "well",
            _well.Header.Name,
            "ok",
            $"{what}; author={_session.Author}; canUndo={_session.CanUndo}");

        _notice.Text = $"{what} recorded. The result is stale: recompute to use the edited log.";
        _summary.Text = $"{_well.Audit.Count} edit(s) · recompute required";
    }

    private void RefreshHistory()
    {
        History = _well.Audit
            .OrderByDescending(entry => entry.Utc)
            .Select(entry => new OgEditRow(
                entry.Utc.ToLocalTime(),
                entry.Author,
                entry.Action.ToString(),
                entry.EntityType,
                entry.Field ?? string.Empty,
                Shorten(entry.After),
                entry.Reason ?? string.Empty))
            .ToList();

        _history.ItemsSource = History;
    }

    private static string Shorten(string? text) =>
        text is null ? string.Empty : text.Length <= 80 ? text : text[..77] + "…";

    private static LogSet SecondRun(LogSet first)
    {
        var second = new LogSet
        {
            Name = "run 2 (synthetic)",
            IndexUnit = first.IndexUnit,
            Index = first.Index.Select(depth => depth + 5).ToArray(),
        };

        foreach (var curve in first.Curves)
        {
            second.Curves.Add(new LogCurveData
            {
                Mnemonic = curve.Mnemonic,
                Unit = curve.Unit,
                Description = curve.Description,
                Values = curve.Values.Select(value => value is null ? null : value + 1).ToArray(),
            });
        }

        return second;
    }

    private static double? Number(string text) =>
        double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;

    private static DataGrid Grid(params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Margin = new Thickness(0, 4, 0, 4),
        };

        foreach (var (header, path, width) in columns)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(path) { StringFormat = "{0:0.###}" },
                Width = new DataGridLength(width),
            });
        }

        return grid;
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
}
