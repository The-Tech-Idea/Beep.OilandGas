using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// PRD-05: the reconciliation of one field and month - produced, measured, allocated, sold and booked, the difference at
/// each step, and the report that proves it.
/// </summary>
/// <remarks>
/// The waterfall is arithmetically consistent by construction and the page says whether it is (AC-1). A material variance
/// needs a reason before the report can be produced, and the report is refused rather than trimmed (AC-2). The stages
/// must be in one unit or the comparison is refused with the units named (AC-3). A closed month cannot take a reason and
/// is marked closed with who closed it and when (AC-4). The month total is the sum of the day rows and the page shows both
/// (AC-5).
/// </remarks>
public sealed class ReconciliationPageView : OgPageBase
{
    public const string Route = "reconciliation";

    private readonly OgApplication _application;
    private readonly IOgReconciliation _reconciliation;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _state;
    private readonly TextBlock _waterfall;
    private readonly TextBlock _monthly;
    private readonly TextBlock _readiness;
    private readonly TextBlock _report;
    private readonly TextBlock _detail;
    private readonly TextBox _period;
    private readonly TextBox _materiality;
    private readonly DataGrid _lines;
    private readonly DataGrid _days;
    private readonly ComboBox _linePicker;
    private readonly ComboBox _stagePicker;
    private readonly ComboBox _reasonPicker;
    private readonly TextBox _note;

    public ReconciliationPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _reconciliation = application.Reconciliation;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _state = Body(string.Empty);
        _state.TextWrapping = TextWrapping.Wrap;
        _state.Margin = new Thickness(0, 4, 0, 4);

        _waterfall = Body(string.Empty);
        _waterfall.TextWrapping = TextWrapping.Wrap;
        _waterfall.Margin = new Thickness(0, 0, 0, 4);

        _monthly = Body(string.Empty);
        _monthly.TextWrapping = TextWrapping.Wrap;
        _monthly.Margin = new Thickness(0, 0, 0, 4);

        _readiness = Body(string.Empty);
        _readiness.TextWrapping = TextWrapping.Wrap;
        _readiness.Margin = new Thickness(0, 0, 0, 8);

        _report = Body(string.Empty);
        _report.TextWrapping = TextWrapping.Wrap;
        _report.Margin = new Thickness(0, 4, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _period = Field("Month yyyy-MM", 110);
        _period.Text = DateTime.Today.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        _materiality = Field("Materiality percent", 110);
        _materiality.Text = ReconciliationRules.DefaultMaterialityPercent.ToString(CultureInfo.InvariantCulture);

        _lines = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 190,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Line", "NameText", 130),
                Text("Produced", "ProducedText", 110),
                Text("Measured", "MeasuredText", 110),
                Text("Allocated", "AllocatedText", 110),
                Text("Sold", "SoldText", 110),
                Text("Booked", "BookedText", 110),
                Text("Material", "MaterialText", 80),
                Text("Reason", "ReasonText", 190),
            },
        };

        Named(_lines, "Reconciliation lines");
        _lines.SelectionChanged += (_, _) => ShowSelectedLine();

        _days = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 160,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Day", "DateText", 95),
                Text("Produced", "ProducedText", 105),
                Text("Measured", "MeasuredText", 105),
                Text("Allocated", "AllocatedText", 105),
                Text("Sold", "SoldText", 105),
                Text("Booked", "BookedText", 105),
            },
        };

        Named(_days, "Daily rows");

        _linePicker = new ComboBox { MinWidth = 170, Margin = new Thickness(0, 0, 8, 0) };
        Named(_linePicker, "Line to reason");

        _stagePicker = new ComboBox
        {
            MinWidth = 120,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = ReconciliationRules.Stages,
        };

        Named(_stagePicker, "Stage the variance is at");
        _stagePicker.SelectedIndex = 1;

        _reasonPicker = new ComboBox { MinWidth = 130, Margin = new Thickness(0, 0, 8, 0) };
        Named(_reasonPicker, "Reason code");

        _note = Field("Reason note", 200);

        var top = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        top.Children.Add(Caption("Month"));
        top.Children.Add(_period);
        top.Children.Add(Caption("Materiality %"));
        top.Children.Add(_materiality);
        top.Children.Add(Button("Refresh", (_, _) => Load()));
        top.Children.Add(Button("Build report", (_, _) => _ = BuildReportAsync()));
        top.Children.Add(Button("Close month", (_, _) => _ = CloseAsync()));

        var reasonBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        reasonBar.Children.Add(Caption("Variance reason"));
        reasonBar.Children.Add(_linePicker);
        reasonBar.Children.Add(_stagePicker);
        reasonBar.Children.Add(_reasonPicker);
        reasonBar.Children.Add(_note);
        reasonBar.Children.Add(Button("Assign reason", (_, _) => _ = AssignReasonAsync()));

        var panel = new StackPanel();
        panel.Children.Add(top);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_state);
        panel.Children.Add(Caption("Waterfall"));
        panel.Children.Add(_waterfall);
        panel.Children.Add(Caption("Totals"));
        panel.Children.Add(_monthly);
        panel.Children.Add(Caption("Export readiness"));
        panel.Children.Add(_readiness);
        panel.Children.Add(Caption("Lines"));
        panel.Children.Add(_lines);
        panel.Children.Add(reasonBar);
        panel.Children.Add(Caption("Daily rows"));
        panel.Children.Add(_days);
        panel.Children.Add(Caption("Report"));
        panel.Children.Add(_report);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgReconciliationWorkspace? Workspace { get; private set; }

    public OgReconciliationLine? SelectedLine { get; private set; }

    public OgReconciliationReport? Report { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string StateText => _state.Text;

    public string WaterfallText => _waterfall.Text;

    public string MonthlyText => _monthly.Text;

    public string ReadinessText => _readiness.Text;

    public string ReportText => _report.Text;

    public string DetailText => _detail.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _reconciliation
            .LoadAsync(session.FieldId, session.FieldName, Month(_period.Text), Materiality())
            .GetAwaiter()
            .GetResult();

        return Show(loaded);
    }

    public async Task<bool> AssignReasonAsync()
    {
        if (_linePicker.SelectedItem is not OgReconciliationLine line)
        {
            _notice.Text = "Choose the line to reason.";
            return false;
        }

        var assigned = await _reconciliation
            .AssignReasonAsync(
                new OgReconciliationReasonValues(
                    _application.Session.FieldId,
                    Month(_period.Text),
                    line.Dimension,
                    _stagePicker.SelectedItem as string,
                    _reasonPicker.SelectedItem as string,
                    _note.Text),
                _application.Session.UserId,
                _application.Session.FieldName,
                Materiality())
            .ConfigureAwait(true);

        var shown = Show(assigned);

        if (assigned.Success)
        {
            _application.AuditLog.Add(
                "production.reconciliation.reason",
                "production",
                line.Dimension,
                "ok",
                _application.Session.UserId);
        }

        return shown;
    }

    public async Task<bool> CloseAsync()
    {
        var closed = await _reconciliation
            .ClosePeriodAsync(
                Month(_period.Text) ?? DateTime.Today,
                _application.Session.FieldId,
                _application.Session.UserId,
                _application.Session.FieldName)
            .ConfigureAwait(true);

        var shown = Show(closed);

        if (closed.Success)
        {
            _application.AuditLog.Add(
                "production.reconciliation.close",
                "production",
                (_period.Text ?? string.Empty).Trim(),
                "ok",
                _application.Session.UserId);
        }

        return shown;
    }

    public async Task<bool> BuildReportAsync()
    {
        var built = await _reconciliation
            .BuildReportAsync(
                _application.Session.FieldId,
                _application.Session.FieldName,
                Month(_period.Text),
                Materiality())
            .ConfigureAwait(true);

        if (!built.Success || built.Value is null)
        {
            Report = null;
            _report.Text = string.Empty;
            _notice.Text = built.Message ?? "The report could not be produced.";
            return false;
        }

        Report = built.Value;
        _report.Text = built.Value.Text;
        _notice.Text =
            $"Report for {built.Value.PeriodLabel}: {built.Value.Lines.ToString(CultureInfo.InvariantCulture)} line(s). " +
            "The CSV is in the detail below.";
        _detail.Text = built.Value.Csv;

        return true;
    }

    private void ShowSelectedLine()
    {
        if (_lines.SelectedItem is not OgReconciliationLine line)
        {
            return;
        }

        SelectedLine = line;
        _detail.Text = line.Describe();
    }

    private bool Show(OgResult<OgReconciliationWorkspace> result)
    {
        if (!result.Success || result.Value is null)
        {
            LastFailed = true;
            Workspace = null;
            SelectedLine = null;
            _lines.ItemsSource = null;
            _days.ItemsSource = null;
            _linePicker.ItemsSource = null;
            _reasonPicker.ItemsSource = null;
            _header.Text = "Reconciliation";
            _state.Text = string.Empty;
            _waterfall.Text = string.Empty;
            _monthly.Text = string.Empty;
            _readiness.Text = string.Empty;
            _report.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = result.Message ?? "The reconciliation could not be read.";
            ShowState(OgViewState.Failed, "The reconciliation could not be read.", result.Reference);
            return false;
        }

        LastFailed = false;
        Workspace = result.Value;

        var workspace = result.Value;

        _lines.ItemsSource = workspace.Lines.ToList();
        _days.ItemsSource = workspace.Days.ToList();
        _linePicker.ItemsSource = workspace.Lines.ToList();
        _linePicker.SelectedIndex = workspace.Lines.Count > 0 ? 0 : -1;
        _reasonPicker.ItemsSource = workspace.Reasons.ToList();
        _reasonPicker.SelectedIndex = workspace.Reasons.Count > 0 ? 0 : -1;

        _header.Text = $"{workspace.FieldName} · {workspace.Summary}";

        _state.Text = workspace.IsClosed
            ? $"Closed by {workspace.ClosedBy ?? "(unknown)"} on " +
              $"{workspace.ClosedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "unknown"} · " +
              "the month is read-only."
            : "Open: variance reasons can still be assigned.";

        _notice.Text = workspace.ReasonReferenceMissing
            ? workspace.ReasonsText
            : workspace.IsEmpty
                ? "No well of this field has a measurement in the month."
                : workspace.NeedsReasonCount > 0
                    ? $"{workspace.NeedsReasonCount.ToString(CultureInfo.InvariantCulture)} material variance(s) have " +
                      "no reason; the report cannot be produced until they do."
                    : "Every material variance has a reason.";

        _waterfall.Text = workspace.WaterfallText +
                          (workspace.FieldLine is { } fieldLine
                              ? " · " + fieldLine.DifferencesText +
                                " · arithmetic consistent: " + (workspace.IsConsistent ? "yes" : "no")
                              : string.Empty);

        _monthly.Text = workspace.MonthlyText;

        _readiness.Text = workspace.ExportReady
            ? "ready: the report can be produced"
            : "blocked: " + workspace.ExportBlockedReason;

        _detail.Text = workspace.JoinKeysText;

        ShowState(workspace.IsEmpty ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    private decimal Materiality() =>
        decimal.TryParse(_materiality.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : ReconciliationRules.DefaultMaterialityPercent;

    private static DateTime? Month(string? text) =>
        DateTime.TryParse(text?.Trim() + "-01", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var plain)
                ? ReconciliationRules.MonthOf(plain)
                : null;

    private static TextBox Field(string name, double width)
    {
        var box = new TextBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = name,
        };

        Named(box, name);
        return box;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
    };

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

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}
